using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

internal class ComboBoxBase {
    protected static bool forceToUnShow = false;
    protected static int useControlID = -1;
    protected bool isClickedComboButton = false;
    protected int selectedItemIndex = 0;
    protected int backSelectedItemIndex = 0;

    protected float itemWidth;
    protected float itemHeight;

    protected GUIContent buttonContent;
    protected GUIContent[] listContent;
    protected GUIStyle buttonStyle;
    protected GUIStyle boxStyle;
    protected GUIStyle listStyle;

    protected ComboBoxBase(GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle)
        : this(buttonContent, listContent, "button", "box", listStyle) {
    }

    protected ComboBoxBase(GUIContent buttonContent, GUIContent[] listContent,
                           GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle) {
        this.buttonContent = buttonContent;
        this.listContent = listContent;
        this.buttonStyle = buttonStyle;
        this.boxStyle = boxStyle;
        this.listStyle = listStyle;
        InitIndex();
        InitSize();
    }

    protected void InitSize() {
        int maxLength = 0;
        foreach (GUIContent c in listContent) {
            if (maxLength < c.text.Length) maxLength = c.text.Length;
        }
        itemWidth = maxLength * 9f;
        itemHeight = listStyle.CalcHeight(listContent[0], 1.0f);

    }
    protected void InitIndex() {
        for (int i = 0; i < listContent.Length; i++) {
            if (buttonContent.text == listContent[i].text) {
                selectedItemIndex = i;
                return;
            }
        }
        selectedItemIndex = -1;
    }
    public int SelectItem(string item) {
        string itemLow = item.ToLower();
        for (int i = 0; i < listContent.Length; i++) {
            if (listContent[i].text.ToLower() == itemLow) {
                selectedItemIndex = i;
                return i;
            }
        }
        return -1;
    }
    public bool IsClickedComboButton {
        get { return isClickedComboButton; }
    }

    public int ItemCount {
        get { return listContent.Length; }
    }

    // Wysokosc jednego wiersza listy (z czcionki stylu listy), ta sama dla rysowania i dla rozmiaru okienka.
    public float RowHeight {
        get { return Mathf.Max(itemHeight, 18f); }
    }

    // Szerokosc najdluzszej pozycji listy przy czcionce stylu listy.
    public float PreferredListWidth() {
        float max = 0f;
        for (int i = 0; i < listContent.Length; i++) {
            float w = listStyle.CalcSize(listContent[i]).x;
            if (w > max) max = w;
        }
        return max;
    }

    public int SelectedItemIndex {
        get { return selectedItemIndex; }
        set {
            if (selectedItemIndex != value) {
                if (value < listContent.Length && value >= 0) {
                    selectedItemIndex = value;
                    buttonContent = listContent[selectedItemIndex];
                } else {
                    buttonContent = GUIContent.none;
                    selectedItemIndex = -1;
                }
            }
        }
    }

    protected Vector2 scrollPosition = Vector2.zero;
}

internal class ComboBox : ComboBoxBase {
    public Rect rect;

    public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle)
        : base(buttonContent, listContent, listStyle) {
        this.rect = rect;
    }

    public ComboBox(Rect rect, GUIContent buttonContent, GUIContent[] listContent, GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle)
        : base(buttonContent, listContent, buttonStyle, boxStyle, listStyle) {
        this.rect = rect;
    }

    public int Show() {
        if (forceToUnShow) {
            forceToUnShow = false;
            isClickedComboButton = false;
        }

        bool done = false;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        switch (Event.current.GetTypeForControl(controlID)) {
            case EventType.MouseUp:
                done |= isClickedComboButton;
                break;
        }

        if (GUI.Button(rect, buttonContent, buttonStyle)) {
            if (useControlID == -1) {
                useControlID = controlID;
                isClickedComboButton = false;
            }

            if (useControlID != controlID) {
                forceToUnShow = true;
                useControlID = controlID;
            }
            isClickedComboButton = true;
        }

        if (isClickedComboButton) {
            var listRect = new Rect(rect.x, rect.y + itemHeight,
                      rect.width, itemHeight * listContent.Length);

            GUI.Box(listRect, string.Empty, boxStyle);
            int newSelectedItemIndex = GUI.SelectionGrid(listRect, selectedItemIndex, listContent, 1, listStyle);
            if (newSelectedItemIndex != selectedItemIndex) {
                SelectedItemIndex = newSelectedItemIndex;
            }
        }

        isClickedComboButton &= !done;
        return selectedItemIndex;
    }
}

