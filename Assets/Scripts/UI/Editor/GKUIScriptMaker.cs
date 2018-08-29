using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using GKFile;

namespace GKUI
{
    public class GKUIScriptMaker
    {

        #region PublicField
        // If you add a function, you need to modify: 1) Here define.2) Script auto making. 3) UI Editor GUI.
        // Get support function list definition by type.
        public static string[] genericType2FunListDef = { "SetColor", "SetScale", "SetActive" };
        // Texture.
        public static string[] texture2FunListDef = { };
        public static string[] rawImage2FunListDef = { "SetTexture" };
        public static string[] image2FunListDef = { "SetSprite" };
        // Button.
        public static string[] button2FunListDef = { };
        // Text.
        public static string[] text2FunListDef = { "SetContent" };
        #endregion

        #region PrivateField
        private static string outputDemoPath = string.Format("{0}/Scripts/_Demo/UI/Widget/", Application.dataPath);
        private static string outputPath = string.Format("{0}/Scripts/UI/Widget/", Application.dataPath);
        #endregion

        #region PublicMethod
        public static bool GenerateScripts(List<UIEditorLayer> list, bool demo)
        {
            string className = GKUIMaker.targetName;
            string p = demo ? outputDemoPath : outputPath;
            p = string.Format("{0}{1}.cs", p, className);

            if (File.Exists(p))
            {
                if (EditorUtility.DisplayDialog("Generate UI script", "The target script file already exists. Is it replaced?", "Of course", "No, Thanks"))
                {
                    Debug.Log("Replaced script.");
                    GKFileUtil.DeleteFile(p);
                }
                else
                {
                    Debug.Log("No replace. Skip to Generate scripts.");
                    return false;
                }
            }

            StreamWriter write = new StreamWriter(p, false, Encoding.UTF8);
            WrittingUsingContent(write);
            write.WriteLine();
            WrittingClassBegin(write, className);
            WrittingSerializable(write, list);
            write.WriteLine();
            WrittingPublicField(write, list);
            write.WriteLine();
            WrittingPrivateField(write, list);
            write.WriteLine();
            PublicMethod(write, list);
            write.WriteLine();
            PrivateMethod(write, list);
            WritingClassEnd(write);

            write.Close();

            AssetDatabase.Refresh();

            return true;
        }
        #endregion

        #region PrivateMethod

        private static void WrittingUsingContent(StreamWriter w)
        {
            w.WriteLine("using System.Collections;");
            w.WriteLine("using System.Collections.Generic;");
            w.WriteLine("using UnityEngine;");
            w.WriteLine("using UnityEngine.UI;");
        }

        private static void WrittingClassBegin(StreamWriter w, string c)
        {
            w.WriteLine(string.Format("public class {0} : SingletonUIBase<{0}>", c));
            w.WriteLine("{");
        }

        private static void WritingClassEnd(StreamWriter w)
        {
            w.WriteLine("}");
        }

        private static void WrittingSerializable(StreamWriter w, List<UIEditorLayer> list)
        {
            w.WriteLine("\t#region Serializable");
            w.WriteLine("\t[System.Serializable]");
            w.WriteLine("\tpublic class Controls");
            w.WriteLine("\t{");

            foreach (var l in list)
            {
                foreach (var widget in l.widgets)
                {
                    if (widget.show && widget.serialize)
                    {
                        w.WriteLine(string.Format("\t\tpublic\t{0}\t{1};", widget.GetSerializeName(), widget.name));
                    }
                }
            }
            w.WriteLine("\t}");
            w.WriteLine("\t#endregion");
        }

        private static void WrittingPublicField(StreamWriter w, List<UIEditorLayer> list)
        {
            w.WriteLine("\t#region PublicField");
            foreach (var l in list)
            {
                foreach (var widget in l.widgets)
                {
                    if (widget.show && widget.serialize)
                    {
                        int eventResult = GKUIMaker.CheckEventsBehavior();
                        if (GKUIMaker.UIWidgetType.Button == widget.type
                            || (GKUIMaker.UIWidgetType.Texture == widget.type && widget.texData.raycast && eventResult > 1)
                            || (GKUIMaker.UIWidgetType.Text == widget.type && widget.textData.raycast && eventResult > 1))
                        {
                            w.WriteLine(string.Format("\tpublic GK.DelegateType On{0}Delegate;", widget.name));
                        }
                    }
                }
            }
            w.WriteLine("\t#endregion");
        }

        private static void WrittingPrivateField(StreamWriter w, List<UIEditorLayer> list)
        {
            w.WriteLine("\t#region PrivateField");
            w.WriteLine("\t[System.NonSerialized]");
            w.WriteLine("\tprivate Controls m_ctl;");
            w.WriteLine("\t#endregion");
        }

