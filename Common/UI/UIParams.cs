using System;
using System.Collections.Generic;
using UnityEngine;

internal class UIParams {
    private static UIParams instance = new UIParams();
    public static UIParams Instance {
        get { return instance; }
    }
    #region Constants
    private const int marginPx = 2;
    private const int marginLPx = 10;
    private const int itemHeightPx = 18;
    private const int fontPx = 14;
    private const int fontPxS = (int)(fontPx * 0.9f);
    private const int fontPxSS = (int)(fontPx * 0.8f);
    private const int fontPxL = 20;
    #endregion

    private int width;
    private int height;
    private float ratio;

    public int margin;
    public int marginL; //
    public int fontSize;
    public int fontSizeS;
    public int fontSizeSS;
    public int fontSizeL;
    public int itemHeight;
    public int unitHeight;
    public readonly GUIStyle lStyle = new GUIStyle();
    // bold
    public readonly GUIStyle lStyleB = new GUIStyle();
    // colored
    public readonly GUIStyle lStyleC = new GUIStyle();
    // small
    public readonly GUIStyle lStyleS = new GUIStyle();
    public readonly GUIStyle lStyleRS = new GUIStyle();

    public readonly GUIStyle bStyle = new GUIStyle();
    public readonly GUIStyle bStyleSC = new GUIStyle();
    public readonly GUIStyle bStyleL = new GUIStyle();
    // przyciski grup wyboru (zamiast radio-buttonow): wcisniety = kolor wyrozniajacy, reszta wylaczona
    public readonly GUIStyle tbStyle = new GUIStyle();

    public readonly GUIStyle tStyle = new GUIStyle();
    public readonly GUIStyle tStyleS = new GUIStyle();
    public readonly GUIStyle tStyleSS = new GUIStyle();
    public readonly GUIStyle tStyleL = new GUIStyle();
    public readonly GUIStyle listStyle = new GUIStyle();
    public readonly GUIStyle textStyle = new GUIStyle();
    public readonly GUIStyle textStyleSC = new GUIStyle();
    public readonly GUIStyle textAreaStyleS = new GUIStyle();

    public readonly GUIStyle boxStyle = new GUIStyle();
    public readonly GUIStyle winStyle = new GUIStyle();
    public readonly GUIStyle dialogStyle = new GUIStyle();
    public readonly GUIStyle tipsStyle = new GUIStyle();

    public readonly Color textColor = new Color(1f, 1f, 1f, 0.98f);

    public Rect titleBarRect = new Rect();

    public Rect winRect = new Rect();
    public Rect fileBrowserRect = new Rect();
    public Rect modalRect = new Rect();

    public Rect mainRect = new Rect();
    public Rect colorRect = new Rect();
    public Rect nodeSelectRect = new Rect();
    public Rect presetSelectRect = new Rect();
    public Rect textureRect = new Rect();
    public Rect labelRect = new Rect();
    public Rect subRect = new Rect();
    public GUILayoutOption optBtnHeight;
    public float subConWidth;
    public GUILayoutOption optSubConWidth;
    public GUILayoutOption optSubConHeight;
    public GUILayoutOption optSubCon6Height;
    public GUILayoutOption optSubConHalfWidth;
    public GUILayoutOption optBtnWidth;
    public GUILayoutOption optCategoryWidth;
    public GUILayoutOption optDBtnWidth;
    public GUILayoutOption optSLabelWidth;

    public GUILayoutOption optContentWidth;

