using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

internal class UIWindow : MonoBehaviour {
    #region メンバ変数
    bool isVisible = false;
    public bool IsVisible { get { return isVisible; } }
    string title;
    int windowID;

    // GUI設定
    Rect windowRect = new Rect(20, 20, 400, 600);
    Vector2 scrollPosition = Vector2.zero;
    Vector2 minSize = new Vector2(100, 100);
    ScaleButton[] scaleButton;

    UIParams uiParams = UIParams.Instance;

    UnityEvent drawEvent = new UnityEvent();
    List<IUIDrawer> drawerList = new List<IUIDrawer>();

    UnityEvent endEvent = new UnityEvent();
    #endregion

    #region イベント関数
    void Start() {
        scaleButton = new ScaleButton[] {
            new ScaleButton(this, ScaleButton.LeftRight.Left, ScaleButton.UpperBottom.Upper),
            //new ScaleButton(this, ScaleButton.LeftRight.Right, ScaleButton.UpperBottom.Bottom)
        };
    }

    void Update() {
        foreach(ScaleButton sb in scaleButton) {
            sb.Drag();
        }

        if (isVisible && windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) {
            //クリックを解除
            if (GetMouseInput()) {
                Input.ResetInputAxes();
            }
        }
    }

    void OnGUI() {
        if (!isVisible) return;
        uiParams.EnsureSkin();
        uiParams.Update();
        windowRect = GUI.Window(windowID, windowRect, DoMyWindow, title, uiParams.winStyle);
        ComboPopup.Draw();

            if (windowRect.Contains(new Vector2(Input.mousePosition.x, Screen.height - Input.mousePosition.y))) {
                if (GetMouseInput()) {
                    Input.ResetInputAxes();
                }
            }
    }
    #endregion

    #region public関数
    // 変化しない名前をタイトルにつける場合
    public void SetWindowInfo(string windowName) {
        title = windowName;
        windowID = windowName.GetHashCode();
    }

    // バージョン情報などをタイトルに含む場合は、
    // バージョンが変わるごとにIDが変化しないように、ID計算用の固定の名前と追加情報を分ける
    public void SetWindowInfo(string windowName, string additionalInfo) {
        title = windowName + " " + additionalInfo;
        windowID = windowName.GetHashCode();
    }

    // 名前に関係ないIDを使いたい場合、または自分で計算する場合
    public void SetWindowInfo(string windowName, int windowID) {
        title = windowName;
        this.windowID = windowID;
    }

    public void AddItem(IUIDrawer item) {
        drawerList.Add(item);
    }

    public void SetPosition(Vector2 position) {
        windowRect.position = position;
    }

    public void SetSize(Vector2 size) {
        windowRect.size = size;
    }

    public void SetVisible(bool visible) {
        if(isVisible == true && visible == false) {
            endEvent.Invoke();
        }
        isVisible = visible;

    }

    public Rect GetRect() {
        return windowRect;
    }

    public Vector2 GetMinSize() {
        return minSize;
    }

    public void ExtendLeft(float x) {
        float deltaX = windowRect.x - x;
        windowRect.x = x;
        windowRect.width += deltaX;
    }

    public void ExtendRight(float x) {
        windowRect.width = x - windowRect.x;
    }

    public void ExtendUpper(float y) {
        float deltaY = windowRect.y - y;
        windowRect.y = y;
        windowRect.height += deltaY;
    }

    public void ExtendBottom(float y) {
        windowRect.height = y - windowRect.y;
    }

    public void AddDrawEvent(UnityAction call) {
        drawEvent.AddListener(call);
    }

    public void AddEndEvent(UnityAction call) {
        endEvent.AddListener(call);
    }
    #endregion

    #region private関数
    void DoMyWindow(int windowID) {
        DrawCloseButton();
        foreach (ScaleButton sb in scaleButton) {
            sb.Draw();
        }
        GUI.DragWindow(new Rect(20f, 0, windowRect.width - 40f, 20f));
        GUILayout.Label("");
        ComboPopup.windowOffset = new Vector2(windowRect.x, windowRect.y);
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        //drawerList.ForEach(d => d.DrawItem());
        drawEvent.Invoke();
        GUILayout.EndScrollView();
    }

    void DrawCloseButton() {
        Rect closeButtonPosition = new Rect(windowRect.width - 20f, 0f, 20f, 20f);
        if (GUI.Button(closeButtonPosition, "×", uiParams.bStyle)) {
            SetVisible(false);
        }
    }

    bool GetMouseInput() {
        if (Input.GetMouseButtonDown(0) ||
            Input.GetMouseButtonDown(1) ||
            Input.GetMouseButtonDown(2) ||
            Input.GetMouseButtonUp(0) ||
            Input.GetMouseButtonUp(1) ||
            Input.GetMouseButtonUp(2) ||
            Input.GetAxis("Mouse ScrollWheel") != 0f) {

            return true;
        }
        
        return false;
    }
    #endregion
}

