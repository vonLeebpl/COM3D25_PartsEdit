using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Reflection;
using UnityEngine;


/// <summary>
/// 公式ギズモを使いやすくするためのクラス
/// </summary>
internal class ExGizmoRenderer : GizmoRender {
    public COORDINATE coordinate = COORDINATE.Local;
    public bool eDragUndo = false;
    protected bool dragnow = false;
    protected bool dragMove = false;

    public delegate void DragEndDelegate();
    public event DragEndDelegate dragEndDelegate = delegate () { };

    protected FieldInfo _fi = null;
    protected FieldInfo _fi_beSelectedType = null;
    protected FieldInfo _fi_local_control_lock_ = null;
    bool _isdrag_bk = false;
    int selectedType = 0;

    public KeyCode smallMoveKey = KeyCode.None;
    public KeyCode bigMoveKey = KeyCode.None;

    // ターゲット情報
    protected Transform targetTrs = null;
    protected virtual Transform posTargetTrs { get { return targetTrs; } }
    protected virtual Transform rotTargetTrs { get { return targetTrs; } }
    protected virtual Transform sclTargetTrs { get { return targetTrs; } }

    //差分計算用
    Vector3 _backup_pos = Vector3.zero;
    Quaternion _backup_rot = Quaternion.identity;
    Vector3 _backup_local_scl = Vector3.one;

    //差分
    protected Vector3 deltaPos = Vector3.zero;
    protected Quaternion deltaRot = Quaternion.identity;
    protected Vector3 deltaLocalPos = Vector3.zero;
    protected Quaternion deltaLocalRot = Quaternion.identity;
    protected Vector3 deltaLocalScl = Vector3.zero;

    //ギズモ位置
    protected Vector3 position {
        get { return this.transform.position; }
        set { this.transform.position = value; }
    }

    protected Vector3 localPosition {
        get { return this.transform.localPosition; }
        set { this.transform.localPosition = value; }
    }

    //ギズモ回転
    protected Quaternion rotation {
        get { return this.transform.rotation; }
        set { this.transform.rotation = value; }
    }

    protected Quaternion localRotation {
        get { return this.transform.localRotation; }
        set { this.transform.localRotation = value; }
    }

    // ギズモ拡縮
    protected Vector3 localScale {
        get { return this.transform.localScale; }
        set { this.transform.localScale = value; }
    }

    //差分計算用
    void BkupPos() { _backup_pos = this.position; }
    void BkupRot() { _backup_rot = this.rotation; }
    void BkupScl() { _backup_local_scl = this.localScale; }
    void BkupPosAndRot() { this.BkupPos(); this.BkupRot(); }
    void BkupAll() { this.BkupPos(); this.BkupRot(); this.BkupScl(); }

    //キャンセル復帰用
    protected Vector3 _predrag_pos = Vector3.zero;
    protected Quaternion _predrag_rot = Quaternion.identity;
    protected Vector3 _predrag_local_pos = Vector3.zero;
    protected Quaternion _predrag_local_rot = Quaternion.identity;
    protected Vector3 _predrag_local_scl = Vector3.one;
    protected bool _predrag_state = false;

    public override void Update() {
        if (!targetTrs) {
            return;
        }

        // フレーム毎の状態初期化
        FrameStateInit();

        if (dragnow && !_predrag_state) {
            // ドラッグ開始
            DragStart();
        }else if(!dragnow && _predrag_state) {
            // ドラッグ終了
            if (dragMove) {
                DragEnd();
            }
        }

        // ドラッグキャンセル判定
        if (this.isDragUndo) {
            DragCancel();
        }

        // 通常Update関数
        base.Update();

        // 
        if (dragnow) {
            // ドラッグ中、ターゲットをギズモに合わせる
            TargetToGizmo();
        } else {
            // 非ドラッグ中、ギズモをターゲットに合わせる
            GizmoToTarget();
        }

        // 現在の情報を次のフレーム用に記憶する
        FrameStateFinish();
    }

    public override void OnRenderObject() {
        // 対称が存在しない時はギズモを表示しない
        if (!targetTrs) {
            if ((int)_fi_beSelectedType.GetValue(this) != 0 && (bool)_fi_local_control_lock_.GetValue(this))
                _fi_local_control_lock_.SetValue(this, false);
            ClearSelectedType();
            return;
        }

        base.OnRenderObject();
    }

    protected virtual void FrameStateInit() {
        dragnow = this.isDrag;

        deltaPos = position - _backup_pos;
        deltaRot = rotation * Quaternion.Inverse(_backup_rot);
        deltaLocalPos = transform.InverseTransformVector(deltaPos);
        deltaLocalRot = Quaternion.Inverse(_backup_rot) * rotation;
        deltaLocalScl = localScale - _backup_local_scl;
    }

    protected virtual void FrameStateFinish() {
        _predrag_state = dragnow;

        BkupAll();
    }

    protected virtual void DragStart() {
        // ドラッグ開始地点を記憶
        _predrag_pos = this.position;
        _predrag_rot = this.rotation;

        if (targetTrs) {
            // ターゲットがあればローカル位置を記憶
            _predrag_local_pos = posTargetTrs.localPosition;
            _predrag_local_rot = rotTargetTrs.localRotation;
            _predrag_local_scl = sclTargetTrs.localScale;
        }

        //
        dragMove = false;
        selectedType = (int)_fi_beSelectedType.GetValue(this);
    }

    void DragEnd() {
        dragEndDelegate();
        selectedType = 0;
    }