/// <summary>
/// GUILayout版のコンボボックスクラス. 
/// </summary>
internal class ComboBoxLO : ComboBoxBase {
    private readonly bool labelFixed;
    public ComboBoxLO(GUIContent buttonContent, GUIContent[] listContent, GUIStyle listStyle)
        : base(buttonContent, listContent, listStyle) {
    }

    public ComboBoxLO(GUIContent buttonContent, GUIContent[] listContent,
                      GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle, bool labelFixed)
        : base(buttonContent, listContent, buttonStyle, boxStyle, listStyle) {
        this.labelFixed = labelFixed;
    }
    public void SetItemWidth(float itemWidth) {
        this.itemWidth = itemWidth;
    }
    public int Show(GUILayoutOption buttonOpt) {
        if (forceToUnShow) {
            forceToUnShow = false;
            isClickedComboButton = false;
        }

        bool done = false;
        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        switch (Event.current.GetTypeForControl(controlID)) {
            case EventType.MouseUp:
                done |= isClickedComboButton;
                break;
        }

        bool expand = isClickedComboButton;
        if (expand) GUILayout.BeginVertical(boxStyle, GUILayout.Width(itemWidth));
        try {
            if (GUILayout.Button(buttonContent, buttonStyle, buttonOpt, GUILayout.Width(itemWidth))) {
                if (useControlID == -1) {
                    useControlID = controlID;
                    isClickedComboButton = false;
                }

                if (useControlID != controlID) {
                    forceToUnShow = true;
                    useControlID = controlID;
                }
                isClickedComboButton = true;
            }

            if (isClickedComboButton) {
                float height = itemHeight * listContent.Length;
                int newSelectedItemIndex = GUILayout.SelectionGrid(selectedItemIndex, listContent, 1, listStyle,
                                                                   GUILayout.Width(itemWidth), GUILayout.Height(height));
                if (newSelectedItemIndex != selectedItemIndex) {
                    // ラベル指定に応じて
                    if (!labelFixed) {
                        SelectedItemIndex = newSelectedItemIndex;
                    } else {
                        selectedItemIndex = newSelectedItemIndex;
                    }
                }
            }
        } finally {
            if (expand) GUILayout.EndVertical();
        }

        isClickedComboButton &= !done;
        return selectedItemIndex;
    }

    // Pozycja przycisku zapamietana podczas rysowania: GetLastRect w zdarzeniu myszy (klikniecie) nie daje poprawnej pozycji.
    Rect drawnButtonRect;

    public int ShowScroll(GUILayoutOption buttonOpt) {
        bool clicked = GUILayout.Button(buttonContent, buttonStyle, buttonOpt, GUILayout.MinWidth(90));
        if (Event.current.type == EventType.Repaint) drawnButtonRect = GUILayoutUtility.GetLastRect();
        if (clicked) {
            if (ComboPopup.IsOpen(this)) {
                ComboPopup.Cancel();
            } else {
                backSelectedItemIndex = selectedItemIndex;
                Rect local = drawnButtonRect;
                Vector2 relative;
                if (local.width > 1f && local.height > 1f) {
                    // Lista otwiera sie tuz pod przyciskiem, wyrownana do jego lewej krawedzi (a nie w miejscu klikniecia).
                    relative = new Vector2(local.x, local.yMax);
                } else {
                    // awaryjnie (przycisk jeszcze nie narysowany): pod kursorem, jak dawniej
                    Vector2 mouse = Event.current.mousePosition;
                    local = new Rect(mouse.x - 45f, mouse.y - 9f, 90f, 18f);
                    relative = new Vector2(local.x, local.yMax);
                }
                ComboPopup.Open(this, relative, new Vector2(Mathf.Max(local.width, 90f), Mathf.Max(local.height, 18f)));
            }
        }
        return selectedItemIndex;
    }

    public void CancelPopup() {
        selectedItemIndex = backSelectedItemIndex;
        if (selectedItemIndex >= 0 && selectedItemIndex < listContent.Length) buttonContent = listContent[selectedItemIndex];
    }