        private static void PublicMethod(StreamWriter w, List<UIEditorLayer> list)
        {
            w.WriteLine("\t#region PublicMethod");

            // General interface.
            foreach (var l in list)
            {
                foreach (var widget in l.widgets)
                {
                    if (widget.show && widget.serialize &&
                        (GKUIMaker.UIWidgetType.Texture == widget.type
                        || GKUIMaker.UIWidgetType.Button == widget.type
                        || GKUIMaker.UIWidgetType.Text == widget.type))
                    {
                        // Set color.
                        w.WriteLine(string.Format("\tpublic void SetColor{0}(Color c)", widget.name));
                        w.WriteLine("\t{");
                        if (GKUIMaker.UIWidgetType.Button == widget.type)
                        {
                            w.WriteLine(string.Format("\t\tColorBlock cb = m_ctl.{0}.colors;", widget.name));
                            w.WriteLine(string.Format("\t\tcb.normalColor = c;"));
                            w.WriteLine(string.Format("\t\tm_ctl.{0}.colors = cb;", widget.name));
                        }
                        else
                        {
                            w.WriteLine(string.Format("\t\tm_ctl.{0}.color = c;", widget.name));
                        }
                        w.WriteLine("\t}");
                        w.WriteLine();
                        // Set scale.
                        w.WriteLine(string.Format("\tpublic void SetScale{0}(Vector3 v)", widget.name));
                        w.WriteLine("\t{");
                        w.WriteLine(string.Format("\t\tm_ctl.{0}.GetComponent<RectTransform>().localScale = v;", widget.name));
                        w.WriteLine("\t}");
                        w.WriteLine();
                        // Set active.
                        w.WriteLine(string.Format("\tpublic void SetActive{0}(bool b)", widget.name));
                        w.WriteLine("\t{");
                        w.WriteLine(string.Format("\t\tm_ctl.{0}.gameObject.SetActive (b);", widget.name));
                        w.WriteLine("\t}");
                        w.WriteLine();

                        // Texture.
                        if (GKUIMaker.UIWidgetType.Texture == widget.type)
                        {
                            // Set image.
                            if (0 == widget.texData.type)
                            {
                                w.WriteLine(string.Format("\tpublic void SetTexture{0}(Texture2D tex)", widget.name));
                                w.WriteLine("\t{");
                                w.WriteLine(string.Format("\t\tm_ctl.{0}.texture = tex;", widget.name));
                                w.WriteLine("\t}");
                                w.WriteLine();
                            }
                            else
                            {
                                w.WriteLine(string.Format("\tpublic void SetSprite{0}(Sprite sp)", widget.name));
                                w.WriteLine("\t{");
                                w.WriteLine(string.Format("\t\tm_ctl.{0}.sprite = sp;", widget.name));
                                w.WriteLine("\t}");
                                w.WriteLine();
                            }
                        }

                        // Text.
                        if (GKUIMaker.UIWidgetType.Text == widget.type)
                        {
                            // Set content.
                            w.WriteLine(string.Format("\tpublic void SetContent{0}(string msg)", widget.name));
                            w.WriteLine("\t{");
                            w.WriteLine(string.Format("\t\tm_ctl.{0}.text = msg;", widget.name));
                            w.WriteLine("\t}");
                            w.WriteLine();
                        }

                    }
                }
            }

            w.WriteLine("\t#endregion");
        }

