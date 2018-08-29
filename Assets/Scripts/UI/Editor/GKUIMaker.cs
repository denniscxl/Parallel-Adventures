using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System;
using UnityEngine.UI;
using GKBase;
using GKFile;

namespace GKUI
{
    public class GKUIMaker : EditorWindow
    {

        #region PublicField
        public enum UIWidgetType
        {
            Texture,
            Button,
            Text,
            Custom,
            EmptyObject
        }

        public enum UIWidgetEventType
        {
            None,
            WidgetMethod,
            Delegate
        }

        public enum UIWidgetComponentType
        {
            None,
            Audio,
            Drag
        }

        public static readonly string tagUIDrag = "UIDrag";
        public static readonly float viewWidth = 180;   // View screen width;
        public static readonly float viewHeight = 320;  // View screen height;
        public static readonly float windowWidth = 1200;// Windows screen width;
        public static readonly float windowHeight = 580;// Windows screen height;
        public static UIEditorLayer currentLayer = null;
        public static UIEditorWidget currentWidget = null;
        public static string targetName = "";
        #endregion

        #region privateField
        private static List<UIEditorLayer> list = new List<UIEditorLayer>();

        private Vector2 layerScrollPos = new Vector2(0, 0);
        private Vector2 widgetScrollPos = new Vector2(0, 0);
        private Vector2 widgetDetailScrollPos = new Vector2(0, 0);

        private static MessageType logLv = MessageType.Info;
        private static string logContent = "";

        private static bool bDemo = true;

        private string[] optTexture = new string[] { "Texture", "Sprite" };
        private string[] optButton = new string[] { "Texture", "Sprite" };
        private string[] optTextLR = new string[] { "Left", "Middle", "Right" };
        private string[] optTextTP = new string[] { "Top", "Middle", "Bottom" };

        private GKUISkin globalSkin = null;
        #endregion

        #region PublicMethod
        [MenuItem("GK/UI/UI Editor ", false, GKEditorConfiger.MenuItemPriorityB)]
        public static void MenuItem_Window()
        {
            var w = EditorWindow.GetWindow<GKUIMaker>("UI Maker");
            w.autoRepaintOnSceneChange = true;
            // Window size is not allowed.
            w.minSize = new Vector2(windowWidth, windowHeight);
            w.Show();
        }

        public static int GenLayerIdx()
        {
            return GetLayerCount();
        }

        public static int GetLayerCount()
        {

            int tCount = 0;
            foreach (var l in list)
            {
                if (l.enable)
                {
                    tCount++;
                }
            }

            return tCount;

        }

        public static int GenWidgetIdx()
        {
            if (null == currentLayer)
                return 0;
            return currentLayer.widgets.Count;
        }

        public static void ModifyLayer(UIEditorLayer layer, int value)
        {
            if (1 == value || -1 == value)
            {
                int targetLayer = layer.layerIdx + value;
                foreach (var e in list)
                {
                    if (!e.enable)
                        continue;

                    if (targetLayer == e.layerIdx)
                    {
                        e.layerIdx = layer.layerIdx;
                        layer.layerIdx = targetLayer;

                        // Rename widget.
                        foreach (var i in e.widgets)
                        {
                            i.GenName();
                        }
                        foreach (var j in layer.widgets)
                        {
                            j.GenName();
                        }

                        SetLog(MessageType.Info, string.Format("Modify layer success. Index changed from {0} to {1}", e.layerIdx, layer.layerIdx));
                        return;
                    }
                }
            }

            SetLog(MessageType.Error, "Modify layer failure. The index value of target exchange cannot be found.");
        }

        public static void ModifySort(UIEditorWidget widget, int value)
        {
            if (1 == value || -1 == value)
            {
                int targetSort = widget.sort + value;
                foreach (var e in currentLayer.widgets)
                {
                    if (targetSort == e.sort)
                    {
                        e.sort = widget.sort;
                        widget.sort = targetSort;

                        // Rename widget.
                        e.GenName();
                        widget.GenName();

                        SetLog(MessageType.Info, string.Format("Modify widget sort success. Index changed from {0} to {1}", e.sort, widget.sort));
                        return;
                    }
                }
            }

            SetLog(MessageType.Error, "Modify widget sort failure. The index value of target exchange cannot be found.");
        }

        public static string GetSerializeName(UIWidgetType type, UIEditorWidget.TextureWidget texData)
        {
            string strName = "";

            if (UIWidgetType.Texture == type)
            {
                if (null != texData && 0 == texData.type)
                {
                    return "RawImage";
                }
                else if (null != texData && 1 == texData.type)
                {
                    return "Image";
                }
                else
                {
                    return "";
                }
            }
            else if (UIWidgetType.Button == type)
            {
                return "Button";
            }
            else if (UIWidgetType.Text == type)
            {
                return "Text";
            }
            else if (UIWidgetType.Custom == type)
            {
                return "GameObject";
            }
            else if (UIWidgetType.EmptyObject == type)
            {
                return "GameObject";
            }

            return strName;
        }

        // Get widget instance based on the name.
        public static UIEditorWidget GetWidgetByName(string name)
        {
            foreach (var l in list)
            {
                foreach (var widget in l.widgets)
                {
                    if (string.Equals(widget.name, name))
                    {
                        return widget;
                    }
                }
            }

            return null;
        }

        /**
             * Detecting event behavior.
             * return 0. None. 1. WidgetMethod. 2. Delegate. 3. (1+2).
             * */
        public static int CheckEventsBehavior()
        {
            int result = 0;

            if (null == currentWidget || 0 == currentWidget.events.Count)
            {
                return 0;
            }

            foreach (var e in currentWidget.events)
            {
                if (UIWidgetEventType.WidgetMethod == e.type)
                {
                    result = 1;
                    break;
                }
            }

            foreach (var e in currentWidget.events)
            {
                if (UIWidgetEventType.Delegate == e.type)
                {
                    result = (1 == result) ? 3 : 2;
                    break;
                }
            }

            return result;
        }

        public static UIEditorLayer GetLayerByName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            foreach (var l in list)
            {
                if (l.name.Equals(name))
                {
                    return l;
                }
            }
            return null;
        }

        public static UIEditorWidget GetWidgetByName(UIEditorLayer layer, string name)
        {

            if (string.IsNullOrEmpty(name))
            {
                return null;
            }

            foreach (var w in layer.widgets)
            {
                if (w.name.Equals(name))
                {
                    return w;
                }
            }

            return null;
        }