    public UIParams() {
        listStyle.onHover.background = listStyle.hover.background = new Texture2D(2, 2);
        listStyle.padding.left = listStyle.padding.right = 4;
        listStyle.padding.top = listStyle.padding.bottom = 1;
        listStyle.normal.textColor = listStyle.onNormal.textColor =
            listStyle.hover.textColor = listStyle.onHover.textColor =
            listStyle.active.textColor = listStyle.onActive.textColor = Color.white;
        listStyle.focused.textColor = listStyle.onFocused.textColor = Color.blue;

        TextAnchor txtAlignment = TextAnchor.MiddleLeft;
        // Bold
        lStyleB.fontStyle = FontStyle.Bold;
        lStyleB.alignment = txtAlignment;

        lStyle.fontStyle = FontStyle.Normal;
        lStyle.normal.textColor = textColor;
        lStyle.alignment = txtAlignment;
        //lStyle.wordWrap          = false;

        lStyleS.fontStyle = FontStyle.Normal;
        lStyleS.normal.textColor = textColor;
        lStyleS.alignment = txtAlignment;

        lStyleRS.fontStyle = FontStyle.Normal;
        lStyleRS.normal.textColor = textColor;
        lStyleRS.alignment = TextAnchor.MiddleRight;

        lStyleC.fontStyle = FontStyle.Normal;
        lStyleC.normal.textColor = new Color(0.82f, 0.88f, 1f, 0.98f);
        lStyleC.alignment = txtAlignment;

        bStyle.normal.textColor = textColor;
        bStyleSC.normal.textColor = textColor;
        bStyleSC.alignment = TextAnchor.MiddleCenter;
        bStyleL.normal.textColor = textColor;
        bStyleL.alignment = TextAnchor.MiddleLeft;

        tStyle.normal.textColor = textColor;
        tStyleS.normal.textColor = textColor;
        tStyleS.alignment = TextAnchor.LowerLeft;
        tStyleSS.normal.textColor = textColor;
        tStyleSS.alignment = TextAnchor.MiddleLeft;

        tStyleL.normal.textColor = textColor;
        tStyleL.alignment = txtAlignment;
        //        var tex2D = new Texture2D(2, 2);
        //        tex2D.SetPixels(new Color[]{Color.white, Color.white});
        //        tStyleL.onHover.background = tStyleL.hover.background = tex2D;
        //        var tbgray = new Texture2D(2, 2);
        //        tbgray.SetPixels(new Color[]{Color.gray, Color.white});
        //        tStyleL.focused.background = tStyleL.onFocused.background = tbgray;
        //        var tex2 = new Texture2D(2, 2);
        //        tex2.SetPixels(new Color[]{Color.gray, Color.grey});
        //        tStyleL.active.background = tStyleL.onActive.background = tex2;
        //tStyle.stretchWidth = true;

        textStyle.normal.textColor = textColor;
        textStyleSC.normal.textColor = textColor;
        textStyleSC.alignment = TextAnchor.MiddleCenter;
        textAreaStyleS.normal.textColor = textColor;

        //winStyle.alignment = TextAnchor.UpperRight;
        dialogStyle.alignment = TextAnchor.UpperCenter;
        dialogStyle.normal.textColor = textColor;
        tipsStyle.alignment = TextAnchor.MiddleCenter;
        tipsStyle.wordWrap = true;
    }

    bool skinReady;
    bool tbBuilt;
    public void EnsureSkin() {
        if (!tbBuilt) {
            tbBuilt = true;
            BuildToolbarStyle(CM3D2.PartsEdit.Plugin.Setting.highlightColor.GetValue());
        }
        if (skinReady) return;
        if (GUI.skin == null || GUI.skin.button == null || GUI.skin.button.normal.background == null) return;
        CopySkin(bStyle, GUI.skin.button);
        CopySkin(bStyleSC, GUI.skin.button);
        CopySkin(bStyleL, GUI.skin.button);
        CopySkin(tStyle, GUI.skin.toggle);
        CopySkin(tStyleS, GUI.skin.toggle);
        CopySkin(tStyleSS, GUI.skin.toggle);
        CopySkin(tStyleL, GUI.skin.toggle);
        CopySkin(textStyle, GUI.skin.textField);
        CopySkin(textStyleSC, GUI.skin.textField);
        CopySkin(textAreaStyleS, GUI.skin.textArea);
        CopySkin(boxStyle, GUI.skin.box);
        CopySkin(winStyle, GUI.skin.window);
        CopySkin(dialogStyle, GUI.skin.window);
        CopySkin(lStyle, GUI.skin.label);
        CopySkin(lStyleB, GUI.skin.label);
        CopySkin(lStyleS, GUI.skin.label);
        CopySkin(lStyleRS, GUI.skin.label);
        CopySkin(lStyleC, GUI.skin.label);
        bStyle.normal.textColor = textColor;
        bStyleSC.normal.textColor = textColor;
        bStyleL.normal.textColor = textColor;
        tStyle.normal.textColor = textColor;
        lStyle.normal.textColor = textColor;
        lStyle.alignment = TextAnchor.MiddleLeft;
        lStyleB.fontStyle = FontStyle.Bold;
        winStyle.fontSize = fontSize > 0 ? fontSize : 14;
        skinReady = true;
        ApplyHighlight(CM3D2.PartsEdit.Plugin.Setting.highlightColor.GetValue());
    }

