using System.Collections.Generic;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    // Niezalezna edycja kosci elementow ubrania (buty, rekawiczki, nakrycia glowy, skarpety itp.).
    //
    // Problem: TBodySkin.CopyTrans() co klatke kopiuje localPosition/localRotation/localScale z ciala
    // do kosci slotu (Bip01 ...), wiec reczna zmiana takiej kosci jest natychmiast nadpisywana.
    //
    // Rozwiazanie: dla kazdej kosci szkieletu (Bip01 ...) uzywanej przez siatke slotu tworzymy kosc-zastepnik
    // (proxy, nazwa = oryginal + "_ShoeAdj"). Proxy startuje z ta sama macierza swiata co oryginal, wiec wyglad
    // sie nie zmienia, a SkinnedMeshRenderer.bones dostaje proxy zamiast oryginalu. Gra nie dotyka proxy,
    // wiec zmiany zostaja, a proxy podaza za oryginalem, bo jest jego dzieckiem.
    //  - proxy bez przodka wsrod uzywanych kosci: dziecko prawdziwej kosci (poza identycznosciowa),
    //  - proxy z przodkiem wsrod uzywanych kosci (np. palec -> dlon): dziecko proxy przodka, a jego poza
    //    wzgledem przodka jest kopiowana z prawdziwych kosci co klatke (LocalPoseCopier).
    // Sufiks "_ShoeAdj" zostaje ze wzgledu na zgodnosc z juz zapisanymi presetami i zestawami.
    internal class ShoeProxyMarker : MonoBehaviour {
        public SkinnedMeshRenderer[] renderers;
        public Transform[][] originalBones;
        // tablice kosci przypisane do renderers (z zastepnikami) - do wykrycia, ze gra podmienila smr.bones
        public Transform[][] proxyBones;
        public List<GameObject> proxies = new List<GameObject>();
        // pary (wezel edytowalny, prawdziwa kosc) - do pomiaru odchylenia
        public List<KeyValuePair<Transform, Transform>> nodes = new List<KeyValuePair<Transform, Transform>>();

        float nextReport;
        int healCount;
        // ustawiane w Disable(): Destroy(marker) jest odroczone do konca klatki, a kontrola przed renderowaniem nie moze wtedy "naprawiac" kosci
        public bool suspended;

        float createdAt;
        bool seenBeforeRender, fallbackLogged;

        void OnEnable() { createdAt = Time.realtimeSinceStartup; Application.onBeforeRender += OnBeforeRender; }
        void OnDisable() { Application.onBeforeRender -= OnBeforeRender; }

        void OnBeforeRender() {
            seenBeforeRender = true;
            Check();
        }

        // Gdyby Application.onBeforeRender nie dzialalo w tej grze, po 3 s kontrola przechodzi na LateUpdate (i zapisuje to w logu).
        void LateUpdate() {
            if (seenBeforeRender || Time.realtimeSinceStartup < createdAt + 3f) return;
            if (!fallbackLogged) {
                fallbackLogged = true;
                Log.Warn("independent bones: Application.onBeforeRender did not fire on '" + name + "', checking in LateUpdate instead");
            }
            Check();
        }

        // Tuz przed renderowaniem (po wszystkich aktualizacjach gry): 1) samonaprawa smr.bones, 2) pomiar odchylenia zastepnikow.
        void Check() {
            if (suspended || renderers == null || proxyBones == null) return;

            bool healed = false;
            for (int r = 0; r < renderers.Length; r++) {
                SkinnedMeshRenderer smr = renderers[r];
                if (!smr || proxyBones[r] == null) continue;
                Transform[] current = smr.bones;
                bool differ = current.Length != proxyBones[r].Length;
                for (int i = 0; !differ && i < current.Length; i++) {
                    if (current[i] != proxyBones[r][i]) differ = true;
                }
                if (differ) { smr.bones = proxyBones[r]; healed = true; }
            }
            if (healed && healCount++ < 5) Log.Warn("independent bones: the game replaced smr.bones on '" + name + "' - proxies restored (" + healCount + ")");

            float now = Time.realtimeSinceStartup;
            if (now < nextReport) return;

            System.Text.StringBuilder sb = null;
            foreach (KeyValuePair<Transform, Transform> pair in nodes) {
                Transform edit = pair.Key, real = pair.Value;
                if (!edit || !real) continue;
                // gdy przodkowie nie maja korekt: swiat zastepnika = swiat prawdziwej kosci * lokalna korekta
                Matrix4x4 expected = real.localToWorldMatrix * Matrix4x4.TRS(edit.localPosition, edit.localRotation, edit.localScale);
                Matrix4x4 actual = edit.localToWorldMatrix;
                float pos = ((Vector3)actual.GetColumn(3) - (Vector3)expected.GetColumn(3)).magnitude;
                float ang = Quaternion.Angle(edit.rotation, real.rotation * edit.localRotation);
                if (pos > 0.01f || ang > 2f) {
                    if (sb == null) sb = new System.Text.StringBuilder();
                    sb.Append(" [").Append(edit.name).Append(": pos ").Append(pos.ToString("F3")).Append(" m, rot ").Append(ang.ToString("F1")).Append(" deg]");
                }
            }

            if (sb != null) {
                nextReport = now + 3f;
                Log.Warn("independent bones drift on '" + name + "' (proxy vs real bone, assuming unedited ancestors):" + sb);
            }
        }
    }

    // Zapamietane pozy edytowalnych zastepnikow po wylaczeniu (zyje na obiekcie, ginie razem z nim).
    // Dzieki temu ponowne wlaczenie Independent bones przywraca wczesniejsze korekty.
    internal class ShoeProxyStash : MonoBehaviour {
        public struct Pose {
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
        }
        public Dictionary<string, Pose> Poses = new Dictionary<string, Pose>();
    }

    // Ustawia proxy wzgledem proxy przodka tak, aby jego macierz swiata odpowiadala prawdziwej kosci
    // (pozycja/rotacja/skala kosci "source" wyrazona w ukladzie prawdziwego przodka "reference").
    [DefaultExecutionOrder(10000)]
    internal class LocalPoseCopier : MonoBehaviour {
        public Transform source;
        public Transform reference;

        void LateUpdate() {
            Apply();
        }

        // Tuz przed renderowaniem, zeby kolejnosc aktualizacji wzgledem gry (animacje, IK, CopyTrans) nie miala znaczenia.
        void OnEnable() { Application.onBeforeRender += Apply; }
        void OnDisable() { Application.onBeforeRender -= Apply; }

        public void Apply() {
            if (!source || !reference) return;
            Matrix4x4 m = reference.worldToLocalMatrix * source.localToWorldMatrix;
            Vector3 right = m.GetColumn(0);
            Vector3 up = m.GetColumn(1);
            Vector3 forward = m.GetColumn(2);
            transform.localPosition = m.GetColumn(3);
            if (forward.sqrMagnitude > 1e-10f && up.sqrMagnitude > 1e-10f) {
                transform.localRotation = Quaternion.LookRotation(forward, up);
            }
            transform.localScale = new Vector3(right.magnitude, up.magnitude, forward.magnitude);
        }
    }

    internal static class ShoeProxy {
        public const string Suffix = "_ShoeAdj";
        // Wezel posredni, ktory tylko podaza za prawdziwa kosc (kopiuje poze); wezel edytowalny (Suffix) jest jego dzieckiem,
        // dzieki czemu LocalPoseCopier nie nadpisuje zmian uzytkownika.
        public const string FollowSuffix = "_ShoeFollow";

        // Kosc szkieletu, ktora gra nadpisuje co klatke (Bip01 ...), i ktora nie jest juz naszym proxy.
        public static bool IsProxyable(Transform t) {
            if (!t) return false;
            string n = t.name;
            if (n.EndsWith(Suffix) || n.EndsWith(FollowSuffix)) return false;
            return n.StartsWith("Bip01");
        }

        // Dlugosc, z jaka rysujemy kosc-zastepnik bez dzieci (zeby bylo ja widac i bylo sensownie dobrane do czesci ciala).
        public static float DisplayLength(string boneName) {
            if (boneName.Contains("Finger")) return 0.03f;
            if (boneName.Contains("Toe")) return 0.05f;
            if (boneName.Contains("Foot")) return 0.12f;
            if (boneName.Contains("Hand")) return 0.08f;
            if (boneName.Contains("Head")) return 0.12f;
            return 0.06f;
        }

        // Przy nakladaniu zapisanych danych pomijamy wpisy, ktorych nie wolno zapisywac na kosciach:
        //  - wezly *_ShoeFollow (co klatke nadpisuje je LocalPoseCopier),
        //  - prawdziwe kosci, gdy dane zawieraja korekty *_ShoeAdj: ich poza to chwilowy stan sterowany przez gre/animacje, a wpisanie
        //    go z powrotem zamraza kosci w pozie z chwili zapisu i po zmianie pozy postaci przedmiot "rozjezdza sie".
        public static bool SkipOnApply(string boneName, bool dataHasProxyBones) {
            if (boneName == null) return true;
            if (boneName.EndsWith(FollowSuffix)) return true;
            return dataHasProxyBones && !boneName.EndsWith(Suffix);
        }

        // Ustawia lokalna poze transformacji z macierzy (pozycja, obrot, skala). Przy macierzach prawie "czystych" (male korekty)
        // blad rozkladu jest pomijalny.
        public static void SetLocalFromMatrix(Transform t, Matrix4x4 m) {
            Vector3 right = m.GetColumn(0);
            Vector3 up = m.GetColumn(1);
            Vector3 forward = m.GetColumn(2);
            t.localPosition = m.GetColumn(3);
            if (up.sqrMagnitude > 1e-10f && forward.sqrMagnitude > 1e-10f) t.localRotation = Quaternion.LookRotation(forward, up);
            t.localScale = new Vector3(right.magnitude, up.magnitude, forward.magnitude);
        }

        public static bool IsActive(GameObject obj) {
            return obj && obj.GetComponent<ShoeProxyMarker>() != null;
        }

        public static bool CanApply(GameObject obj) {
            if (!obj) return false;
            return CollectUsed(obj.GetComponentsInChildren<SkinnedMeshRenderer>(true)).Count > 0;
        }

        // Kosci, ktore zastepujemy. Jesli siatka uzywa kosci szkieletu (Bip01...), bierzemy tylko je (ubrania).
        // Jesli nie uzywa zadnej (czyste akcesorium z wlasnymi kosciami), bierzemy wszystkie jej kosci - gra
        // nadpisuje m.in. korzen akcesorium (obj_tr) co klatke, wiec bez zastepnika nie da sie go przesunac.
        static HashSet<Transform> CollectUsed(SkinnedMeshRenderer[] renderers) {
            bool hasSkeletonBones = false;
            foreach (SkinnedMeshRenderer smr in renderers) {
                foreach (Transform b in smr.bones) {
                    if (IsProxyable(b)) { hasSkeletonBones = true; break; }
                }
                if (hasSkeletonBones) break;
            }
            HashSet<Transform> used = new HashSet<Transform>();
            foreach (SkinnedMeshRenderer smr in renderers) {
                foreach (Transform b in smr.bones) {
                    if (!b) continue;
                    if (b.name.EndsWith(Suffix) || b.name.EndsWith(FollowSuffix)) continue;
                    if (hasSkeletonBones && !IsProxyable(b)) continue;
                    used.Add(b);
                }
            }
            return used;
        }

        // Przy imporcie/wczytaniu: jesli dane zawieraja kosci-zastepniki (*_ShoeAdj), a na obiekcie ich jeszcze nie ma,
        // wlacz je - inaczej wpisy zostalyby po cichu pominiete (SearchObjName nie znajdzie kosci).
        public static void EnsureFor(GameObject obj, IEnumerable<string> names) {
            if (!obj || IsActive(obj)) return;
            bool need = false;
            foreach (string n in names) {
                if (n != null && n.EndsWith(Suffix)) { need = true; break; }
            }
            if (!need || !CanApply(obj)) return;
            Enable(obj);
            BoneEdit.requestRebuild = true;
        }

        // restored (opcjonalnie) dostaje wezly, ktorym przywrocono zapamietane korekty (zeby wywolujacy mogl zarejestrowac kopie zapasowe).
        // restoreStash=true tylko przy recznym wlaczeniu z przelacznika; przy imporcie/wczytaniu dane same ustalaja wartosci.
        public static int Enable(GameObject obj, List<Transform> restored = null, bool restoreStash = false) {
            if (!obj || IsActive(obj)) return 0;

            SkinnedMeshRenderer[] renderers = obj.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            ShoeProxyMarker marker = obj.AddComponent<ShoeProxyMarker>();
            marker.renderers = renderers;
            marker.originalBones = new Transform[renderers.Length][];
            marker.proxyBones = new Transform[renderers.Length][];

            // zbior kosci, ktore faktycznie uzywa siatka (i ktore zastepujemy)
            HashSet<Transform> used = CollectUsed(renderers);

            Dictionary<Transform, Transform> map = new Dictionary<Transform, Transform>();
            int replaced = 0;
            List<string> names = new List<string>();
            for (int r = 0; r < renderers.Length; r++) {
                Transform[] bones = renderers[r].bones;
                marker.originalBones[r] = (Transform[])bones.Clone();
                bool changed = false;
                for (int i = 0; i < bones.Length; i++) {
                    if (!bones[i] || !used.Contains(bones[i])) continue;
                    string realName = bones[i].name;
                    bones[i] = GetProxy(bones[i], obj.transform, used, map, marker);
                    names.Add(realName);
                    replaced++;
                    changed = true;
                }
                if (changed) renderers[r].bones = bones;
                marker.proxyBones[r] = changed ? bones : null;
            }
            if (restoreStash) {
                ShoeProxyStash stash = obj.GetComponent<ShoeProxyStash>();
                if (stash != null && stash.Poses.Count > 0) {
                    int back = 0;
                    foreach (GameObject go in marker.proxies) {
                        ShoeProxyStash.Pose pose;
                        if (go && stash.Poses.TryGetValue(go.name, out pose)) {
                            go.transform.localPosition = pose.Position;
                            go.transform.localRotation = pose.Rotation;
                            go.transform.localScale = pose.Scale;
                            if (restored != null) restored.Add(go.transform);
                            back++;
                        }
                    }
                    Log.Info("independent bones: restored " + back + " saved adjustments on '" + obj.name + "'");
                }
            }
            Log.Info("independent bones enabled on '" + obj.name + "', replaced " + replaced + " bone refs, created " + marker.proxies.Count + " nodes; bones: " + string.Join(", ", names.ToArray()));
            return replaced;
        }

        public static void Disable(GameObject obj) {
            if (!obj) return;
            ShoeProxyMarker marker = obj.GetComponent<ShoeProxyMarker>();
            if (marker == null) return;
            marker.suspended = true;

            // zapamietaj korekty (tylko wezly edytowalne, nie "follow"), zeby dalo sie je przywrocic po ponownym wlaczeniu
            ShoeProxyStash stash = obj.GetComponent<ShoeProxyStash>();
            if (stash == null) stash = obj.AddComponent<ShoeProxyStash>();
            stash.Poses.Clear();
            foreach (GameObject go in marker.proxies) {
                if (!go || !go.name.EndsWith(Suffix)) continue;
                Transform t = go.transform;
                if (t.localPosition == Vector3.zero && t.localRotation == Quaternion.identity && t.localScale == Vector3.one) continue;
                ShoeProxyStash.Pose pose = new ShoeProxyStash.Pose();
                pose.Position = t.localPosition;
                pose.Rotation = t.localRotation;
                pose.Scale = t.localScale;
                stash.Poses[go.name] = pose;
            }

            for (int r = 0; r < marker.renderers.Length; r++) {
                if (marker.renderers[r] && marker.originalBones[r] != null) {
                    marker.renderers[r].bones = marker.originalBones[r];
                }
            }
            for (int i = marker.proxies.Count - 1; i >= 0; i--) {
                if (marker.proxies[i]) Object.Destroy(marker.proxies[i]);
            }
            Object.Destroy(marker);
            Log.Info("independent bones disabled on '" + obj.name + "'");
        }

        static Transform GetProxy(Transform real, Transform root, HashSet<Transform> used, Dictionary<Transform, Transform> map, ShoeProxyMarker marker) {
            Transform proxy;
            if (map.TryGetValue(real, out proxy)) return proxy;

            GameObject go = new GameObject(real.name + Suffix);
            go.layer = real.gameObject.layer;
            proxy = go.transform;
            marker.proxies.Add(go);
            marker.nodes.Add(new KeyValuePair<Transform, Transform>(proxy, real));
            Transform proxyParent;

            // najblizszy przodek, ktory tez jest zastepowany
            Transform ancestor = real.parent;
            while (ancestor && ancestor != root && !used.Contains(ancestor)) ancestor = ancestor.parent;
            if (ancestor && !used.Contains(ancestor)) ancestor = null;

            if (ancestor) {
                // wezel "follow": dziecko proxy przodka, poza kopiowana z prawdziwych kosci co klatke
                Transform parentProxy = GetProxy(ancestor, root, used, map, marker);
                GameObject follow = new GameObject(real.name + FollowSuffix);
                follow.layer = real.gameObject.layer;
                marker.proxies.Add(follow);
                follow.transform.SetParent(parentProxy, false);
                LocalPoseCopier copier = follow.AddComponent<LocalPoseCopier>();
                copier.source = real;
                copier.reference = ancestor;
                copier.Apply();
                proxyParent = follow.transform;
            } else {
                // dziecko prawdziwej kosci (swiat = swiat oryginalu)
                proxyParent = real;
            }
            // wezel edytowalny: poza identycznosciowa wzgledem rodzica, nigdy nie jest nadpisywany
            proxy.SetParent(proxyParent, false);
            proxy.localPosition = Vector3.zero;
            proxy.localRotation = Quaternion.identity;
            proxy.localScale = Vector3.one;
            map[real] = proxy;
            return proxy;
        }
    }
}