        // Change hierarchy dependencies.
        public static void ChangeLayerParent(UIEditorLayer srcLayer, UIEditorLayer destLayer, UIEditorWidget w)
        {
            srcLayer.layerIdx = -1;
            destLayer.list.Add(srcLayer);
            srcLayer.parentWidget = w;

            srcLayer.enable = false;

            RecalcLayerIdx();
            srcLayer.editorChildmode = false;
        }

        // Gets all hierarchy objects. Contains child nodes.
        public static List<UIEditorLayer> GetAllLayerList()
        {

            List<UIEditorLayer> l = new List<UIEditorLayer>();

            foreach (var layer in list)
            {

                l.Add(layer);

            }

            return l;

        }

        public static List<UIEditorLayer> GetLayerList(UIEditorLayer layer)
        {

            List<UIEditorLayer> l = new List<UIEditorLayer>();
            l.Add(layer);

            foreach (var subLayer in layer.list)
            {

                var subLayers = GetLayerList(subLayer);
                foreach (var sl in subLayers)
                {

                    l.Add(sl);

                }

            }

            return l;

        }

        public static void SetLog(MessageType lv, string msg)
        {
            logLv = lv;
            logContent = msg;

            switch (lv)
            {
                case MessageType.Info:
                    Debug.Log(msg);
                    break;
                case MessageType.Warning:
                    Debug.LogWarning(msg);
                    break;
                case MessageType.Error:
                    Debug.LogError(msg);
                    break;
            }

        }

        public static void RollBackLayer(UIEditorLayer srcLayer)
        {
            if (null == srcLayer.parentLayer)
            {
                return;
            }

            var parentLayer = srcLayer.parentLayer;
            parentLayer.list.Remove(srcLayer);
            srcLayer.parentLayer = null;
            srcLayer.parentWidget = null;
            srcLayer.layerIdx = GenLayerIdx();
            srcLayer.enable = true;

        }
        #endregion

