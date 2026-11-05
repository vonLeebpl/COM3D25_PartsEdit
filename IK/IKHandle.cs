using UnityEngine;

namespace CM3D2.PartsEdit.Plugin {
    // Free-moving sphere. Drag in any direction; the chain solves toward it.
    class IKHandle : MonoBehaviour {
        public IKChain chain;
        Camera cam;
        bool dragging;
        Plane plane;
        Vector3 grabOffset;
        public bool pole;
        Material mat;
        static Mesh sphereMesh;
        static Mesh cylinderMesh;

        public void Setup(IKChain fChain, bool isPole) {
            chain = fChain;
            pole = isPole;
            gameObject.name = (isPole ? "IKPole_" : "IKHandle_") + fChain.name;
            if (!GetComponent<MeshFilter>()) {
                MeshFilter mf = gameObject.AddComponent<MeshFilter>();
                mf.mesh = isPole ? Cylinder() : Sphere();
                MeshRenderer mr = gameObject.AddComponent<MeshRenderer>();
                mat = new Material(Shader.Find("Diffuse"));
                mr.material = mat;
                if (isPole) {
                    CapsuleCollider col = gameObject.AddComponent<CapsuleCollider>();
                    col.radius = 0.5f;
                    col.height = 2f;
                } else {
                    SphereCollider col = gameObject.AddComponent<SphereCollider>();
                    col.radius = 0.5f;
                }
            }
            ApplyColor();
            transform.localScale = isPole ? new Vector3(0.02f, 0.06f, 0.02f) : Vector3.one * 0.04f;
            Place();
        }

        public void ApplyColor() {
            if (mat == null) return;
            Color c = Setting.ikChainColor.GetValue();
            mat.color = c;
        }

        public void Place() {
            if (dragging || chain == null) return;
            if (pole) {
                transform.position = new Vector3(chain.poleX, chain.poleY, chain.poleZ);
                return;
            }
            GameObject owner = IKManager.FindObject(chain.owner);
            if (chain.hasGoal && owner) transform.position = owner.transform.TransformPoint(new Vector3(chain.goalX, chain.goalY, chain.goalZ));
            else {
                Transform end = IKManager.FindBoneOn(owner, chain.endBone);
                if (end) transform.position = end.position;
            }
        }

        void Update() {
            if (!dragging) return;
            if (!Input.GetMouseButton(0)) { dragging = false; return; }
            bool planeKey = Input.GetKey(Setting.ikPlaneKey);
            bool axisKey = Input.GetKey(Setting.ikAxisKey);
            bool angleKey = Input.GetKey(Setting.ikAngleKey);
            Ray ray = CameraRay();
            float dist;
            if (!plane.Raycast(ray, out dist)) return;
            Vector3 hit = ray.GetPoint(dist) + grabOffset;
            if (planeKey && angleKey && !pole && chain != null) {
                Transform end = IKManager.FindBoneOn(IKManager.FindObject(chain.owner), chain.endBone);
                if (end && end.parent) {
                    Vector3 dir = hit - transform.position;
                    if (dir.sqrMagnitude > 0.0001f) {
                        end.parent.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
                        chain.tipX = end.parent.rotation.x; chain.tipY = end.parent.rotation.y;
                        chain.tipZ = end.parent.rotation.z; chain.tipW = end.parent.rotation.w;
                        chain.hasTip = true;
                    }
                }
                return;
            }
            if (planeKey && axisKey) {
                hit.x = transform.position.x;
                hit.z = transform.position.z;
            } else if (planeKey) {
                hit.y = transform.position.y;
            }
            transform.position = hit;
            if (chain == null) return;
            if (pole) {
                chain.poleX = transform.position.x;
                chain.poleY = transform.position.y;
                chain.poleZ = transform.position.z;
            } else {
                GameObject owner = IKManager.FindObject(chain.owner);
                Vector3 local = owner ? owner.transform.InverseTransformPoint(transform.position) : transform.position;
                chain.goalX = local.x; chain.goalY = local.y; chain.goalZ = local.z;
                chain.hasGoal = true;
                IKManager.Solve(chain, transform.position);
            }
        }

        void OnMouseDown() {
            cam = Camera.main;
            if (!cam) return;
            dragging = true;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            plane = new Plane(-cam.transform.forward, transform.position);
            float dist;
            if (plane.Raycast(ray, out dist)) grabOffset = transform.position - ray.GetPoint(dist);
            else grabOffset = Vector3.zero;
        }

        Ray CameraRay() {
            if (!cam) cam = Camera.main;
            return cam.ScreenPointToRay(Input.mousePosition);
        }

        static Mesh Sphere() {
            if (sphereMesh) return sphereMesh;
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphereMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(temp);
            return sphereMesh;
        }

        static Mesh Cylinder() {
            if (cylinderMesh) return cylinderMesh;
            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cylinderMesh = temp.GetComponent<MeshFilter>().sharedMesh;
            Object.Destroy(temp);
            return cylinderMesh;
        }
    }
}
