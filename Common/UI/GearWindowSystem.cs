using COM3D2API;
using UnityEngine;
using UnityEngine.SceneManagement;

internal class GearWindowSystem : MonoBehaviour {
    bool sceneChangeHide = true;
    PngData scIcon = new PngData("GearIcon.png");
    bool registered = false;
    UIWindow uiWindow;

    void Awake() {
        uiWindow = gameObject.AddComponent<UIWindow>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start() {
        TryRegister();
    }

    void OnDestroy() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // COM3D2.5: przycisk w menu SystemShortcut rejestrujemy przez COM3D2.API (jak ShapekeyMaster / PartsEditRevised).
    void TryRegister() {
        if (registered) return;
        string label = PluginInfo.Name + " " + PluginInfo.Version;
        SystemShortcutAPI.AddButton(PluginInfo.Name, ClickGearButton, label, scIcon.GetData());
        registered = true;
    }

    void OnSceneLoaded(Scene loaded, LoadSceneMode mode) {
        if (sceneChangeHide) SetVisible(false);
    }

    public UIWindow GetUIWindow() {
        return uiWindow;
    }

    // Stan widocznosci trzyma wylacznie UIWindow (okno moze byc schowane tez przez "End" albo krzyzyk).
    public void SetVisible(bool fVisible) {
        if (uiWindow.IsVisible == fVisible) return;
        uiWindow.SetVisible(fVisible);
    }

    void ClickGearButton() {
        SetVisible(!uiWindow.IsVisible);
    }
}
