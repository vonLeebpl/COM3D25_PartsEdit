using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    public class IKLimit {
        public bool enabled;
        public float minX = -45f, maxX = 45f;
        public float minY = -45f, maxY = 45f;
        public float minZ = -45f, maxZ = 45f;
        public void CopyFrom(IKLimit other) {
            enabled = other.enabled;
            minX = other.minX; maxX = other.maxX;
            minY = other.minY; maxY = other.maxY;
            minZ = other.minZ; maxZ = other.maxZ;
        }
    }

    public class IKChain {
        public string id = Guid.NewGuid().ToString("N");
        public string name = "IK";
        public string owner = "";
        public string rootBone = "";
        public string endBone = "";
        public string linkFromId = "";
        public string bindObject = "";
        public string bindBone = "";
        public float bindX, bindY, bindZ;
        public bool enabled = true;
        public bool poleEnabled;
        public float poleX, poleY, poleZ;
        public bool gravityEnabled;
        public float gravityStrength = 1f;
        public float gravityX, gravityY = -1f, gravityZ;
        public float goalX, goalY, goalZ;
        public bool hasGoal;
        public float tipX, tipY, tipZ, tipW = 1f;
        public bool hasTip;
        public List<string> bones = new List<string>();
        public List<IKLimit> limits = new List<IKLimit>();
    }

    public class IKFile {
        public string objectName = "";
        public bool hideHandles;
        public bool frozen;
        public bool ignoreGravity;
        public bool freezeAngle;
        public List<IKChain> chains = new List<IKChain>();
    }

    static class IKManager {
        public static Dictionary<string, IKFile> files = new Dictionary<string, IKFile>();
        public static IKChain selected;
        public static bool includeInPreset = true;
        static HashSet<string> loaded = new HashSet<string>();

        public static string DirectoryPath {
            get { return Path.Combine(PresetManager.directoryPath, "IK") + Path.DirectorySeparatorChar; }
        }

        public static List<IKChain> chains {
            get {
                IKFile file = CurrentFile();
                return file == null ? new List<IKChain>() : file.chains;
            }
        }

        public static void BindObject() {
            string key = ObjectKey();
            if (string.IsNullOrEmpty(key) || loaded.Contains(key)) return;
            loaded.Add(key);
            string path = DirectoryPath + Sanitize(key) + ".xml";
            if (!File.Exists(path)) {
                files[key] = new IKFile { objectName = key };
                return;
            }
            try {
                XmlSerializer ser = new XmlSerializer(typeof(IKFile));
                using (StreamReader sr = new StreamReader(path)) {
                    IKFile file = ser.Deserialize(sr) as IKFile;
                    files[key] = file ?? new IKFile { objectName = key };
                }
            } catch (Exception e) {
                Debug.Log("PartsEdit IK load failed: " + e.Message);
                files[key] = new IKFile { objectName = key };
            }
        }

        public static IKFile CurrentFile() {
            string key = ObjectKey();
            if (string.IsNullOrEmpty(key)) return null;
            BindObject();
            IKFile file;
            return files.TryGetValue(key, out file) ? file : null;
        }

        public static void Save() { SaveKey(ObjectKey()); }

        public static void SaveKey(string key) {
            if (string.IsNullOrEmpty(key) || !files.ContainsKey(key)) return;
            Directory.CreateDirectory(DirectoryPath);
            XmlSerializer ser = new XmlSerializer(typeof(IKFile));
            using (StreamWriter sw = new StreamWriter(DirectoryPath + Sanitize(key) + ".xml", false, new UTF8Encoding(false))) {
                ser.Serialize(sw, files[key]);
            }
        }

        public static string ObjectKey() {
            return CommonUIData.obj ? CommonUIData.obj.name : "";
        }

        static string Sanitize(string name) {
            foreach (char c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name;
        }

        public static IKChain CreateFromSelection(Transform end, Transform root) {
            if (!end || !root) return null;
            List<Transform> path = PathBetween(root, end);
            if (path.Count < 2) return null;
            IKFile file = CurrentFile();
            if (file == null) return null;
            IKChain chain = new IKChain();
            chain.name = end.name;
            chain.owner = ObjectKey();
            chain.rootBone = root.name;
            chain.endBone = end.name;
            chain.poleX = end.position.x; chain.poleY = end.position.y + 0.1f; chain.poleZ = end.position.z;
            Vector3 local = root.InverseTransformPoint(end.position);
            chain.goalX = local.x; chain.goalY = local.y; chain.goalZ = local.z;
            chain.hasGoal = true;
            foreach (Transform t in path) {
                chain.bones.Add(t.name);
                chain.limits.Add(new IKLimit());
            }
            file.chains.Add(chain);
            selected = chain;
            return chain;
        }

        public static void Remove(IKChain chain) {
            if (chain == null) return;
            foreach (IKFile file in files.Values) {
                foreach (IKChain other in file.chains) if (other.linkFromId == chain.id) other.linkFromId = "";
                file.chains.Remove(chain);
            }
            if (selected == chain) selected = null;
        }

        public static List<Transform> Resolve(IKChain chain) {
            List<Transform> result = new List<Transform>();
            if (chain == null) return result;
            GameObject owner = FindObject(chain.owner);
            if (!owner) return result;
            HashSet<string> seen = new HashSet<string>();
            foreach (IKChain part in Linked(chain)) {
                foreach (string name in part.bones) {
                    if (seen.Contains(name)) continue;
                    Transform t = FindBoneOn(owner, name);
                    if (!t) continue;
                    seen.Add(name);
                    result.Add(t);
                }
            }
            return result;
        }

        static List<IKChain> Linked(IKChain chain) {
            List<IKChain> order = new List<IKChain>();
            HashSet<string> guard = new HashSet<string>();
            IKChain cursor = chain;
            while (cursor != null && guard.Add(cursor.id)) {
                order.Insert(0, cursor);
                cursor = string.IsNullOrEmpty(cursor.linkFromId) ? null : Find(cursor.linkFromId);
            }
            return order;
        }

        public static bool InAnyChain(Transform bone) {
            if (!bone) return false;
            foreach (IKFile file in files.Values)
                foreach (IKChain chain in file.chains)
                    if (chain.enabled && chain.bones.Contains(bone.name)) return true;
            return false;
        }

        public static IKChain Find(string id) {
            foreach (IKFile file in files.Values)
                foreach (IKChain c in file.chains) if (c.id == id) return c;
            return null;
        }

        public static Transform FindBone(string name) { return FindBoneOn(CommonUIData.obj, name); }

        static Dictionary<int, Transform[]> boneCache = new Dictionary<int, Transform[]>();

        public static Transform FindBoneOn(GameObject obj, string name) {
            if (!obj || string.IsNullOrEmpty(name)) return null;
            int id = obj.GetInstanceID();
            Transform[] bones;
            if (!boneCache.TryGetValue(id, out bones) || bones == null) {
                bones = obj.GetComponentsInChildren<Transform>(true);
                boneCache[id] = bones;
            }
            for (int i = 0; i < bones.Length; i++) if (bones[i] && bones[i].name == name) return bones[i];
            return null;
        }

        static Dictionary<string, GameObject> owners = new Dictionary<string, GameObject>();

        public static GameObject FindObject(string name) {
            if (string.IsNullOrEmpty(name)) return null;
            if (CommonUIData.obj && CommonUIData.obj.name == name) {
                owners[name] = CommonUIData.obj;
                return CommonUIData.obj;
            }
            GameObject cached;
            if (owners.TryGetValue(name, out cached) && cached) return cached;
            GameObject found = GameObject.Find(name);
            if (found) owners[name] = found;
            return found;
        }

        static List<Transform> PathBetween(Transform root, Transform end) {
            List<Transform> up = new List<Transform>();
            Transform cursor = end;
            while (cursor) {
                up.Add(cursor);
                if (cursor == root) break;
                cursor = cursor.parent;
            }
            up.Reverse();
            return up;
        }

        static Dictionary<string, IKHandle> handles = new Dictionary<string, IKHandle>();
        static Dictionary<string, IKHandle> poles = new Dictionary<string, IKHandle>();

        public static void RefreshHandles() {
            HashSet<string> live = new HashSet<string>();
            HashSet<string> livePoles = new HashSet<string>();
            List<string> gone = new List<string>();
            foreach (IKFile file in new List<IKFile>(files.Values)) {
                if (!FindObject(file.objectName)) { gone.Add(file.objectName); continue; }
                if (file.frozen) continue;
                foreach (IKChain chain in file.chains) {
                    if (!chain.enabled) continue;
                    Tick(chain);
                    if (Setting.gizmoType != GizmoType.IK) continue;
                    live.Add(chain.id);
                    Ensure(handles, chain, false);
                    if (chain.poleEnabled) {
                        livePoles.Add(chain.id);
                        Ensure(poles, chain, true);
                    }
                }
            }
            foreach (string name in gone) Drop(name);
            Sweep(handles, live);
            Sweep(poles, livePoles);
        }

        static void Drop(string objectName) {
            IKFile file;
            if (!files.TryGetValue(objectName, out file)) return;
            foreach (IKChain chain in file.chains) {
                IKHandle handle;
                if (handles.TryGetValue(chain.id, out handle) && handle) UnityEngine.Object.Destroy(handle.gameObject);
                if (poles.TryGetValue(chain.id, out handle) && handle) UnityEngine.Object.Destroy(handle.gameObject);
                handles.Remove(chain.id);
                poles.Remove(chain.id);
            }
            files.Remove(objectName);
            loaded.Remove(objectName);
            owners.Remove(objectName);
            string path = DirectoryPath + Sanitize(objectName) + ".xml";
            if (File.Exists(path)) File.Delete(path);
        }

        static HashSet<string> physicsOn = new HashSet<string>();
        static Dictionary<string, bool> settled = new Dictionary<string, bool>();
        static Dictionary<string, Vector3> lastGoal = new Dictionary<string, Vector3>();

        static Dictionary<string, float> objectRadius = new Dictionary<string, float>();

        public static bool PhysicsEnabled(string objectName) { return physicsOn.Contains(objectName); }
        public static float RadiusFor(string objectName) {
            float v;
            return objectRadius.TryGetValue(objectName, out v) ? v : Setting.physicsRadius;
        }
        public static void SetRadius(string objectName, float value) {
            if (!string.IsNullOrEmpty(objectName)) objectRadius[objectName] = value;
        }
        public static void SetPhysics(string objectName, bool on) {
            if (string.IsNullOrEmpty(objectName)) return;
            if (on) physicsOn.Add(objectName); else physicsOn.Remove(objectName);
            settled.Clear();
        }
        static Dictionary<string, Vector3> lastPole = new Dictionary<string, Vector3>();

        static void Tick(IKChain chain) {
            if (IsFrozen(chain)) return;
            GameObject owner = FindObject(chain.owner);
            if (!owner) return;
            Vector3 goal;
            if (!string.IsNullOrEmpty(chain.bindBone)) {
                Transform bone = FindBoneOn(ResolveBind(chain.bindObject), chain.bindBone);
                if (!bone) return;
                goal = bone.TransformPoint(new Vector3(chain.bindX, chain.bindY, chain.bindZ));
            } else if (chain.hasGoal) {
                goal = owner.transform.TransformPoint(new Vector3(chain.goalX, chain.goalY, chain.goalZ));
            } else {
                Transform end = FindBoneOn(owner, chain.endBone);
                if (!end) return;
                goal = end.position;
            }
            Vector3 prev;
            Vector3 pole = new Vector3(chain.poleX, chain.poleY, chain.poleZ);
            string key = chain.id;
            bool moved = !lastGoal.TryGetValue(key, out prev) || (prev - goal).sqrMagnitude > 0.0000001f;
            Vector3 prevPole;
            bool poleMoved = chain.poleEnabled && (!lastPole.TryGetValue(key, out prevPole) || (prevPole - pole).sqrMagnitude > 0.0000001f);
            if (moved || poleMoved) settled[key] = false;
            bool isSettled;
            if (!moved && !poleMoved && settled.TryGetValue(key, out isSettled) && isSettled) return;
            lastGoal[key] = goal;
            lastPole[key] = pole;
            Vector3 before = Vector3.zero;
            Transform tip = FindBoneOn(owner, chain.endBone);
            if (tip) before = tip.position;
            Solve(chain, goal);
            if (tip && (tip.position - before).sqrMagnitude < 0.00000001f) settled[key] = true;
        }

        static void Ensure(Dictionary<string, IKHandle> map, IKChain chain, bool pole) {
            IKHandle handle;
            if (!map.TryGetValue(chain.id, out handle) || !handle) {
                GameObject go = new GameObject(pole ? "IKPole" : "IKHandle");
                handle = go.AddComponent<IKHandle>();
                handle.Setup(chain, pole);
                map[chain.id] = handle;
            }
            handle.chain = chain;
            handle.ApplyColor();
            if (!pole && FollowBind(chain, handle)) return;
            handle.Place();
        }

        static void Sweep(Dictionary<string, IKHandle> map, HashSet<string> live) {
            List<string> dead = new List<string>();
            foreach (KeyValuePair<string, IKHandle> pair in map) if (!live.Contains(pair.Key)) dead.Add(pair.Key);
            foreach (string id in dead) {
                if (map[id]) UnityEngine.Object.Destroy(map[id].gameObject);
                map.Remove(id);
            }
        }

        public static void BindTipToBone(IKChain chain, Transform bone) {
            if (chain == null || !bone) return;
            Transform end = FindBoneOn(FindObject(chain.owner), chain.endBone);
            Maid maid = MaidOf(bone);
            if (maid != null && maid.status != null) {
                chain.bindObject = "maid:" + maid.status.guid;
            } else {
                GameObject owner = bone.gameObject;
                while (owner.transform.parent && owner.transform.parent.gameObject.GetComponent<Maid>() == null) owner = owner.transform.parent.gameObject;
                chain.bindObject = owner.name;
            }
            chain.bindBone = bone.name;
            Vector3 local = bone.InverseTransformPoint(end ? end.position : bone.position);
            chain.bindX = local.x; chain.bindY = local.y; chain.bindZ = local.z;
        }

        static Maid MaidOf(Transform bone) {
            Transform cursor = bone;
            while (cursor) {
                Maid maid = cursor.GetComponent<Maid>();
                if (maid) return maid;
                cursor = cursor.parent;
            }
            return null;
        }

        static GameObject ResolveBind(string key) {
            if (string.IsNullOrEmpty(key)) return null;
            if (key.StartsWith("maid:")) {
                string guid = key.Substring(5);
                CharacterMgr mgr = GameMain.Instance ? GameMain.Instance.CharacterMgr : null;
                if (mgr == null) return null;
                for (int i = 0; i < mgr.GetMaidCount(); i++) {
                    Maid maid = mgr.GetMaid(i);
                    if (maid && maid.status != null && maid.status.guid == guid) return maid.gameObject;
                }
                return null;
            }
            return FindObject(key);
        }

        public static void Unbind(IKChain chain) {
            if (chain == null) return;
            chain.bindObject = "";
            chain.bindBone = "";
        }

        static bool FollowBind(IKChain chain, IKHandle handle) {
            if (string.IsNullOrEmpty(chain.bindBone) || handle == null) return false;
            GameObject bound = ResolveBind(chain.bindObject);
            Transform bone = FindBoneOn(bound, chain.bindBone);
            if (!bone) return false;
            Vector3 world = bone.TransformPoint(new Vector3(chain.bindX, chain.bindY, chain.bindZ));
            handle.transform.position = world;
            if (!IsFrozen(chain)) Solve(chain, world);
            return true;
        }

        static bool FreezeAngle(IKChain chain) {
            IKFile file;
            return files.TryGetValue(chain.owner, out file) && file.freezeAngle;
        }

        static bool IgnoreGravity(IKChain chain) {
            IKFile file;
            return files.TryGetValue(chain.owner, out file) && file.ignoreGravity;
        }

        static bool IsFrozen(IKChain chain) {
            IKFile file;
            return files.TryGetValue(chain.owner, out file) && file.frozen;
        }

        public static void SolveSelected() {
            if (selected == null || !selected.enabled || IsFrozen(selected)) return;
            Transform end = FindBoneOn(FindObject(selected.owner), selected.endBone);
            if (!end) return;
            Solve(selected, end.position);
        }

        public static void Solve(IKChain chain, Vector3 target) {
            if (IsFrozen(chain)) return;
            List<Transform> bones = Resolve(chain);
            if (bones.Count < 2) return;
            int n = bones.Count;
            float[] lengths = new float[n - 1];
            Vector3[] pos = new Vector3[n];
            for (int i = 0; i < n; i++) pos[i] = bones[i].position;
            for (int i = 0; i < lengths.Length; i++) lengths[i] = Vector3.Distance(pos[i], pos[i + 1]);
            Vector3 root = pos[0];
            float total = 0f;
            for (int i = 0; i < lengths.Length; i++) total += lengths[i];
            float reach = Vector3.Distance(root, target);
            bool slack = reach < total * 0.98f;
            for (int iter = 0; iter < 8; iter++) {
                pos[n - 1] = target;
                for (int i = n - 2; i >= 0; i--) pos[i] = pos[i + 1] + (pos[i] - pos[i + 1]).normalized * lengths[i];
                pos[0] = root;
                for (int i = 0; i < n - 1; i++) pos[i + 1] = pos[i] + (pos[i + 1] - pos[i]).normalized * lengths[i];
                if ((pos[n - 1] - target).sqrMagnitude < 0.0000001f) break;
            }
            if (chain.poleEnabled) TwistPole(pos, target, new Vector3(chain.poleX, chain.poleY, chain.poleZ), lengths);
            if (Setting.gravityOn && !IgnoreGravity(chain)) {
                Vector3 g = new Vector3(Setting.gravityX, Setting.gravityY, Setting.gravityZ);
                if (g.sqrMagnitude > 0.0001f) {
                    Vector3 pull = g.normalized * Setting.gravityStrength * 0.01f;
                    for (int i = 1; i < pos.Length; i++) {
                        if (Setting.gravityFloorOn && pos[i].y <= Setting.gravityFloor + 0.001f) continue;
                        pos[i] += pull * i;
                    }
                }
                if (Setting.gravityFloorOn) {
                    for (int i = 1; i < pos.Length; i++) if (pos[i].y < Setting.gravityFloor) pos[i].y = Setting.gravityFloor;
                    if (!chain.poleEnabled) HangOffFloor(pos, g.sqrMagnitude > 0.0001f ? g.normalized : Vector3.down, chain.id);
                }
            }
            if (Setting.experimental) PushApart(pos, chain);
            for (int i = 0; i < n - 1; i++) {
                Vector3 dir = pos[i + 1] - pos[i];
                if (dir.sqrMagnitude < 0.0000001f) continue;
                Vector3 current = bones[i + 1].position - bones[i].position;
                if (current.sqrMagnitude < 0.0000001f) current = bones[i].TransformDirection(Vector3.forward);
                bones[i].rotation = Quaternion.FromToRotation(current, dir) * bones[i].rotation;
                bones[i + 1].position = bones[i].position + dir.normalized * lengths[i];
                ApplyLimit(chain, bones[i], i);
            }
            if (chain.hasTip && bones.Count > 1 && FreezeAngle(chain)) {
                bones[bones.Count - 2].rotation = new Quaternion(chain.tipX, chain.tipY, chain.tipZ, chain.tipW);
            }
        }

        static Dictionary<string, float> lastHang = new Dictionary<string, float>();

        // Ignore the first 2 bones and the last 2 on the floor. Average 3 floor bones in between. Pole overrides.
        static void HangOffFloor(Vector3[] pos, Vector3 gravity, string chainId) {
            int lastFloor = -1;
            for (int i = 0; i < pos.Length; i++) if (pos[i].y <= Setting.gravityFloor + 0.001f) lastFloor = i;
            if (lastFloor < 0) return;
            int from = 2;
            int to = lastFloor - 2;
            List<int> picks = new List<int>();
            for (int i = from; i <= to; i++) if (pos[i].y <= Setting.gravityFloor + 0.001f) picks.Add(i);
            float angle = 0f;
            Vector3 pivot = pos[0];
            if (picks.Count >= 1) {
                int a = picks[0];
                int b = picks[picks.Count / 2];
                int c = picks[picks.Count - 1];
                pivot = (pos[a] + pos[b] + pos[c]) / 3f;
                Vector3 axis = pos[pos.Length - 1] - pivot;
                if (axis.sqrMagnitude > 0.0000001f) {
                    Vector3 mid = pos[b] - pivot;
                    Vector3 want = Vector3.ProjectOnPlane(gravity, axis);
                    Vector3 have = Vector3.ProjectOnPlane(mid, axis);
                    if (have.sqrMagnitude > 0.0000001f && want.sqrMagnitude > 0.0000001f)
                        angle = SignedAngle(have, want, axis.normalized);
                }
            }
            float prev;
            if (lastHang.TryGetValue(chainId, out prev) && Mathf.Abs(Mathf.DeltaAngle(prev, angle)) > 45f) angle = 0f;
            lastHang[chainId] = angle;
            if (Mathf.Abs(angle) < 1f) return;
            Vector3 axis2 = pos[pos.Length - 1] - pivot;
            if (axis2.sqrMagnitude < 0.0000001f) return;
            Quaternion twist = Quaternion.AngleAxis(angle, axis2.normalized);
            for (int i = lastFloor + 1; i < pos.Length; i++) pos[i] = pivot + twist * (pos[i] - pivot);
        }
        // Pole only sets the facing angle. It does not pull or change reach.
        static void TwistPole(Vector3[] pos, Vector3 target, Vector3 pole, float[] lengths) {
            Vector3 axis = target - pos[0];
            if (axis.sqrMagnitude < 0.0000001f) return;
            int mid = pos.Length / 2;
            Vector3 have = Vector3.ProjectOnPlane(pos[mid] - pos[0], axis);
            Vector3 want = Vector3.ProjectOnPlane(pole - pos[0], axis);
            if (have.sqrMagnitude < 0.0000001f || want.sqrMagnitude < 0.0000001f) return;
            float angle = SignedAngle(have, want, axis.normalized);
            if (Mathf.Abs(angle) < 0.5f) return;
            Quaternion twist = Quaternion.AngleAxis(angle, axis.normalized);
            for (int i = 1; i < pos.Length; i++) pos[i] = pos[0] + twist * (pos[i] - pos[0]);
        }

        static float SignedAngle(Vector3 from, Vector3 to, Vector3 axis) {
            float angle = Vector3.Angle(from, to);
            if (Vector3.Dot(axis, Vector3.Cross(from, to)) < 0f) angle = -angle;
            return angle;
        }

        static void ApplyLimit(IKChain chain, Transform bone, int index) {
            if (index < 0 || index >= chain.limits.Count) return;
            IKLimit limit = chain.limits[index];
            if (limit == null || !limit.enabled) return;
            Vector3 euler = bone.localEulerAngles;
            euler.x = ClampAngle(euler.x, limit.minX, limit.maxX);
            euler.y = ClampAngle(euler.y, limit.minY, limit.maxY);
            euler.z = ClampAngle(euler.z, limit.minZ, limit.maxZ);
            bone.localEulerAngles = euler;
        }

        static float ClampAngle(float angle, float min, float max) {
            return Mathf.Clamp(Mathf.DeltaAngle(0f, angle), min, max);
        }

        public static void MassSet(IKChain chain, int fromIndex) {
            if (chain == null || fromIndex < 0 || fromIndex >= chain.limits.Count) return;
            IKLimit source = chain.limits[fromIndex];
            for (int i = fromIndex; i >= 0; i--) chain.limits[i].CopyFrom(source);
        }

        static Dictionary<string, Transform[]> physicsCache = new Dictionary<string, Transform[]>();
        static Dictionary<string, Vector3> lastPhysicsPos = new Dictionary<string, Vector3>();

        static Transform[] PhysicsBones(string objectName) {
            Transform[] bones;
            if (physicsCache.TryGetValue(objectName, out bones) && bones != null) return bones;
            GameObject obj = FindObject(objectName);
            if (!obj) return new Transform[0];
            bones = obj.GetComponentsInChildren<Transform>(true);
            physicsCache[objectName] = bones;
            return bones;
        }

        static void PushApart(Vector3[] pos, IKChain self) {
            float r = RadiusFor(self.owner);
            List<Transform> selfBones = Resolve(self);
            if (PhysicsEnabled(self.owner)) PushAgainst(pos, selfBones, selfBones, r);
            foreach (IKChain other in AllChains()) {
                if (other == self || !other.enabled || !PhysicsEnabled(other.owner)) continue;
                if (!MovedInto(other.owner, r)) continue;
                PushAgainst(pos, selfBones, Resolve(other), r);
            }
            foreach (string name in physicsOn) {
                if (name == self.owner) continue;
                if (!MovedInto(name, r)) continue;
                GameObject obj = FindObject(name);
                if (!obj) continue;
                PushAgainst(pos, selfBones, new List<Transform>(PhysicsBones(name)), r);
            }
        }

        static bool MovedInto(string objectName, float radius) {
            GameObject obj = FindObject(objectName);
            if (!obj) return false;
            Vector3 now = obj.transform.position;
            Vector3 prev;
            if (!lastPhysicsPos.TryGetValue(objectName, out prev)) { lastPhysicsPos[objectName] = now; return true; }
            lastPhysicsPos[objectName] = now;
            return (now - prev).sqrMagnitude > 0.00000001f;
        }

        static void PushAgainst(Vector3[] pos, List<Transform> selfBones, List<Transform> others, float r) {
            for (int i = 1; i < pos.Length; i += 2) {
                for (int j = 0; j < others.Count; j += 2) {
                    Transform t = others[j];
                    if (!t) continue;
                    if (i < selfBones.Count && IsNeighbor(selfBones, i, t)) continue;
                    Vector3 d = pos[i] - t.position;
                    float dist = d.magnitude;
                    if (dist < r * 2f && dist > 0.0001f) pos[i] += d.normalized * (r * 2f - dist) * 0.5f;
                }
            }
        }

        static bool IsNeighbor(List<Transform> bones, int index, Transform other) {
            if (index > 0 && bones[index - 1] == other) return true;
            if (index + 1 < bones.Count && bones[index + 1] == other) return true;
            return false;
        }

        public static IEnumerable<IKChain> AllChains() {
            foreach (IKFile file in files.Values)
                foreach (IKChain chain in file.chains) yield return chain;
        }
    }
}