        private static void PrivateMethod(StreamWriter w, List<UIEditorLayer> list)
        {
            w.WriteLine("\t#region PrivateMethod");
            w.WriteLine("\tprivate void Awake()");
            w.WriteLine("\t{");
            w.WriteLine("\t\tSerializable();");
            w.WriteLine("\t\tInitListener();");
            w.WriteLine("\t\tInit();");
            w.WriteLine("\t}");

            w.WriteLine();

            w.WriteLine("\tprivate void Serializable()");
            w.WriteLine("\t{");
            w.WriteLine("\t\tGKUI.FindControls(this.gameObject, ref m_ctl);");
            w.WriteLine("\t}");

            w.WriteLine();

            w.WriteLine("\tprivate void InitListener()");
            w.WriteLine("\t{");
            foreach (var l in list)
            {
                foreach (var widget in l.widgets)
                {
                    if (widget.show && widget.serialize)
                    {
                        int eventResult = GKUIMaker.CheckEventsBehavior();
                        if (GKUIMaker.UIWidgetType.Button == widget.type
                            || (GKUIMaker.UIWidgetType.Texture == widget.type && widget.texData.raycast && (eventResult > 0 || widget.components.Count > 0))
                            || (GKUIMaker.UIWidgetType.Text == widget.type && widget.textData.raycast && (eventResult > 0 || widget.components.Count > 0)))
                        {
                            w.WriteLine(string.Format("\t\tGKUIEventTriggerListener.Get(m_ctl.{0}.gameObject).onClick = On{0};", widget.name));
                        }
                    }
                }
            }
            w.WriteLine("\t}");

            w.WriteLine();

            w.WriteLine("\tprivate void Init()");
            w.WriteLine("\t{");
            w.WriteLine("\t}");

            w.WriteLine();

            foreach (var l in list)
            {
                foreach (var widget in l.widgets)
                {
                    if (widget.show && widget.serialize)
                    {
                        int eventResult = GKUIMaker.CheckEventsBehavior();
                        if (GKUIMaker.UIWidgetType.Button == widget.type
                            || (GKUIMaker.UIWidgetType.Texture == widget.type && widget.texData.raycast && (eventResult > 0 || widget.components.Count > 0))
                            || (GKUIMaker.UIWidgetType.Text == widget.type && widget.textData.raycast && (eventResult > 0 || widget.components.Count > 0)))
                        {

                            w.WriteLine(string.Format("\tprivate void On{0}(GameObject go)", widget.name));
                            w.WriteLine("\t{");

                            // Events.
                            if (1 == eventResult || 3 == eventResult)
                            {

                                // Draw Do something.
                                foreach (var e in widget.events)
                                {

                                    switch (e.methodName)
                                    {
                                        case "SetColor":

                                            w.WriteLine(string.Format("\t\tSetColor{0}(new Color({1}, {2}, {3}, {4}));", e.widgetName,
                                                ((int)((Color)e.paramList[0]).r * 255), (int)(((Color)e.paramList[0]).g * 255), (int)(((Color)e.paramList[0]).b * 255), (int)(((Color)e.paramList[0]).a * 255)));

                                            break;

                                        case "SetScale":

                                            w.WriteLine(string.Format("\t\tSetScale{0}(new Vector3({1}, {2}, {3}));", e.widgetName,
                                                ((Vector3)e.paramList[0]).x, ((Vector3)e.paramList[0]).y, ((Vector3)e.paramList[0]).z));

                                            break;

                                        case "SetActive":

                                            w.WriteLine(string.Format("\t\tSetActive{0}({1})", e.widgetName, (bool)e.paramList[0]));

                                            break;

                                        case "SetTexture":


                                            w.WriteLine(string.Format("\t\tTexture2D tex = GK.GetTextureByName(\"{0}\");", ((Texture2D)e.paramList[0]).name));
                                            w.WriteLine(string.Format("\t\tSetTexture{0}(tex);", e.widgetName));

                                            break;

                                        case "SetSprite":

                                            w.WriteLine(string.Format("\t\tSprite sp = GK.GetSpriteByName(\"{0}\");", ((Sprite)e.paramList[0]).name));
                                            w.WriteLine(string.Format("\t\tSetSprite{0}(sp);", e.widgetName));

                                            break;

                                        case "SetContent":

                                            w.WriteLine(string.Format("\t\tSetContent{0}({1})", e.widgetName, (string)e.paramList[0]));

                                            break;
                                    }

                                    w.WriteLine();
                                }

                            }

                            if (2 == eventResult || 3 == eventResult)
                            {

                                // Draw Delegate.
                                w.WriteLine(string.Format("\t\tif (null != On{0}Delegate)", widget.name));
                                w.WriteLine("\t\t{");
                                w.WriteLine(string.Format("\t\t\tOn{0}Delegate ();", widget.name));
                                w.WriteLine("\t\t}");
                                w.WriteLine();

                            }

                            // Component.
                            // Audio.
                            if (null != widget.btnData.clickClip || GetComponentType(GKUIMaker.UIWidgetComponentType.Audio, widget))
                            {

                                w.WriteLine("\t\t// Audio.");
                                w.WriteLine("\t\tAudioSource audioSrc = m_ctl.{0}.GetComponent<AudioSource> ();", widget.name);
                                w.WriteLine("\t\tif (null != audioSrc && null != audioSrc.clip) {");
                                w.WriteLine("\t\t\taudioSrc.Play ();");
                                w.WriteLine("\t\t}");

                            }

                            w.WriteLine("\t}");
                            w.WriteLine();

                        }
                    }
                }
            }

            w.WriteLine("\t#endregion");

        }

        private static bool GetComponentType(GKUIMaker.UIWidgetComponentType type, UIEditorWidget w)
        {
            if (null == w || GKUIMaker.UIWidgetComponentType.None == type)
            {
                return false;
            }

            foreach (var c in w.components)
            {
                if (c.type == type && c.paramList.Count > 0)
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}