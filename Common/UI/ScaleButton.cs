
using UnityEngine;

// UIWindowのサイズ変更用
internal class ScaleButton {
    #region メンバ変数
    static private float size = 20f;
    private bool isDrag = false;
    private LeftRight horizon;
    private UpperBottom vertical;
    private UIWindow parentWindow;
    #endregion

    #region コンストラクタ
    public ScaleButton(UIWindow parentWindow, LeftRight horizon, UpperBottom vertical) {
        this.parentWindow = parentWindow;
        this.horizon = horizon;
        this.vertical = vertical;
    }
    #endregion

    #region public関数
    public void Drag() {
        // ドラッグ中じゃなければ終了
        if (!isDrag) return;

        Vector2 position = Input.mousePosition;

        // 水平方向
        if (horizon == LeftRight.Left) {
            parentWindow.ExtendLeft(position.x - size / 2);

            if (parentWindow.GetMinSize().x > parentWindow.GetRect().width) {
                parentWindow.ExtendRight(parentWindow.GetRect().x + parentWindow.GetMinSize().x);
            }
        } else {
            parentWindow.ExtendRight(position.x + size / 2);

            float dWindowWidth = parentWindow.GetMinSize().x - parentWindow.GetRect().width;
            if (dWindowWidth > 0) {
                parentWindow.ExtendLeft(parentWindow.GetRect().x - dWindowWidth);
            }
        }

        // 垂直方向
        if (vertical == UpperBottom.Upper) {
            parentWindow.ExtendUpper(Screen.height - position.y - size / 2);

            if (parentWindow.GetMinSize().y > parentWindow.GetRect().height) {
                parentWindow.ExtendBottom(parentWindow.GetRect().y + parentWindow.GetMinSize().y);
            }
        } else {
            parentWindow.ExtendBottom(Screen.height - position.y + size / 2);

            float dWindowHeight = parentWindow.GetMinSize().y - parentWindow.GetRect().height;
            if (dWindowHeight > 0) {
                parentWindow.ExtendUpper(parentWindow.GetRect().y - dWindowHeight);
            }
        }
    }

    public void Draw() {
        // ボタンの位置設定から座標を設定
        Rect windowRect = parentWindow.GetRect();
        Rect scaleButtonPosition = new Rect(0f, 0f, size, size);
        if (horizon == ScaleButton.LeftRight.Right) {
            scaleButtonPosition.x = windowRect.width - size;
        }
        if (vertical == ScaleButton.UpperBottom.Bottom) {
            scaleButtonPosition.y = windowRect.height - size;
        }

        // GUI操作
        bool buttonDown = GUI.RepeatButton(scaleButtonPosition, "□");

        if(!isDrag && buttonDown) {
            isDrag = true;
        }

        if((Event.current.type == EventType.Repaint) && isDrag && !buttonDown) {
            isDrag = false;
        }
    }
    #endregion

    #region Enum
    public enum LeftRight {
        Left,
        Right
    }

    public enum UpperBottom {
        Upper,
        Bottom
    }
    #endregion
}
