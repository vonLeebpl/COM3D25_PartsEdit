using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

internal class BoneRendererAssist : MonoBehaviour{
    #region メンバ変数
    public bool visible = true;
    public bool selectable = false;
    // >0: kosc rysowana zawsze jako odcinek o tej dlugosci wzdluz lokalnej osi X (np. kosci-zastepniki butow)
    public float fixedLength = 0f;
    public bool Selectable { get { return visible && selectable; } }

    public bool IsRoot {
        get {
            return parent == null;
        }
    }

    private Material lineMaterial;
    Color lineColor = Color.white;
    private readonly float lineWidth = 0.012f;

    LineRenderer boneRenderer = null;
    LineRenderer BoneRenderer {
        get {
            if (boneRenderer == null) {
                boneRenderer = gameObject.AddComponent<LineRenderer>();
                lineMaterial = CreateMaterial();
                boneRenderer.materials = new[] { lineMaterial, };
#if UNITY_5_6_OR_NEWER
        line.startWidth = lineWidth;
        line.endWidth   = lineWidth*0.2f;
        line.positionCount = 2;
#else
                boneRenderer.SetWidth(lineWidth, lineWidth*0.2f);
                boneRenderer.SetVertexCount(2);
#endif
            }
            return boneRenderer;
        }
    }
    CapsuleCollider boneCollider = null;
    CapsuleCollider BoneCollider {
        get {
            if (boneCollider == null) {
                boneCollider = gameObject.AddComponent<CapsuleCollider>();
                boneCollider.direction = 0;
                boneCollider.radius = lineWidth;
                boneCollider.isTrigger = true;
            }
            return boneCollider;
        }
    }

    public BoneRendererAssist parent = null;
    public List<BoneRendererAssist> children = new List<BoneRendererAssist>();

    Vector3 boneTailPos = Vector3.zero;

    static float minLength = 0.03f;
    static float maxLength = 0.1f;
    #endregion

    #region イベント関数
    public void BRAUpdate() {
        if (!IsRoot) return;
        UpdateTransform();
        UpdatePosition();
    }
    #endregion

    #region public関数
    // 親子関係自動取得
    public void AutoSetUp() {
        // 親を設定
        if (transform.parent) {
            parent = transform.parent.GetComponent<BoneRendererAssist>();
        }else {
            parent = null;
        }

        // 子を設定
        children = new List<BoneRendererAssist>();
        foreach(Transform child in transform) {
            BoneRendererAssist bra = child.GetComponent<BoneRendererAssist>();
            if (bra != null) {
                children.Add(bra);
            }
        }
    }

    public void SetParent(BoneRendererAssist bra) {
        parent = bra;
    }

    public void SetChild(BoneRendererAssist bra) {
        children.Add(bra);
    }

    public void SetFirstChild(BoneRendererAssist bra) {
        children.Insert(0, bra);
    }

    public void UpdatePosition() {
        // BoneRenderer
        if (visible) {
            UpdateBoneRendererPosition();
        }

        // Collider
        if (Selectable) {
            UpdateBoneColliderPosition();
        }

        foreach(BoneRendererAssist child in children) {
            child.UpdatePosition();
        }
    }

    public void UpdateTransform() {
        gameObject.GetComponent<CopyTransform>().FollowTarget();

        foreach(BoneRendererAssist child in children) {
            child.UpdateTransform();
        }
    }

    public Vector3 GetBoneTailPos() {
        return boneTailPos;
    }

    public float GetBoneLength() {
        return (boneTailPos - transform.position).magnitude;
    }

    public void SetVisible(bool fVisible) {
        visible = fVisible;
        if (boneRenderer != null) {
            boneRenderer.enabled = fVisible;
        }
        if (boneCollider != null) {
            boneCollider.enabled = selectable && visible;
        }
    }

    public void SetSelectable(bool fSelectable) {
        selectable = fSelectable;
        if (boneCollider != null) {
            boneCollider.enabled = selectable && visible;
        }
    }

    public void SetColor(Color fColor) {
        lineColor = fColor;

        if (boneRenderer != null) {
            boneRenderer.material.color = lineColor;
        }
    }

    public Vector3 GetColliderCenter() {
        if (!boneCollider) return Vector3.zero;
        return boneCollider.center;
    }

    public float GetColliderLength() {
        if (!boneCollider) return 0;
        return boneCollider.height;
    }
    #endregion

    #region private関数
    void UpdateBoneRendererPosition() {
        BoneRenderer.SetPosition(0, transform.position);

        if (fixedLength > 0f && children.Count == 0) {
            boneTailPos = transform.TransformPoint(fixedLength, 0f, 0f);
            BoneRenderer.SetPosition(1, boneTailPos);
            return;
        }

        boneTailPos = transform.position;
        switch (children.Count) {
            case 0:
                break;
            case 1:
                //boneTailPos = children[0].transform.position;
                boneTailPos = transform.position + Vector3.Project(children[0].transform.position - transform.position, transform.right);
                break;
            default:
                // 子が2以上の時
                boneTailPos = transform.position + Vector3.Project(children[0].transform.position - transform.position, transform.right);
                break;
        }

        if (boneTailPos == transform.position) {
            if (parent) {
                boneTailPos = transform.TransformPoint(-parent.GetBoneLength(), 0f, 0f);
            } else {
                boneTailPos = transform.TransformPoint(-maxLength, 0f, 0f);
            }
        }else if((transform.position - boneTailPos).magnitude < minLength){
            boneTailPos = transform.TransformPoint(-minLength, 0f, 0f);
        }

        BoneRenderer.SetPosition(1, boneTailPos);
    }

    void UpdateBoneColliderPosition() {
        BoneCollider.center = transform.InverseTransformPoint(boneTailPos) / 2;
        BoneCollider.height = Mathf.Abs(transform.InverseTransformPoint(boneTailPos).x);
    }

    Material CreateMaterial() {
        var shader = Shader.Find("Hidden/Internal-Colored");
        var material = new Material(shader) {
            hideFlags = HideFlags.HideAndDontSave
        };
        material.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Disabled);
        material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        material.SetInt("_ZWrite", 0);
        material.renderQueue = 5000;
        material.color = lineColor;
        return material;
    }
    #endregion


}