    public void ApplyHighlight(Color color) {
        Texture2D tex = new Texture2D(2, 2);
        Color[] pixels = new Color[] { color, color, color, color };
        tex.SetPixels(pixels);
        tex.Apply();
        listStyle.onNormal.background = tex;
        listStyle.onHover.background = tex;
        listStyle.onActive.background = tex;
        listStyle.hover.textColor = color;
        listStyle.onHover.textColor = Color.white;
        listStyle.onNormal.textColor = Color.white;
        listStyle.onActive.textColor = Color.white;
        Color text = color;
        tStyle.onNormal.textColor = text;
        tStyle.onHover.textColor = text;
        tStyle.onActive.textColor = text;
        tStyle.onFocused.textColor = text;
        tStyle.hover.textColor = text;
        tStyleS.onNormal.textColor = text;
        tStyleS.onHover.textColor = text;
        tStyleSS.onNormal.textColor = text;
        tStyleL.onNormal.textColor = text;
        textStyle.focused.textColor = text;
        textStyle.onFocused.textColor = text;
        textStyle.active.textColor = text;
        bStyle.hover.textColor = text;
        bStyle.onHover.textColor = text;
        BuildToolbarStyle(color);
    }

    // Styl przyciskow grup wyboru (GUILayout.Toolbar). Wlasne tekstury (nie zaleza od skorki gry): ciemny przycisk z jasniejsza
    // ramka, najechany jasniejszy, zaznaczony wypelniony kolorem wyrozniajacym (ten sam co podswietlenie list).
    readonly Texture2D[] tbTextures = new Texture2D[4];
    void BuildToolbarStyle(Color accent) {
        for (int i = 0; i < tbTextures.Length; i++) {
            if (tbTextures[i]) UnityEngine.Object.Destroy(tbTextures[i]);
        }
        Color edge = new Color(0.45f, 0.45f, 0.50f, 1f);
        tbTextures[0] = ButtonTexture(new Color(0.17f, 0.17f, 0.19f, 0.96f), edge);
        tbTextures[1] = ButtonTexture(new Color(0.28f, 0.28f, 0.32f, 0.96f), new Color(0.78f, 0.78f, 0.84f, 1f));
        tbTextures[2] = ButtonTexture(new Color(0.10f, 0.10f, 0.12f, 0.96f), edge);
        Color accentOpaque = new Color(accent.r, accent.g, accent.b, 1f);
        tbTextures[3] = ButtonTexture(accentOpaque, Color.Lerp(accentOpaque, Color.white, 0.5f));

        tbStyle.normal.background = tbTextures[0];
        tbStyle.hover.background = tbTextures[1];
        tbStyle.active.background = tbTextures[2];
        tbStyle.focused.background = tbTextures[0];
        tbStyle.onNormal.background = tbTextures[3];
        tbStyle.onHover.background = tbTextures[3];
        tbStyle.onActive.background = tbTextures[3];
        tbStyle.onFocused.background = tbTextures[3];

        Color txt = new Color(0.92f, 0.92f, 0.95f, 1f);
        tbStyle.normal.textColor = txt;
        tbStyle.hover.textColor = Color.white;
        tbStyle.active.textColor = Color.white;
        tbStyle.focused.textColor = txt;
        tbStyle.onNormal.textColor = Color.white;
        tbStyle.onHover.textColor = Color.white;
        tbStyle.onActive.textColor = Color.white;
        tbStyle.onFocused.textColor = Color.white;

        tbStyle.border = new RectOffset(1, 1, 1, 1);
        tbStyle.padding = new RectOffset(8, 8, 3, 3);
        tbStyle.margin = new RectOffset(0, 0, 2, 2);
        tbStyle.alignment = TextAnchor.MiddleCenter;
        tbStyle.fontStyle = FontStyle.Normal;
        tbStyle.clipping = TextClipping.Clip;
        tbStyle.fontSize = fontSize > 0 ? fontSize : 14;
    }

    static Texture2D ButtonTexture(Color fill, Color edge) {
        Texture2D tex = new Texture2D(3, 3, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < 3; y++) {
            for (int x = 0; x < 3; x++) {
                tex.SetPixel(x, y, (x == 0 || x == 2 || y == 0 || y == 2) ? edge : fill);
            }
        }
        tex.Apply();
        return tex;
    }

    static void CopySkin(GUIStyle dst, GUIStyle src) {
        dst.normal.background = src.normal.background;
        dst.hover.background = src.hover.background;
        dst.active.background = src.active.background;
        dst.focused.background = src.focused.background;
        dst.onNormal.background = src.onNormal.background;
        dst.onHover.background = src.onHover.background;
        dst.onActive.background = src.onActive.background;
        dst.onFocused.background = src.onFocused.background;
        dst.normal.textColor = src.normal.textColor;
        dst.border = src.border;
        dst.padding = src.padding;
        dst.margin = src.margin;
        dst.overflow = src.overflow;
    }