    protected virtual void DragCancel() {
        if (targetTrs) {
            // ターゲットがあれば元のローカル位置に戻す
            posTargetTrs.localPosition = _predrag_local_pos;
            rotTargetTrs.localRotation = _predrag_local_rot;
            sclTargetTrs.localScale = _predrag_local_scl;

            GizmoToTarget();
        } else {
            // ターゲットがなければ元の位置に戻す
            this.position = _predrag_pos;
            this.rotation = _predrag_rot;
            transform.localScale = Vector3.one;
        }

        // ギズモのドラッグを終了する
        _fi.SetValue(null, false);
        _fi_local_control_lock_.SetValue(null, false);
        ClearSelectedType();

        //
        dragnow = false;
        dragMove = false;
    }

    protected virtual void TargetToGizmo() {
        float rate = 1.0f;
        if (smallMoveKey != KeyCode.None && Input.GetKey(smallMoveKey)){
            rate = 0.1f;
        } else if (bigMoveKey != KeyCode.None && Input.GetKey(bigMoveKey)) {
            rate = 10.0f;
        }
        switch (coordinate) {
            case COORDINATE.Local:
                posTargetTrs.position += (posTargetTrs.TransformVector( deltaLocalPos).normalized * deltaLocalPos.magnitude) * rate;
                rotTargetTrs.rotation = rotTargetTrs.rotation * deltaLocalRot;
                sclTargetTrs.localScale += deltaLocalScl * rate;
                if(deltaLocalPos != Vector3.zero ||
                    deltaLocalRot != Quaternion.identity ||
                    deltaLocalScl != Vector3.zero
                    ) {
                    dragMove = true;
                }
                break;
            case COORDINATE.World:
                posTargetTrs.position += deltaPos * rate;
                rotTargetTrs.rotation = deltaRot * rotTargetTrs.rotation;
                if(deltaPos != Vector3.zero || deltaRot != Quaternion.identity) {
                    dragMove = true;
                }
                break;
            case COORDINATE.View:
                posTargetTrs.position += deltaPos * rate;
                rotTargetTrs.rotation = deltaRot * rotTargetTrs.rotation;
                if (deltaPos != Vector3.zero || deltaRot != Quaternion.identity) {
                    dragMove = true;
                }
                break;
            default:
                break;
        }
    }

    void GizmoToTarget() {
        switch (coordinate) {
            case COORDINATE.Local:
                position = posTargetTrs.position;
                rotation = rotTargetTrs.rotation;
                transform.localScale = Vector3.one;
                break;
            case COORDINATE.World:
                position = posTargetTrs.position;
                rotation = Quaternion.identity;
                transform.localScale = Vector3.one;
                break;
            case COORDINATE.View:
                position = posTargetTrs.position;
                transform.localScale = Vector3.one;

                Transform cameraTrs = Camera.main.transform;
                rotation = Quaternion.LookRotation(position - cameraTrs.position, cameraTrs.up);
                break;
            default:
                break;
        }
    }

    bool isDragUndo {
        get {
            if (!_predrag_state || !eDragUndo)
                return false;
            return Input.GetMouseButton(1) || Input.GetKey(KeyCode.Escape);
        }
    }

    //ドラッグ判定、複数ギズモを表示中でも個別判定できるようにした
    bool isDrag {
        get {
            if (!this.Visible)
                return false;

            if (_fi != null && _fi_beSelectedType != null) {
                object obj = _fi.GetValue(this);
                if (obj is bool && (bool)obj) {
                    object obj2 = _fi_beSelectedType.GetValue(this);
                    if (obj2 is Enum && (int)obj2 != 0) {
                        //GizmoRender.MOVETYPE.NONE以外ならこのギズモのどこかをドラッグ中
                        return true;
                    }
                }
            }
            return false;
        }
    }

    void ClearSelectedType() {
        _fi_beSelectedType.SetValue(this, 0);
    }

    //ドラッグエンド判定用（変化を見るだけなので毎フレーム呼び出す必要あり）
    bool isDragEnd {
        get {
            bool drag = this.isDrag;
            if (drag != _isdrag_bk) {
                _isdrag_bk = drag;
                if (drag == false)
                    return true;
            }
            return false;
        }
    }

    void DragBkup() {
        {
            _isdrag_bk = this.isDrag;
        }
    }

    public ExGizmoRenderer() {
        //beSelectedType
        if (_fi_beSelectedType == null)
            _fi_beSelectedType = typeof(GizmoRender).GetField("beSelectedType", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

        if (_fi == null)
            _fi = typeof(GizmoRender).GetField("is_drag_", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);

        if (_fi_local_control_lock_ == null)
            _fi_local_control_lock_ = typeof(GizmoRender).GetField("local_control_lock_", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.DeclaredOnly);
    }

    public virtual void SetTarget(Transform t) {
        targetTrs = t;
    }

    public void SetVisible(bool fVisible) {
        Visible = fVisible;
    }

    public void SetCoordinate(COORDINATE fCoordinate) {
        coordinate = fCoordinate;
    }

    public Vector3 GetBackUpLocalPosition() {
        return _predrag_local_pos;
    }

    public Quaternion GetBackUpLocalRotation() {
        return _predrag_local_rot;
    }

    public Vector3 GetBackUpLocalScale() {
        return _predrag_local_scl;
    }

    public int GetSelectedType() {
        return selectedType;
    }

    //ギズモ作成補助
    static public List<GameObject> _gameObjects_ = new List<GameObject>();
    static public ExGizmoRenderer AddGizmo(Transform parent_tr, string gizmo_name) {
        GameObject go = new GameObject();
        _gameObjects_.Add(go);
        go.transform.SetParent(parent_tr, false);
        go.name = gizmo_name;

        ExGizmoRenderer mg = go.AddComponent<ExGizmoRenderer>();
        mg.name = gizmo_name + "_GR";
        return mg;
    }

    public enum COORDINATE {
        Local,
        World,
        View
    };
}
