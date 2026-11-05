using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;


namespace CM3D2.PartsEdit.Plugin {
    class SceneDataManager {
        public static XElement GetSceneXmlData() {
            XElement sceneData = new XElement("SceneData");

            sceneData.Add(GetMaidListData());
            sceneData.Add(GetObjectListData());

            return sceneData;
        }

        static XElement GetMaidListData() {
            XElement maidListData = new XElement("Maids");

            CharacterMgr characterMgr = GameMain.Instance.CharacterMgr;
            int maidCount = characterMgr.GetMaidCount();
            for (int i = 0; i < maidCount; i++) {
                Maid maid = characterMgr.GetMaid(i);
                if (!maid) continue;
                XElement maidXmlData = GetMaidData(maid);
                if (maidXmlData == null) continue;

                maidListData.Add(maidXmlData);
            }
            return maidListData;
        }

        static XElement GetMaidData(Maid maid) {
            BackUpMaidData maidData = BackUpData.GetOrNullMaidData(maid);
            if (maidData == null) return null;

            XElement maidXmlData = new XElement("Maid");

            XElement guidXml = new XElement("GUID");
            guidXml.Add(maid.status.guid);

            XElement nameXml = new XElement("Name");
            nameXml.Add(maid.status.fullNameJpStyle);

            maidXmlData.Add(guidXml);
            maidXmlData.Add(nameXml);

            XElement slotListXml = new XElement("Slots");
            foreach(int slotID in maidData.slotDataDic.Keys) {
                XElement slotXmlData = GetSlotData(maid, slotID);
                if (slotXmlData == null) continue;
                slotListXml.Add(slotXmlData);
            }
            maidXmlData.Add(slotListXml);

            return maidXmlData;
        }

        static XElement GetSlotData(Maid maid, int slotID) {
            TBodySkin tbs;
            GameObject obj;
            if (slotID == (int)EXSlot.Base) {
                tbs = maid.body0.goSlot[0];
                obj = maid.body0.m_Bones.gameObject;
            } else { 
                tbs = maid.body0.GetSlot(slotID);
                obj = tbs.obj;
            }
            if (!obj) return null;
            BackUpSlotData bkSlotData = BackUpData.GetOrNullMaidSlotData(maid, slotID);
            if (bkSlotData == null) return null;
            if (!bkSlotData.objectDataDic.ContainsKey(obj)) return null;

            XElement slotXml = new XElement("Slot");

            XElement categoryXml = new XElement("Category");
            if (slotID == (int)EXSlot.Base) {
                categoryXml.Add("base");
            } else {
                categoryXml.Add(((TBody.SlotID)slotID).ToString());
            }
            slotXml.Add(categoryXml);

            XElement modelFileNameXml = new XElement("ModelFileName");
            modelFileNameXml.Add(tbs.m_strModelFileName);
            slotXml.Add(modelFileNameXml);

            XElement objectXml = GetMaidObjectData(maid, slotID);
            slotXml.Add(objectXml);


            return slotXml;
        }

        static XElement GetObjectListData() {
            XElement objectListData = new XElement("ObjectListData");

            return objectListData;
        }