    public void Update() {
        bool screenSizeChanged = false;

        if (Screen.height != height) {
            height = Screen.height;
            screenSizeChanged = true;
        }
        if (Screen.width != width) {
            width = Screen.width;
            screenSizeChanged = true;
        }
        if (!screenSizeChanged)
            return;

        ratio = (1.0f + (width / 1280.0f - 1.0f) * 0.6f);

        // 画面サイズが変更された場合にのみ更新
        fontSize = FixPx(fontPx);
        fontSizeS = FixPx(fontPxS);
        fontSizeSS = FixPx(fontPxSS);
        fontSizeL = FixPx(fontPxL);
        margin = FixPx(marginPx);
        marginL = FixPx(marginLPx);
        itemHeight = FixPx(itemHeightPx);
        unitHeight = margin + itemHeight;

        lStyle.fontSize = fontSize;
        lStyleC.fontSize = fontSize;
        lStyleB.fontSize = fontSize;

        lStyleS.fontSize = fontSizeS;
        lStyleRS.fontSize = fontSizeS;

        bStyle.fontSize = fontSize;
        bStyleSC.fontSize = fontSizeS;
        bStyleL.fontSize = fontSize;
        tbStyle.fontSize = fontSize;
        tStyle.fontSize = fontSize;
        tStyleS.fontSize = fontSizeS;
        tStyleSS.fontSize = fontSizeSS;
        tStyleL.fontSize = fontSizeL;
        listStyle.fontSize = fontSizeS;
        textStyle.fontSize = fontSize;
        textStyleSC.fontSize = fontSizeS;
        textAreaStyleS.fontSize = fontSizeS;

        //LogUtil.DebugF("screen=({0},{1}),margin={2},height={3},ratio={4})", width, height, margin, itemHeight, ratio);

        winStyle.fontSize = fontSize;
        tipsStyle.fontSize = fontSize;
        dialogStyle.fontSize = fontSize;
        InitWinRect();
        InitFBRect();
        InitModalRect();

        subConWidth = winRect.width - margin * 2;
        optBtnHeight = GUILayout.Height(itemHeight);
        // sub
        optSubConWidth = GUILayout.Width(subConWidth);
        optSubConHeight = GUILayout.Height(winRect.height - unitHeight * 3f);
        optSubCon6Height = GUILayout.Height(winRect.height - unitHeight * 6.6f);
        optSubConHalfWidth = GUILayout.Width((winRect.width - marginL * 2) * 0.5f); // margin値が小さい前提になってしまっている
        optSLabelWidth = GUILayout.Width(fontSizeS * 6f);

        mainRect.Set(margin, unitHeight * 5 + margin, winRect.width - margin * 2, winRect.height - unitHeight * 6.5f);
        textureRect.Set(margin, unitHeight, winRect.width - margin * 2, winRect.height - unitHeight * 2.5f);
        float baseWidth = textureRect.width - 20;
        optBtnWidth = GUILayout.Width(baseWidth * 0.09f);
        optDBtnWidth = GUILayout.Width(fontSizeS * 5f * 0.6f);
        optContentWidth = GUILayout.MaxWidth(baseWidth * 0.69f);
        optCategoryWidth = GUILayout.MaxWidth(fontSize * 12f * 0.47f);

        nodeSelectRect.Set(margin, unitHeight * 2, winRect.width - margin * 2, winRect.height - unitHeight * 4.5f);
        colorRect.Set(margin, unitHeight * 2, winRect.width - margin * 3, winRect.height - unitHeight * 4);
        labelRect.Set(0, 0, winRect.width - margin * 2, itemHeight * 1.2f);
        subRect.Set(0, itemHeight, winRect.width - margin * 2, itemHeight);

        foreach (var func in updaters) {
            func(this);
        }
    }
    public void InitWinRect() {
        winRect.Set(width - FixPx(290), FixPx(48), FixPx(280), height - FixPx(150));
        titleBarRect.Set(0, 0, winRect.width, 24f);

    }
    public void InitFBRect() {
        fileBrowserRect.Set(width - FixPx(620), FixPx(100), FixPx(600), FixPx(600));
    }
    public void InitModalRect() {
        modalRect.Set(width / 2 - FixPx(300), height / 2 - FixPx(300), FixPx(600), FixPx(600));
    }
    public int FixPx(int px) {
        return (int)(ratio * px);
    }
    readonly List<Action<UIParams>> updaters = new List<Action<UIParams>>();
    public void Add(Action<UIParams> action) {
        action(this);
        updaters.Add(action);
    }
    public bool Remove(Action<UIParams> action) {
        return updaters.Remove(action);
    }
}