    public int DrawPopupList() {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        float height = RowHeight * listContent.Length;
        int picked = GUILayout.SelectionGrid(selectedItemIndex, listContent, 1, listStyle, GUILayout.Height(height), GUILayout.ExpandWidth(true));
        GUILayout.EndScrollView();
        return picked;
    }
}

internal static class ComboPopup {
    public static Vector2 windowOffset;
    static ComboBoxLO current;
    static Vector2 relativeToWindow;
    static Rect buttonScreen;
    static Rect popupScreen;
    const int PopupId = 875421;

    public static bool IsOpen(ComboBoxLO box) {
        return current == box;
    }

    public static void Open(ComboBoxLO box, Vector2 relative, Vector2 buttonSize) {
        current = box;
        relativeToWindow = relative;
        buttonSizeStored = buttonSize;
        Place();
    }

    static Vector2 buttonSizeStored;

    static void Place() {
        Vector2 abs = windowOffset + relativeToWindow;
        // relativeToWindow to lewy dolny rog przycisku, lista zaczyna sie pod nim
        buttonScreen = new Rect(abs.x, abs.y - buttonSizeStored.y, buttonSizeStored.x, buttonSizeStored.y);
        // Rozmiar okienka z zawartosci: szerokosc najdluzszej pozycji (+ pasek przewijania i marginesy), wysokosc z liczby
        // wierszy; ograniczone do czesci ekranu, dalej przewijanie.
        float width = Mathf.Max(buttonSizeStored.x, 120f);
        float height = 40f;
        if (current != null) {
            width = Mathf.Max(width, current.PreferredListWidth() + 40f);
            height = current.RowHeight * current.ItemCount + 16f;
        }
        width = Mathf.Min(width, Screen.width * 0.6f);
        height = Mathf.Min(height, Screen.height * 0.6f);
        popupScreen = new Rect(abs.x, abs.y, width, height);
        if (popupScreen.xMax > Screen.width) popupScreen.x = Screen.width - popupScreen.width;
        // brak miejsca pod przyciskiem: lista otwiera sie nad nim
        if (popupScreen.yMax > Screen.height) popupScreen.y = abs.y - buttonSizeStored.y - popupScreen.height;
        if (popupScreen.y < 0f) popupScreen.y = 0f;
    }

    public static void Cancel() {
        if (current != null) current.CancelPopup();
        current = null;
    }

    public static void Draw() {
        if (current == null) return;
        Place();
        popupScreen = GUI.Window(PopupId, popupScreen, DrawWindow, "", PopupStyle());
        Event e = Event.current;
        if (e.type == EventType.MouseDown && !popupScreen.Contains(e.mousePosition) && !buttonScreen.Contains(e.mousePosition)) {
            Cancel();
            e.Use();
        }
    }

    static GUIStyle popupStyle;
    static GUIStyle PopupStyle() {
        // tekstura tla moze zostac zniszczona przez gre (zmiana sceny / UnloadUnusedAssets): wtedy lista bylaby przezroczysta,
        // wiec odtwarzamy styl, gdy tla juz nie ma
        if (popupStyle != null && popupStyle.normal.background != null) return popupStyle;
        popupStyle = new GUIStyle(UIParams.Instance.winStyle);
        Texture2D bg = new Texture2D(2, 2);
        bg.hideFlags = HideFlags.HideAndDontSave;
        Color dark = new Color(0.08f, 0.08f, 0.08f, 0.96f);
        bg.SetPixels(new Color[] { dark, dark, dark, dark });
        bg.Apply();
        popupStyle.normal.background = bg;
        popupStyle.onNormal.background = bg;
        popupStyle.hover.background = bg;
        popupStyle.active.background = bg;
        popupStyle.focused.background = bg;
        popupStyle.onHover.background = bg;
        popupStyle.onActive.background = bg;
        popupStyle.onFocused.background = bg;
        // ramka okna ze skorki jest wieksza niz tekstura 2x2 (inaczej tlo sie nie rysuje)
        popupStyle.border = new RectOffset(0, 0, 0, 0);
        popupStyle.padding = new RectOffset(4, 4, 4, 4);
        return popupStyle;
    }

    static void DrawWindow(int id) {
        if (current == null) return;
        int picked = current.DrawPopupList();
        if (picked != current.SelectedItemIndex && picked >= 0) {
            current.SelectedItemIndex = picked;
            current = null;
            Event.current.Use();
        }
    }
}