        static HashSet<Transform> trsHash = null;
        static XElement GetMaidObjectData(Maid maid, int slotID) {
            trsHash = new HashSet<Transform>();
            bool yureOff = false;

            TBodySkin tbs;
            GameObject obj;
            if (slotID == (int)EXSlot.Base) {
                tbs = maid.body0.goSlot[0];
                obj = maid.body0.m_Bones.gameObject;
            } else {
                tbs = maid.body0.GetSlot(slotID);
                obj = tbs.obj;
            }
            if (!obj) return null;
            BackUpSlotData bkSlotData = BackUpData.GetOrNullMaidSlotData(maid, slotID);
            if (bkSlotData == null) return null;
            if (!bkSlotData.objectDataDic.ContainsKey(obj)) return null;

            BackUpObjectData bkObjectData = bkSlotData.objectDataDic[obj];
            XElement objectXml = new XElement("ObjectData");
            if (bkObjectData.changedYure) {
                XElement yureXml = new XElement("Yure");
                bool bYure = YureUtil.GetYureState(maid, slotID);
                yureXml.Add(bYure);
                objectXml.Add(yureXml);

                if (!bYure) {
                    yureOff = true;
                }
            }

            Transform rootBone;
            Transform[] bones;
            if (slotID == (int)EXSlot.Base) {
                SkinnedMeshRenderer smr = maid.body0.goSlot[0].obj.GetComponentInChildren<SkinnedMeshRenderer>();
                rootBone = maid.body0.m_Bones.transform;
                bones =
                    smr.bones
                    .Where(bone => bone != null)
                    .Select(bone => CMT.SearchObjName(rootBone, bone.name, true))
                    .Where(bone => bone != null)
                    .ToArray();
            } else {
                rootBone = maid.body0.goSlot[slotID].obj_tr;
                SkinnedMeshRenderer smr = rootBone.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
                bones = smr.bones.Where(bone => bone != null).ToArray();
            }
            foreach (Transform bone in bones) {
                Transform trs = bone;
                while (trs != rootBone) {
                    if (!trs) {
                        Debug.Log("PartsEdit: bone is not under the root object: " + bone.name); // ja: ルートオブジェクト配下にありません
                        return null;
                    }
                    if (trsHash.Contains(trs)) break;
                    if (yureOff || BackUpData.GetOrNullMaidBoneData(maid, slotID, obj, trs) != null) {
                        XElement transformXmlData = new XElement("TransformData");
                        XElement nameXmlData = new XElement("Name");
                        nameXmlData.Add(trs.name);
                        transformXmlData.Add(nameXmlData);

                        transformXmlData.Add(GetVector3Data("Scale", trs.localScale));
                        transformXmlData.Add(GetVector3Data("Position", trs.localPosition));
                        transformXmlData.Add(GetVector3Data("Rotation", trs.localEulerAngles));

                        objectXml.Add(transformXmlData);
                    }
                    trsHash.Add(trs);
                    trs = trs.parent;
                }
            }


            return objectXml;
        }

        static XElement GetVector3Data(string str, Vector3 vec) {
            XElement xml = new XElement(str);

            XElement x_xml = new XElement("x");
            x_xml.Add(vec.x);
            xml.Add(x_xml);

            XElement y_xml = new XElement("y");
            y_xml.Add(vec.y);
            xml.Add(y_xml);

            XElement z_xml = new XElement("z");
            z_xml.Add(vec.z);
            xml.Add(z_xml);

            return xml;
        }

        public static void SetSceneXmlData(XElement xmlData) {
            BackUpData.Refresh();

            XElement maidListXml = xmlData.Element("Maids");
            if (maidListXml == null) return;

            SetMaidListData(maidListXml);
        }

        static void SetMaidListData(XElement maidListXml) {
            IEnumerable<XElement> maidXmlList = maidListXml.Elements("Maid");
            if (maidXmlList == null) return;

            foreach(XElement maidXml in maidXmlList) {
                SetMaidData(maidXml);
            }
        }

        static void SetMaidData(XElement maidXml) {
            XElement guidXml = maidXml.Element("GUID");
            if (guidXml == null) return;

            Maid maid = GameMain.Instance.CharacterMgr.GetMaid((string)guidXml);
            if (!maid) return;

            XElement slotListData = maidXml.Element("Slots");
            if (slotListData == null) return;

            IEnumerable<XElement> slotXmlList = slotListData.Elements("Slot");
            if (slotXmlList == null) return;
            foreach(XElement slotXml in slotXmlList) {
                SetSlotData(maid, slotXml);
            }
        }

        static void SetSlotData(Maid maid, XElement slotXml) {
            XElement categoryXml = slotXml.Element("Category");
            if (categoryXml == null) return;

            int slotID = GetCategoryID((string)categoryXml);
            if (slotID == (int)EXSlot.None) return;

            XElement modelNameXml = slotXml.Element("ModelFileName");
            if (modelNameXml == null) return;

            string modelFileName = (string)modelNameXml;


            TBodySkin tbs;
            if (slotID == (int)EXSlot.Base) {
                tbs = maid.body0.goSlot[0];
            } else {
                tbs = maid.body0.GetSlot(slotID);
            }

            if (tbs.m_strModelFileName != modelFileName) {
                Debug.Log("PartsEdit: " + (string)categoryXml + " model file changed since save, skipped."); // ja: カテゴリのmodelファイルがセーブ時と変わっているため、ロードされません
                Debug.Log(modelFileName + "→" + tbs.m_strModelFileName);
                return;
            }

            XElement objectXml = slotXml.Element("ObjectData");
            if (objectXml == null) return;

            SetMaidObjectData(maid, slotID, objectXml);
        }

