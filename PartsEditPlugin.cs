using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using UnityEngine;
using UnityInjector;
using UnityInjector.Attributes;
using ExIni;

namespace CM3D2.PartsEdit.Plugin {
    [PluginName("CM3D2 PartsEditPlugin"), PluginVersion("0.2")]
    class PartsEditPlugin : PluginBase {
        internal static bool started;

        void Awake() {
            if (started) return;
            started = true;
            SetPluginInfo();
            CreatePluginObject();
        }

        // 外部から読める場所にプラグイン情報を設定
        void SetPluginInfo() {
            var pluginNameAttr = Attribute.GetCustomAttribute(this.GetType(), typeof(PluginNameAttribute)) as PluginNameAttribute;
            var pluginVersionAttr = Attribute.GetCustomAttribute(this.GetType(), typeof(PluginVersionAttribute)) as PluginVersionAttribute;
            string pluginName = (pluginNameAttr == null) ? this.Name : pluginNameAttr.Name;
            string pluginVersion = (pluginVersionAttr == null) ? string.Empty : pluginVersionAttr.Version;
            string nameSpace = this.GetType().Namespace;
            PluginInfo.SetInfo(pluginName, pluginVersion, nameSpace);
            IniUtil.Init(this);
            IniUtil.saveMethod += SaveConfig;
            Setting.LoadIni();
            Setting.SaveIni();
        }

        // プラグイン本体を動かすオブジェクトを生成
        void CreatePluginObject() {
            GameObject obj = new GameObject();
            obj.name = "PartsEdit.Plugin";
            //obj.AddComponent<TestUIParts>();
            obj.AddComponent<PartsEdit>();
            DontDestroyOnLoad(obj);
        }


#if COM3D2
        //StudioEx AUX Plugin Manager;
        protected PluginBase hPluginStudioExCoPluginManager;

        /////////////////////////////////////////////////////////////////////////////////////////
        // StduioEx Event;
        #region StduioExEvents;

        public void OnStudioExNotifyInitStart(PluginBase sender) {
            //StudioExプラグイン連携のためこのプラグイン情報を登録する;
            hPluginStudioExCoPluginManager = sender;
            //このプラグインを登録;
            hPluginStudioExCoPluginManager.SendMessage("OnStudioExCallRegistPlugin", PluginInfo.Name);
            return;
        }

        public void OnStudioExCallSave(Dictionary<string, XElement[]> hConfig) {
            try {
                //プラグイン情報を保存;
                XElement hPluginRootElement = new XElement("PartsEdit");
                hPluginRootElement.Add(new XElement("PluginVersion", "0.2"));

                hPluginRootElement.Add(SceneDataManager.GetSceneXmlData());

                //StudioExへ登録;
                XElement[] phXElements = new XElement[1];
                phXElements[0] = hPluginRootElement;
                KeyValuePair<string, XElement[]> hArg = new KeyValuePair<string, XElement[]>(PluginInfo.Name, phXElements);
                hPluginStudioExCoPluginManager.SendMessage("OnStudioExCallWriteXML", hArg);
            }catch(Exception e) {
                Debug.Log(e);
            }
        }

        public void OnStudioExCallLeave(Dictionary<string, XElement[]> hConfig) {
            try {
                //プラグイン情報を保存;
                XElement hPluginRootElement = new XElement("PartsEdit");
                hPluginRootElement.Add(new XElement("PluginVersion", "0.2"));

                hPluginRootElement.Add(SceneDataManager.GetSceneXmlData());

                //StudioExへ登録;
                XElement[] phXElements = new XElement[1];
                phXElements[0] = hPluginRootElement;
                KeyValuePair<string, XElement[]> hArg = new KeyValuePair<string, XElement[]>(PluginInfo.Name, phXElements);
                hPluginStudioExCoPluginManager.SendMessage("OnStudioExCallWriteXML", hArg);
            } catch (Exception e) {
                Debug.Log(e);
            }
        }

        public void OnStudioExCallLoadFinishing(Dictionary<string, XElement[]> hConfig) {
            try {
                string pszLoadedBackgroundPrefabName = string.Empty;
                XElement[] phElements = hConfig.ContainsKey(PluginInfo.Name) ? hConfig[PluginInfo.Name] : null;
                if (phElements != null) {
                    XElement hPluginRootElement = phElements[0];
                    //結果データの取得;
                    XElement hElementGroupRoot = hPluginRootElement.Element("SceneData");
                    SceneDataManager.SetSceneXmlData(hElementGroupRoot);
                }
            }catch(Exception e) {
                Debug.Log(e);
            }
        }

        public void OnStudioExCallEnterFinishing(Dictionary<string, System.Xml.Linq.XElement[]> hConfig) {
            try {
                string pszLoadedBackgroundPrefabName = string.Empty;
                XElement[] phElements = hConfig.ContainsKey(PluginInfo.Name) ? hConfig[PluginInfo.Name] : null;
                if (phElements != null) {
                    XElement hPluginRootElement = phElements[0];
                    //結果データの取得;
                    XElement hElementGroupRoot = hPluginRootElement.Element("SceneData");
                    SceneDataManager.SetSceneXmlData(hElementGroupRoot);
                }
            } catch (Exception e) {
                Debug.Log(e);
            }
        }

        #endregion
#endif
    }
}