        #region PrivateMethod
        void OnGUI()
        {

            GUILayoutOption[] options;

            EditorGUILayout.BeginVertical("Box");
            {
                // Draw title.
                GKEditor.DrawInspectorSeperator();
                GUI.skin.label.fontSize = 24;
                GUI.skin.label.alignment = TextAnchor.MiddleCenter;
                GUILayout.Label("UI Editor", GUILayout.MaxWidth(windowWidth));
                GKEditor.DrawInspectorSeperator();

                EditorGUILayout.BeginHorizontal();
                {

                    // Draw layer texture view.
                    EditorGUI.BeginDisabledGroup(null != currentLayer && null == currentLayer.tex);
                    {
                        options = new[] { GUILayout.Width(viewWidth), GUILayout.Height(viewHeight) };
                        //					Texture t = (null != currentLayer) ? currentLayer.tex : null;
                        //					GUILayout.Box (t, options);
                        GUILayout.Box("", options);

                        // Render based on data. 
                        if (null != currentLayer)
                        {

                            foreach (var w in currentLayer.widgets)
                            {

                                DrawWidget(w, w.GetWidgetColor());

                            }

                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    // Draw layer list.

                    layerScrollPos = EditorGUILayout.BeginScrollView(layerScrollPos);
                    {
                        options = new[] { GUILayout.Width(250), GUILayout.Height(viewHeight) };
                        EditorGUILayout.BeginVertical("Box", options);
                        {
                            if (0 < list.Count)
                            {

                                foreach (var e in list)
                                {
                                    e.Draw();
                                }
                                ReorderingLayer();

                            }

                            if (GUILayout.Button("New layer"))
                            {

                                UIEditorLayer uiLayer = new UIEditorLayer();
                                currentLayer = uiLayer;
                                UIEditorWidget uiWidget = new UIEditorWidget(new Vector4(UIController.width * 0.5f, UIController.height * 0.5f, 0, 0), GenWidgetIdx(), uiLayer);
                                uiLayer.widgets.Add(uiWidget);
                                list.Add(uiLayer);

                            }
                        }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUILayout.EndScrollView();

                    // Draw layer detail.
                    if (null != currentLayer)
                    {
                        EditorGUILayout.BeginVertical("Box");
                        {
                            GUI.skin.label.fontSize = 12;
                            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
                            options = new[] { GUILayout.Width(160), GUILayout.Height(16) };
                            GUILayout.Label(string.Format("Layer: {0}", (null != currentLayer) ? currentLayer.name : ""), options);

                            GUILayout.Label(string.Format("Resolution: {0}X{1}",
                                (null != currentLayer) ? currentLayer.width : 0, (null != currentLayer) ? currentLayer.height : 0), options);

                            if (null != currentLayer && !currentLayer.resize)
                            {

                                if (GUILayout.Button("Resize"))
                                {
                                    currentLayer.resize = true;
                                }

                            }
                            else if (null != currentLayer && currentLayer.resize)
                            {

                                currentLayer.width = EditorGUILayout.IntField("Width", currentLayer.width);
                                currentLayer.height = EditorGUILayout.IntField("Height", currentLayer.height);

                                if (GUILayout.Button("Close"))
                                {
                                    currentLayer.resize = false;
                                }

                            }

                            GUILayout.Label(string.Format("Widget count: {0}", (null != currentLayer) ? currentLayer.widgets.Count : 0), options);

                        }
                        EditorGUILayout.EndVertical();
                    }

                    // Draw widget list.
                    if (null != currentLayer && 0 < currentLayer.widgets.Count)
                    {
                        widgetScrollPos = EditorGUILayout.BeginScrollView(widgetScrollPos);
                        {
                            options = new[] { GUILayout.Width(250), GUILayout.Height(viewHeight) };
                            EditorGUILayout.BeginVertical("Box", options);
                            {
                                foreach (var w in currentLayer.widgets)
                                {
                                    w.Draw();
                                }
                                ReorderingWidgetSort();

                                if (GUILayout.Button("New widget"))
                                {

                                    if (null == currentLayer)
                                    {

                                        SetLog(MessageType.Error, "Create new widget failure. current is null.");
                                        return;

                                    }
                                    UIEditorWidget uiWidget = new UIEditorWidget(new Vector4(UIController.width * 0.5f, UIController.height * 0.5f, 0, 0), GenWidgetIdx(), currentLayer);
                                    currentLayer.widgets.Add(uiWidget);

                                }

                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndScrollView();
                    }

                    // Draw widget detail.
                    if (null != currentWidget)
                    {
                        widgetDetailScrollPos = EditorGUILayout.BeginScrollView(widgetDetailScrollPos);
                        {
                            options = new[] { GUILayout.Width(250), GUILayout.Height(viewHeight) };
                            EditorGUILayout.BeginVertical("Box", options);
                            {
                                EditorGUILayout.BeginHorizontal();
                                {
                                    currentWidget.bRename = GUILayout.Toggle(currentWidget.bRename, "Rename");
                                    if (currentWidget.bRename)
                                    {
                                        options = new[] { GUILayout.Width(120), GUILayout.Height(16) };
                                        currentWidget.name = GUILayout.TextField(currentWidget.name, options);
                                    }
                                }
                                EditorGUILayout.EndHorizontal();

                                // Position.
                                GUILayout.Label(string.Format("Position X: {0}", currentWidget.position.x));
                                GUILayout.Label(string.Format("Position Y: {0}", currentWidget.position.y));
                                if (!currentWidget.rePosition)
                                {

                                    if (GUILayout.Button("RePosition"))
                                    {
                                        currentWidget.rePosition = true;
                                    }

                                }
                                else
                                {

                                    currentWidget.position = EditorGUILayout.Vector2Field("Position", currentWidget.position);

                                    if (GUILayout.Button("Close"))
                                    {
                                        currentWidget.rePosition = false;
                                    }

                                }

                                // Size.
                                GUILayout.Label(string.Format("Width: {0}", currentWidget.width));
                                GUILayout.Label(string.Format("Height: {0}", currentWidget.height));
                                if (!currentWidget.resize)
                                {

                                    if (GUILayout.Button("Resize"))
                                    {
                                        currentWidget.resize = true;
                                    }

                                }
                                else
                                {

                                    currentWidget.width = EditorGUILayout.IntField("Width", currentWidget.width);
                                    currentWidget.height = EditorGUILayout.IntField("Height", currentWidget.height);

                                    if (GUILayout.Button("Close"))
                                    {
                                        currentWidget.resize = false;
                                    }

                                }

                                // Draw custom detail.
                                EditorGUILayout.BeginHorizontal();
                                {
                                    var optWidget = GK.EnumNames<UIWidgetType>();
                                    currentWidget.type = (UIWidgetType)EditorGUILayout.Popup("Widget type: ", (int)currentWidget.type, optWidget);
                                }
                                EditorGUILayout.EndHorizontal();

                                EditorGUILayout.BeginHorizontal();
                                {
                                    currentWidget.anchors = (GKUIEditor.UIAncherType)EditorGUILayout.Popup("Anchors: ",
                                        (int)currentWidget.anchors, GK.EnumNames<GKUIEditor.UIAncherType>());
                                }
                                EditorGUILayout.EndHorizontal();

                                GUI.skin.label.fontSize = 12;
                                GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                                switch (currentWidget.type)
                                {
                                    case UIWidgetType.Texture:

                                        currentWidget.texData.type = EditorGUILayout.Popup("Texture type: ", currentWidget.texData.type, optTexture);
                                        currentWidget.texData.raycast = EditorGUILayout.Toggle("Ray cast: ", currentWidget.texData.raycast);
                                        currentWidget.serialize = EditorGUILayout.Toggle("Serialize: ", currentWidget.serialize);
                                        currentWidget.texData.color = EditorGUILayout.ColorField("Color: ", currentWidget.texData.color);

                                        switch (currentWidget.texData.type)
                                        {
                                            case 0: // Texture.

                                                currentWidget.texData.tex = EditorGUILayout.ObjectField("Texture: ", currentWidget.texData.tex, typeof(Texture2D), false) as Texture2D;

                                                break;
                                            case 1: // Sprite.
                                                EditorGUILayout.BeginVertical();
                                                {
                                                    currentWidget.texData.sprite = EditorGUILayout.ObjectField(currentWidget.texData.sprite, typeof(Sprite), false) as Sprite;
                                                    if (null != currentWidget.texData.sprite)
                                                    {
                                                        EditorGUILayout.BeginHorizontal();
                                                        {
                                                            currentWidget.texData.imageType =
                                                                (Image.Type)EditorGUILayout.Popup("Image type: ", (int)currentWidget.texData.imageType, GK.EnumNames<Image.Type>());
                                                        }
                                                        EditorGUILayout.EndHorizontal();
                                                    }
                                                }
                                                EditorGUILayout.EndVertical();
                                                break;
                                        }
                                        break;
                                    case UIWidgetType.Button:
                                        EditorGUILayout.BeginHorizontal();
                                        {
                                            currentWidget.btnData.type = EditorGUILayout.Popup("Button type: ", currentWidget.btnData.type, optButton);
                                        }
                                        EditorGUILayout.EndHorizontal();
                                        switch (currentWidget.btnData.type)
                                        {
                                            case 0: // Texture.
                                                EditorGUILayout.BeginVertical();
                                                {
                                                    currentWidget.btnData.tex = EditorGUILayout.ObjectField(currentWidget.btnData.tex, typeof(Texture2D), false) as Texture2D;
                                                }
                                                EditorGUILayout.EndVertical();
                                                break;
                                            case 1: // Sprite.
                                                EditorGUILayout.BeginVertical();
                                                {
                                                    currentWidget.btnData.sprite = EditorGUILayout.ObjectField(currentWidget.btnData.sprite, typeof(Sprite), false) as Sprite;
                                                    if (null != currentWidget.btnData.sprite)
                                                    {
                                                        EditorGUILayout.BeginHorizontal();
                                                        {
                                                            currentWidget.btnData.imageType =
                                                                (Image.Type)EditorGUILayout.Popup("Image type: ", (int)currentWidget.btnData.imageType, GK.EnumNames<Image.Type>());
                                                        }
                                                        EditorGUILayout.EndHorizontal();
                                                    }
                                                }
                                                EditorGUILayout.EndVertical();
                                                break;
                                        }

                                        EditorGUILayout.BeginHorizontal();
                                        {
                                            currentWidget.btnData.transition = (Selectable.Transition)EditorGUILayout.Popup("Transition: ",
                                                (int)currentWidget.btnData.transition, GK.EnumNames<Selectable.Transition>());
                                        }
                                        EditorGUILayout.EndHorizontal();

                                        switch (currentWidget.btnData.transition)
                                        {
                                            case Selectable.Transition.ColorTint:
                                                EditorGUILayout.BeginVertical();
                                                {
                                                    currentWidget.btnData.color[0] = EditorGUILayout.ColorField("Normal", currentWidget.btnData.color[0]);
                                                    currentWidget.btnData.color[1] = EditorGUILayout.ColorField("Hover", currentWidget.btnData.color[1]);
                                                    currentWidget.btnData.color[2] = EditorGUILayout.ColorField("Press", currentWidget.btnData.color[2]);
                                                    currentWidget.btnData.color[3] = EditorGUILayout.ColorField("Disable", currentWidget.btnData.color[3]);
                                                }
                                                EditorGUILayout.EndVertical();
                                                break;
                                            case Selectable.Transition.SpriteSwap:
                                                currentWidget.btnData.sprites[0] = EditorGUILayout.ObjectField("Hover", currentWidget.btnData.sprites[0], typeof(Sprite), false) as Sprite;
                                                currentWidget.btnData.sprites[1] = EditorGUILayout.ObjectField("Press", currentWidget.btnData.sprites[1], typeof(Sprite), false) as Sprite;
                                                currentWidget.btnData.sprites[2] = EditorGUILayout.ObjectField("Disable", currentWidget.btnData.sprites[2], typeof(Sprite), false) as Sprite;
                                                break;
                                            case Selectable.Transition.Animation:
                                                break;
                                            case Selectable.Transition.None:
                                                break;
                                        }

                                        // Audio.
                                        currentWidget.btnData.clickClip = EditorGUILayout.ObjectField("Auido clip: ", currentWidget.btnData.clickClip, typeof(AudioClip), false) as AudioClip;

                                        break;

                                    case UIWidgetType.Text:
                                        options = new[] { GUILayout.Width(140), GUILayout.Height(16) };
                                        GUILayout.Label("Content", options);
                                        GUI.skin.textArea.wordWrap = true;
                                        string strMsg = string.IsNullOrEmpty(currentWidget.textData.content) ? "" : currentWidget.textData.content;
                                        currentWidget.textData.content = EditorGUILayout.TextArea(strMsg, GUI.skin.textArea, GUILayout.Height(100f));
                                        currentWidget.textData.font = EditorGUILayout.ObjectField("Font: ", currentWidget.textData.font, typeof(Font), false) as Font;
                                        currentWidget.textData.alignmentLR = EditorGUILayout.Popup("Alignment Left-Right: ", currentWidget.textData.alignmentLR, optTextLR);
                                        currentWidget.textData.alignmentTB = EditorGUILayout.Popup("Alignment Top-Bottom: ", currentWidget.textData.alignmentTB, optTextTP);
                                        currentWidget.textData.lineSpace = EditorGUILayout.IntField("Line space: ", currentWidget.textData.lineSpace);
                                        currentWidget.textData.size = EditorGUILayout.IntField("Size: ", currentWidget.textData.size);
                                        currentWidget.textData.richText = EditorGUILayout.Toggle("Rich text: ", currentWidget.textData.richText);
                                        currentWidget.textData.color = EditorGUILayout.ColorField("Color: ", currentWidget.textData.color);
                                        currentWidget.textData.raycast = EditorGUILayout.Toggle("Ray cast: ", currentWidget.textData.raycast);
                                        currentWidget.serialize = EditorGUILayout.Toggle("Serialize: ", currentWidget.serialize);
                                        break;

                                    case UIWidgetType.Custom:
                                        currentWidget.customData.prefab = EditorGUILayout.ObjectField("Custom prefab: ", currentWidget.customData.prefab, typeof(GameObject), false) as GameObject;
                                        break;

                                    case UIWidgetType.EmptyObject:

                                        currentWidget.serialize = EditorGUILayout.Toggle("Serialize: ", currentWidget.serialize);

                                        break;
                                }

                                // Draw Events.
                                if (null != currentWidget && 0 != currentWidget.events.Count)
                                {
                                    for (int i = 0; i < currentWidget.events.Count; i++)
                                    {
                                        var e = currentWidget.events[i];

                                        EditorGUILayout.BeginVertical("Box");
                                        {
                                            var tList = GK.EnumNames<UIWidgetEventType>();
                                            int tIdx = (int)e.type;
                                            string[] tStrNone = { "None" };
                                            int tResult = 3;

                                            // Draw event type.
                                            e.type = (UIWidgetEventType)EditorGUILayout.Popup("Event type: ", (int)e.type, tList.ToArray());

                                            if (UIWidgetEventType.None == e.type)
                                            {

                                                e.type = UIWidgetEventType.None;

                                            }
                                            else if (UIWidgetEventType.WidgetMethod == e.type)
                                            {

                                                // Draw event widget name pop menu.
                                                tList = GetSerializableWidgetsName().ToArray();
                                                tIdx = GK.GetElementIdxByList(tList, e.widgetName);

                                                if (0 < tList.Length)
                                                {

                                                    // From no element to element, you need to reset the index to 0.
                                                    if (tIdx < 0)
                                                        tIdx = 0;

                                                    tIdx = EditorGUILayout.Popup("Widget name: ", tIdx, tList.ToArray());
                                                    tResult--;

                                                    if (tIdx >= 0)
                                                    {
                                                        e.widgetName = tList[tIdx];

                                                        var w = GetWidgetByName(e.widgetName);
                                                        if (null != w)
                                                        {
                                                            e.TypeName = w.GetSerializeName();

                                                            // Draw method pop menu.
                                                            List<string> tMethodList = new List<string>();
                                                            foreach (var m in GKUIScriptMaker.genericType2FunListDef)
                                                            {
                                                                tMethodList.Add(m);
                                                            }
                                                            if (UIWidgetType.Texture == w.type)
                                                            {
                                                                foreach (var m in GKUIScriptMaker.texture2FunListDef)
                                                                {
                                                                    tMethodList.Add(m);
                                                                }
                                                                if (0 == w.texData.type)
                                                                {
                                                                    foreach (var m in GKUIScriptMaker.rawImage2FunListDef)
                                                                    {
                                                                        tMethodList.Add(m);
                                                                    }
                                                                }
                                                                else if (1 == w.texData.type)
                                                                {
                                                                    foreach (var m in GKUIScriptMaker.image2FunListDef)
                                                                    {
                                                                        tMethodList.Add(m);
                                                                    }
                                                                }
                                                            }
                                                            else if (UIWidgetType.Button == w.type)
                                                            {
                                                                foreach (var m in GKUIScriptMaker.button2FunListDef)
                                                                {
                                                                    tMethodList.Add(m);
                                                                }
                                                            }
                                                            else if (UIWidgetType.Text == w.type)
                                                            {
                                                                foreach (var m in GKUIScriptMaker.text2FunListDef)
                                                                {
                                                                    tMethodList.Add(m);
                                                                }
                                                            }

                                                            if (UIWidgetType.Custom != w.type || UIWidgetType.EmptyObject != w.type)
                                                            {
                                                                tIdx = GK.GetElementIdxByList(tMethodList, e.methodName);

                                                                // From no element to element, you need to reset the index to 0.
                                                                if (tIdx < 0 || tIdx >= tMethodList.Count)
                                                                {
                                                                    tIdx = 0;

                                                                    // If the function name is not empty, 
                                                                    // It represents the change of the target component type. 
                                                                    // The function type should be reset.
                                                                    //																if (!string.Equals (e.methodName, "")) {
                                                                    //																	EditorUtility.DisplayDialog ("Warning", "The widget type is changed. " +
                                                                    //																	"Please change the corresponding function", "I Know");
                                                                    //																}
                                                                }

                                                                tIdx = EditorGUILayout.Popup("Method: ", tIdx, tMethodList.ToArray());
                                                                e.methodName = tMethodList[tIdx];
                                                                tResult--;

                                                                // Draw param pop menu.
                                                                switch (e.methodName)
                                                                {
                                                                    case "SetColor":

                                                                        var c = AdjustEventsParam<Color>(e.paramList);
                                                                        c = EditorGUILayout.ColorField("Color: ", c);
                                                                        e.paramList[0] = c;

                                                                        break;

                                                                    case "SetScale":

                                                                        var v3 = AdjustEventsParam<Vector3>(e.paramList);
                                                                        v3 = EditorGUILayout.Vector3Field("Scale: ", v3);
                                                                        e.paramList[0] = v3;

                                                                        break;

                                                                    case "SetActive":

                                                                        var b = AdjustEventsParam<bool>(e.paramList);
                                                                        b = EditorGUILayout.Toggle("Active: ", b);
                                                                        e.paramList[0] = b;

                                                                        break;

                                                                    case "SetTexture":

                                                                        var tex = AdjustEventsParam<Texture2D>(e.paramList);
                                                                        tex = EditorGUILayout.ObjectField("Texture: ", tex, typeof(Texture2D), false) as Texture2D;
                                                                        e.paramList[0] = tex;

                                                                        break;

                                                                    case "SetSprite":

                                                                        var sp = AdjustEventsParam<Sprite>(e.paramList);
                                                                        sp = EditorGUILayout.ObjectField("Sprite: ", sp, typeof(Sprite), false) as Sprite;
                                                                        e.paramList[0] = sp;

                                                                        break;

                                                                    case "SetContent":

                                                                        var str = AdjustEventsParam<string>(e.paramList);
                                                                        if (string.IsNullOrEmpty(str))
                                                                        {
                                                                            str = "";
                                                                        }
                                                                        str = EditorGUILayout.TextArea(str, GUI.skin.textArea, GUILayout.Height(100f));
                                                                        e.paramList[0] = str;

                                                                        break;
                                                                }


                                                                tResult--;
                                                            }
                                                        }
                                                    }
                                                }

                                                if (2 < tResult)
                                                {
                                                    tIdx = 0;
                                                    EditorGUILayout.Popup("Name: ", tIdx, tStrNone);
                                                }
                                                if (1 < tResult)
                                                {
                                                    EditorGUILayout.Popup("Method: ", tIdx, tStrNone);
                                                }
                                            }
                                        }
                                        EditorGUILayout.EndVertical();
                                    }

                                }

                                if (GUILayout.Button("Add Event"))
                                {
                                    UIWidgetEvent e = new UIWidgetEvent();
                                    currentWidget.events.Add(e);
                                }

                                // Draw Component.
                                if (null != currentWidget && 0 != currentWidget.components.Count)
                                {
                                    for (int i = 0; i < currentWidget.components.Count; i++)
                                    {
                                        var c = currentWidget.components[i];

                                        EditorGUILayout.BeginVertical("Box");
                                        {
                                            var tList = GK.EnumNames<UIWidgetComponentType>();
                                            int tIdx = (int)c.type;

                                            // Draw event type.
                                            c.type = (UIWidgetComponentType)EditorGUILayout.Popup("Component type: ", (int)c.type, tList);

                                            if (UIWidgetComponentType.None == c.type)
                                            {

                                            }
                                            else if (UIWidgetComponentType.Audio == c.type)
                                            {

                                                var audioSrc = AdjustEventsParam<AudioClip>(c.paramList);
                                                audioSrc = EditorGUILayout.ObjectField("Auido clip: ", audioSrc, typeof(AudioClip), false) as AudioClip;
                                                c.paramList[0] = audioSrc;

                                            }
                                            else if (UIWidgetComponentType.Drag == c.type)
                                            {

                                                var iTypeIdx = AdjustEventsParam<int>(c.paramList, 0);
                                                var strRoot = AdjustEventsParam<string>(c.paramList, 1);
                                                var strGrid = AdjustEventsParam<string>(c.paramList, 2);

                                                // Draw interaction type.
                                                var tInterTypes = GK.EnumNames<UIDragDropItem.InteractionType>();
                                                iTypeIdx = EditorGUILayout.Popup("Interaction type: ", iTypeIdx, tInterTypes);
                                                c.paramList[0] = iTypeIdx;

                                                var strSetWidgetNames = GetSerializableWidgetsName().ToArray();

                                                if (strSetWidgetNames.Length > 0)
                                                {

                                                    // Draw root gameobject name.
                                                    tIdx = GK.GetElementIdxByList(strSetWidgetNames, strRoot);
                                                    if (tIdx < 0)
                                                    {
                                                        tIdx = 0;
                                                    }
                                                    tIdx = EditorGUILayout.Popup("Root: ", tIdx, strSetWidgetNames);
                                                    strRoot = strSetWidgetNames[tIdx];
                                                    c.paramList[1] = strRoot;

                                                    // Draw grid gameobject name.
                                                    tIdx = GK.GetElementIdxByList(strSetWidgetNames, strGrid);
                                                    if (tIdx < 0)
                                                    {
                                                        tIdx = 0;
                                                    }
                                                    tIdx = EditorGUILayout.Popup("Grid: ", tIdx, strSetWidgetNames);
                                                    strGrid = strSetWidgetNames[tIdx];
                                                    c.paramList[2] = strGrid;

                                                }
                                                else
                                                {
                                                    GUILayout.Label("Unable to find serialization object.");
                                                }

                                            }

                                        }
                                        EditorGUILayout.EndVertical();
                                    }
                                }

                                if (GUILayout.Button("Add Component"))
                                {
                                    UIWidgetComponent c = new UIWidgetComponent();
                                    currentWidget.components.Add(c);
                                }

                            }
                            EditorGUILayout.EndVertical();
                        }
                        EditorGUILayout.EndScrollView();

                    }
                }
                EditorGUILayout.EndHorizontal();

                GKEditor.DrawInspectorSeperator();

                EditorGUILayout.BeginHorizontal("Box");
                {
                    options = new[] { GUILayout.Width(70), GUILayout.Height(60) };
                    if (GUILayout.Button("Import\nPng", options))
                    {
                        string strDefaultPath = GetDefaultFolder();
                        string path = EditorUtility.OpenFilePanel("Import hierarchical file", strDefaultPath, "");
                        if (!string.IsNullOrEmpty(path))
                        {
                            string p = GKFileUtil.GetAssetPath(path);
                            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                            Add(tex);
                        }
                    }

                    if (GUILayout.Button("Import\nPng folder", options))
                    {
                        string strDefaultPath = GetDefaultFolder();
                        string path = EditorUtility.OpenFolderPanel("Import hierarchical folder", strDefaultPath, "");
                        if (!string.IsNullOrEmpty(path))
                        {

                            targetName = GKFileUtil.GetDirctoryName(path);

                            string[] files = GKFileUtil.GetFilesInDirectory(path);
                            foreach (var f in files)
                            {
                                string p = GKFileUtil.GetAssetPath(f);
                                string extension = System.IO.Path.GetExtension(p);
                                if (extension.Contains("jpg") || extension.Contains("png"))
                                {
                                    Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                                    Add(tex);
                                }
                            }
                        }
                    }

                    if (GUILayout.Button("Import\nJs", options))
                    {

                        Debug.Log(string.Format("Import Js file."));

                        string strDefaultPath = GetDefaultFolder();
                        string path = EditorUtility.OpenFilePanel("Select Import file", strDefaultPath, "");

                        if (Path.GetExtension(path) == ".json")
                        {

                            StreamReader sr = new StreamReader(path);
                            if (null != sr)
                            {

                                string json = sr.ReadToEnd();
                                if (0 < json.Length)
                                {

                                    UIEditorLayer jsonLayer = UIEditorLayer.CreateFromJSON(json);

                                    UIEditorLayer layer = GetLayerByName(Path.GetFileNameWithoutExtension(path));
                                    if (null != layer)
                                    {

                                        list.Remove(layer);

                                    }

                                    list.Add(jsonLayer);

                                }

                            }
                            sr.Close();

                        }

                    }

                    if (GUILayout.Button("Import\nJs folder", options))
                    {

                        Debug.Log(string.Format("Import Js folder."));

                        string strDefaultPath = GetDefaultFolder();
                        string path = EditorUtility.OpenFolderPanel("Select Import folder", strDefaultPath, "");
                        var files = GKFileUtil.GetFilesInDirectory(path);

                        foreach (var f in files)
                        {

                            if (Path.GetExtension(f) == ".json")
                            {

                                StreamReader sr = new StreamReader(f);
                                if (null != sr)
                                {

                                    string json = sr.ReadToEnd();
                                    if (0 < json.Length)
                                    {

                                        UIEditorLayer jsonLayer = UIEditorLayer.CreateFromJSON(json);

                                        UIEditorLayer layer = GetLayerByName(Path.GetFileNameWithoutExtension(f));
                                        if (null != layer)
                                        {

                                            layer.CopyData(jsonLayer);

                                        }
                                        else
                                        {

                                            list.Add(jsonLayer);

                                        }

                                    }

                                }
                                sr.Close();
                            }

                        }

                        Debug.Log(string.Format("List GetLayerCount: {0}", list.Count));

                        foreach (var l in list)
                        {
                            l.SyncJsonData();
                        }

                        // Set current layer.
                        if (null == currentLayer && list.Count > 0)
                        {
                            currentLayer = list[0];
                        }

                    }

                    if (GUILayout.Button("Export\nJs", options))
                    {

                        string strDefaultPath = GetDefaultFolder();
                        string path = EditorUtility.OpenFolderPanel("Select export folder", strDefaultPath, "");
                        if (!string.IsNullOrEmpty(path))
                        {

                            if (Directory.Exists(path) && null != currentLayer)
                            {

                                GenJsonByData(path, currentLayer);

                            }

                        }

                    }

                    if (GUILayout.Button("Export\nJs folder", options))
                    {

                        string strDefaultPath = GetDefaultFolder();
                        string path = EditorUtility.OpenFolderPanel("Select export folder", strDefaultPath, "");
                        if (!string.IsNullOrEmpty(path))
                        {

                            foreach (var l in list)
                            {

                                GenJsonByData(path, l);

                            }

                        }

                    }

                    if (GUILayout.Button("Import\nPrefab", options))
                    {



                    }

                    if (GUILayout.Button("Generate", options))
                    {
                        if (string.IsNullOrEmpty(targetName))
                        {
                            SetLog(MessageType.Error, "Generate failure. Please input ui prefab name.");
                            return;
                        }

                        bool result = GenUI();

                        if (!result)
                        {
                            return;
                        }

                        SetLog(MessageType.Info, "Generate success.");
                    }

                    if (GUILayout.Button("Reset", options))
                    {
                        Reset();
                    }

                    EditorGUILayout.BeginVertical();
                    {

                        GUI.skin.label.fontSize = 12;
                        GUI.skin.label.alignment = TextAnchor.MiddleLeft;

                        // Set demo mode.
                        options = new[] { GUILayout.Width(100), GUILayout.Height(16) };
                        bDemo = GUILayout.Toggle(bDemo, "Mode: is Demo?", options);

                        GUILayout.Label("UI Prefab Name", options);
                        targetName = GUILayout.TextField(targetName, options);

                        // Set global skin.
                        globalSkin = EditorGUILayout.ObjectField("Global skin: ", globalSkin, typeof(GKUISkin), false) as GKUISkin;
                        foreach (var l in list)
                        {
                            if (null == l.defaultSkin)
                            {
                                l.defaultSkin = globalSkin;
                            }
                        }

                    }
                    EditorGUILayout.EndVertical();

                }
                EditorGUILayout.EndHorizontal();



                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.HelpBox(logContent, logLv);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();

            // Draw highlight based on the currently selected widget. 
            DrawWidget(currentWidget, Color.yellow);
        }

        private void DrawWidget(UIEditorWidget w, Color c)
        {

            if (null != w)
            {

                // 0.0058 This value is the default difference between box and texture. It comes through manual testing.
                float widthK = viewWidth / UIController.width - 0.0058f;
                float heightK = viewHeight / UIController.height - 0.0058f;

                Rect rect = new Rect();
                rect.x = 10 + w.position.x * widthK;
                rect.y = 98 + (UIController.height - w.position.y - w.height) * heightK;
                rect.width = w.width * widthK;
                rect.height = w.height * heightK;

                EditorGUI.DrawRect(rect, c);
            }

        }

        private void Reset()
        {
            list.Clear();

            layerScrollPos = new Vector2(0, 0);
            widgetScrollPos = new Vector2(0, 0);
            widgetDetailScrollPos = new Vector2(0, 0);

            GKUIMaker.currentLayer = null;
            GKUIMaker.currentWidget = null;

            SetLog(MessageType.Info, " ");
        }

        private void Add(Texture2D tex)
        {
            if (null == tex)
            {
                SetLog(MessageType.Error, "Add layout failure. The layout missing or parses the exception.");
                return;
            }

            UIEditorLayer layer = new UIEditorLayer(tex);
            list.Add(layer);

            currentLayer = layer;

            tex = null;

            SetLog(MessageType.Info, "Add layout success.");
        }

        // Bubble sort.
        private static void ReorderingLayer()
        {
            list.Sort();
        }

        // Bubble sort.
        private static void ReorderingWidgetSort()
        {
            currentLayer.widgets.Sort();
        }

        private static void RecalcLayerIdx()
        {
            int idx = 0;
            for (int i = 0; i < list.Count; i++)
            {

                if (list[i].enable)
                {
                    list[i].layerIdx = idx;
                    idx++;
                }

            }

        }

        // Automatic generation of UI objects based on data.
        private bool GenUI()
        {
            // Check.
            bool result = CheckNames();
            if (!result)
            {
                return false;
            }

            // Create scritp.
            result = GKUIScriptMaker.GenerateScripts(list, bDemo);
            if (!result)
            {
                return false;
            }

            // Create root.
            GameObject root = GKUIEditor.CreateNode(targetName, GKUIEditor.ui.GetOrAddGroup("UIRoot"), "UI");
            GKUIEditor.AdjustNodeData(root, Vector3.zero, Vector3.zero, Vector2.zero, GKUIEditor.UIAncherType.FullScree);

            for (int i = 0; i < list.Count; i++)
            {

                if (list[i].enable)
                {
                    CreateLayer(list[i], root);
                }

            }

            // Save prefab to folder.
            string savePath = string.Format("Assets/Resources/{0}UI/Panels/{1}.prefab", bDemo ? "_Demo/" : "", targetName);
            GKEditor.CreateOrReplacePrefab(savePath, root);
            AssetDatabase.SaveAssets();
            GK.Destroy(root);
            Reset();
            return true;
        }

        private void CreateLayer(UIEditorLayer layer, GameObject root)
        {

            GameObject layerRoot = null;

            // If the layer is the root node, create and initialize the corresponding parent node. Otherwise bound widget.
            if (layer.enable)
            {
                layerRoot = GKUIEditor.CreateNode(layer.name, root, "UI");
                GKUIEditor.AdjustNodeData(layerRoot, Vector3.zero, Vector3.zero, Vector2.zero, GKUIEditor.UIAncherType.FullScree);
            }
            else
            {
                layerRoot = GK.FindChild(root, layer.parentWidget.name, true);
            }

            // Create widgets.
            CreateWidgets(layer, layerRoot, root);

            // sub layer.
            foreach (var l in layer.list)
            {
                CreateLayer(l, root);
            }
        }

        private void CreateWidgets(UIEditorLayer layer, GameObject layerRoot, GameObject root)
        {

            // The offset between the parent node and the world coordinate.
            Vector3 offest = layerRoot.transform.position - root.transform.position;

            for (int j = 0; j < layer.widgets.Count; j++)
            {

                GameObject widgetRoot = GKUIEditor.CreateNode(layer.widgets[j].name, layerRoot, "UI");

                switch (layer.widgets[j].type)
                {
                    case UIWidgetType.Texture:

                        var tex = layer.widgets[j];

                        if (0 == tex.texData.type)
                        {
                            var rawImg = GK.GetOrAddComponent<RawImage>(widgetRoot);
                            rawImg.texture = tex.texData.tex;
                            rawImg.raycastTarget = tex.texData.raycast;
                            rawImg.color = tex.texData.color;
                        }
                        else if (1 == tex.texData.type)
                        {
                            var img = GK.GetOrAddComponent<Image>(widgetRoot);
                            img.sprite = tex.texData.sprite;
                            img.type = tex.texData.imageType;
                            img.raycastTarget = tex.texData.raycast;
                            img.color = tex.texData.color;
                        }

                        GKUIEditor.AdjustNodeData(widgetRoot, new Vector3(tex.position.x, tex.position.y, 0), Vector3.zero,
                            new Vector2(tex.width, tex.height), tex.anchors);

                        break;
                    case UIWidgetType.Button:

                        Button btnGo = GK.GetOrAddComponent<Button>(widgetRoot);
                        var btn = layer.widgets[j];

                        if (0 == btn.btnData.type)
                        {
                            var rawImg = GK.GetOrAddComponent<RawImage>(widgetRoot);
                            rawImg.texture = btn.btnData.tex;
                        }
                        else if (1 == btn.btnData.type)
                        {
                            var img = GK.GetOrAddComponent<Image>(widgetRoot);
                            img.sprite = btn.btnData.sprite;
                            img.type = btn.btnData.imageType;
                        }

                        GKUIEditor.AdjustNodeData(widgetRoot, new Vector3(btn.position.x, btn.position.y, 0), Vector3.zero,
                            new Vector2(btn.width, btn.height), btn.anchors);

                        btnGo.targetGraphic = btnGo.GetComponent<Graphic>();

                        if (Selectable.Transition.ColorTint == btn.btnData.transition)
                        {
                            btnGo.transition = Selectable.Transition.ColorTint;
                            ColorBlock cb = new ColorBlock();
                            cb.normalColor = btn.btnData.color[0];
                            cb.highlightedColor = btn.btnData.color[1];
                            cb.pressedColor = btn.btnData.color[2];
                            cb.disabledColor = btn.btnData.color[3];
                            cb.colorMultiplier = 1;
                            cb.fadeDuration = 0.15f;
                            btnGo.colors = cb;
                        }
                        else if (Selectable.Transition.SpriteSwap == btn.btnData.transition)
                        {
                            btnGo.transition = Selectable.Transition.SpriteSwap;
                            SpriteState ss = new SpriteState();
                            ss.highlightedSprite = btn.btnData.sprites[0];
                            ss.pressedSprite = btn.btnData.sprites[1];
                            ss.disabledSprite = btn.btnData.sprites[2];
                            btnGo.spriteState = ss;
                        }
                        else if (Selectable.Transition.Animation == btn.btnData.transition)
                        {
                            btnGo.transition = Selectable.Transition.Animation;

                        }

                        UnityEngine.UI.Navigation navi = new Navigation();
                        navi.mode = Navigation.Mode.None;
                        btnGo.navigation = navi;

                        // Audio.
                        if (null != btn.btnData.clickClip)
                        {
                            var audiosrc = GK.GetOrAddComponent<AudioSource>(btnGo.gameObject);
                            audiosrc.clip = btn.btnData.clickClip;
                            audiosrc.playOnAwake = false;
                            audiosrc.loop = false;
                        }

                        break;

                    case UIWidgetType.Text:

                        Text textGo = GK.GetOrAddComponent<Text>(widgetRoot);
                        var text = layer.widgets[j];

                        textGo.font = text.textData.font;
                        textGo.text = text.textData.content;
                        textGo.alignment = text.textData.GetAnchor();
                        textGo.color = text.textData.color;
                        textGo.fontSize = text.textData.size;
                        textGo.lineSpacing = text.textData.lineSpace;
                        textGo.supportRichText = text.textData.richText;
                        textGo.raycastTarget = text.textData.raycast;

                        GKUIEditor.AdjustNodeData(widgetRoot, new Vector3(text.position.x, text.position.y, 0), Vector3.zero,
                            new Vector2(text.width, text.height), text.anchors);

                        break;

                    case UIWidgetType.Custom:

                        if (null != layer.widgets[j].customData.prefab)
                        {
                            var p = layer.widgets[j].customData.prefab;
                            GK.SetParent(p, widgetRoot, false);

                            var customData = layer.widgets[j];

                            GK.GetOrAddComponent<UIBase>(p).OnPosition(new Vector3(customData.position.x, customData.position.y, 0));
                            GK.GetOrAddComponent<UIBase>(p).OnSize(new Vector2(customData.width, customData.height));

                        }
                        else
                        {
                            SetLog(MessageType.Error, "Create Custom Node failure.");
                        }

                        break;

                    case UIWidgetType.EmptyObject:



                        break;

                }

                var tLocalPos = widgetRoot.transform.localPosition;
                //			Debug.Log (string.Format ("offest:{0} | tLocalPos: {1}", offest, tLocalPos));
                tLocalPos = new Vector3(tLocalPos.x - offest.x, tLocalPos.y - offest.y, tLocalPos.z - offest.z);
                widgetRoot.transform.localPosition = tLocalPos;
                //			Debug.Log (string.Format ("tLocalPos:{0}", tLocalPos));

                // Generate component.
                foreach (var c in layer.widgets[j].components)
                {

                    switch (c.type)
                    {
                        case UIWidgetComponentType.Audio:

                            if (c.paramList.Count > 0 && null != c.paramList[0])
                            {
                                var audiosrc = GK.GetOrAddComponent<AudioSource>(widgetRoot);
                                audiosrc.clip = (AudioClip)c.paramList[0];
                                audiosrc.playOnAwake = false;
                                audiosrc.loop = false;
                            }

                            break;

                        case UIWidgetComponentType.Drag:

                            if (c.paramList.Count > 2)
                            {

                                var dragItem = GK.GetOrAddComponent<UIDragDropItem>(widgetRoot);

                                // Set ineraction type.
                                int t = (int)c.paramList[0];
                                dragItem.interactionType = (UIDragDropItem.InteractionType)t;

                                // Set move gameobject.
                                string rootName = (string)c.paramList[1];
                                GameObject r = root ?? layerRoot;
                                GameObject tMove = GK.FindChild(r, rootName, true);
                                if (null != tMove)
                                {
                                    dragItem.moveRoot = tMove;
                                }

                                // Set grid gameobject.
                                string gridName = (string)c.paramList[2];
                                GameObject tGrid = GK.FindChild(r, gridName, true);
                                if (null != tGrid)
                                {
                                    tGrid.tag = tagUIDrag;
                                    dragItem.gridTag = tagUIDrag;
                                    dragItem.gameObject.tag = tagUIDrag;
                                }

                            }

                            break;
                    }

                }

            }

        }

        private bool CheckNames()
        {
            List<string> nameList = new List<string>();
            nameList.Clear();

            foreach (var l in list)
            {
                foreach (var widget in l.widgets)
                {
                    if (widget.show && widget.serialize)
                    {
                        if (nameList.Contains(widget.name.Trim()))
                        {
                            SetLog(MessageType.Error, string.Format("Widget name repetition. name: {0}", widget.name));
                            return false;
                        }
                        nameList.Add(widget.name);
                    }
                }
            }
            return true;
        }

        private List<string> GetSerializableWidgetsName()
        {
            List<string> names = new List<string>();
            foreach (var l in list)
            {
                foreach (var widget in l.widgets)
                {
                    if (widget.show && widget.serialize)
                    {
                        names.Add(widget.name);
                    }
                }
            }
            return names;
        }

        private T AdjustEventsParam<T>(List<object> l, int idx = 0)
        {
            T param;
            if (l.Count >= idx + 1 && null != l[idx])
            {
                if (l[idx].GetType() == typeof(T))
                {
                    param = (T)l[idx];
                    return param;
                }
            }
            else
            {
                object o = new object();
                l.Add(o);
            }
            return default(T);
        }

        // Generating data structure based on prefab.
        private void LoadPrefab(GameObject prefab)
        {

            //		GK.FindChild

        }

        // Generating data based on json.
        private void GenDataFromFile(string path)
        {

        }

        // Generating data based on json.
        private void GenDataFromFolder(string path)
        {

        }

        // Generating json based on data.
        private void GenJsonByData(string path, UIEditorLayer layer)
        {

            if (null == layer)
            {
                return;
            }

            layer.GenGIDList();

            string json = JsonUtility.ToJson(layer);
            string layerPath = string.Format("{0}/{1}.json", path, layer.name);

            //		Debug.Log ("GenJsonByData layerPath: " + layerPath);
            File.WriteAllText(layerPath, json, Encoding.UTF8);

        }

        private string GetDefaultFolder()
        {
            return string.Format("{0}/UI{1}/Layout/", Application.dataPath, bDemo ? "/_Demo" : "");
        }
        #endregion
    }
}