        static void SetMaidObjectData(Maid maid, int slotID, XElement objectXml) {
            TBodySkin tbs;
            GameObject obj;
            if (slotID == (int)EXSlot.Base) {
                tbs = maid.body0.goSlot[0];
                obj = maid.body0.m_Bones.gameObject;
            } else {
                tbs = maid.body0.GetSlot(slotID);
                obj = tbs.obj;
            }

            XElement yureXml = objectXml.Element("Yure");
            if(yureXml != null) {
                bool bYure = (bool)yureXml;
                if(YureUtil.GetYureAble(maid, slotID)) {
                    if(YureUtil.GetYureState(maid, slotID) != bYure) {
                        BackUpObjectData bkObjData = BackUpData.GetOrAddMaidObjectData(maid, slotID, obj);
                        if (bkObjData.changedYure) {
                            bkObjData.changedYure = false;
                            bkObjData.bYure = true;
                        }else {
                            bkObjData.changedYure = true;
                            bkObjData.bYure = YureUtil.GetYureState(maid, slotID);
                        }
                        YureUtil.SetYureState(maid, slotID, bYure);
                    }
                }
            }

            IEnumerable<XElement> transformXmlList = objectXml.Elements("TransformData");
            if (transformXmlList != null) ShoeProxy.EnsureFor(obj, transformXmlList.Select(x => (string)x.Element("Name")));
            if (transformXmlList != null) {
                foreach(XElement transformXml in transformXmlList) {
                    XElement nameXml = transformXml.Element("Name");
                    if (nameXml == null) return;

                    Transform bone;
                    if (slotID == (int)EXSlot.Base) {
                        bone = CMT.SearchObjName(obj.transform, (string)nameXml, true);
                    } else {
                        bone = CMT.SearchObjName(obj.transform, (string)nameXml, false);
                    }
                    if (bone == null) continue;

                    BackUpBoneData bkBoneData = BackUpData.GetOrAddMaidBoneData(maid, slotID, obj, bone);

                    XElement scaleXml = transformXml.Element("Scale");
                    if (scaleXml != null) {
                        if (!bkBoneData.changedScl) {
                            bkBoneData.changedScl = true;
                            bkBoneData.scale = bone.localScale;
                        }
                        Vector3 scale = GetVectorData(scaleXml);
                        bone.localScale = scale;
                    }

                    XElement positionXml = transformXml.Element("Position");
                    if (positionXml != null) {
                        if (!bkBoneData.changedPos) {
                            bkBoneData.changedPos = true;
                            bkBoneData.position = bone.localPosition;
                        }
                        Vector3 position = GetVectorData(positionXml);
                        bone.localPosition = position;
                    }

                    XElement rotationXml = transformXml.Element("Rotation");
                    if (rotationXml != null) {
                        if (!bkBoneData.changedRot) {
                            bkBoneData.changedRot = true;
                            bkBoneData.rotation = bone.localRotation;
                        }
                        Vector3 rotation = GetVectorData(rotationXml);
                        bone.localEulerAngles = rotation;
                    }
                }
            }
        }

        static Vector3 GetVectorData(XElement vecXml) {
            XElement x_xml = vecXml.Element("x");
            XElement y_xml = vecXml.Element("y");
            XElement z_xml = vecXml.Element("z");

            return new Vector3((float)x_xml, (float)y_xml, (float)z_xml);
        }

        static int GetCategoryID(string categoryName) {
            if (categoryName == "base") return (int)EXSlot.Base;
            try {
                return (int)Enum.Parse(typeof(TBody.SlotID), categoryName, true);
            }catch(OverflowException e) {
                return (int)EXSlot.None;
            }
        }
    }
}
