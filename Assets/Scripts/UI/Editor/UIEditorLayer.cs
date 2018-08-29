using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System;
using GKBase;

namespace GKUI
{
    [System.Serializable]
    public class UIEditorLayer : IComparable
    {
        public bool show = true;
        public int layerIdx = 0;
        public bool enable = true;
        public Texture2D tex = null;
        public string name = "";
        public int width = 0;
        public int height = 0;
        public bool resize = false;
        public bool editorChildmode = false;

        public GKUISkin _defaultSkin;
        public GKUISkin defaultSkin
        {
            get
            {
                return _defaultSkin;
            }
            set
            {
                _defaultSkin = value;
                foreach (var w in widgets)
                {
                    if (null == w.defaultSkin)
                    {
                        w.defaultSkin = _defaultSkin;
                    }
                }
            }
        }

        [NonSerialized]
        // sub layer list.
        public List<UIEditorLayer> list = new List<UIEditorLayer>();
        // sub layer gid list. 
        public List<string> subLayerGIDlist = new List<string>();
        [NonSerialized]
        public UIEditorLayer parentLayer = null;
        public string parentLayerGID = "";
        [NonSerialized]
        public UIEditorWidget parentWidget = null;
        public string parentWidgetGID = "";

        public List<UIEditorWidget> widgets = new List<UIEditorWidget>();
        //		public List<string> widgetsGID = new List<string> ();

        public UIEditorLayer()
        {
            name = "";
            tex = null;
            layerIdx = GKUIMaker.GenLayerIdx();
            width = UIController.width;
            height = UIController.height;
            list.Clear();
        }

        // The new generated layer defaults to the current layer.
        public UIEditorLayer(Texture2D t)
        {
            name = t.name;
            tex = t;
            layerIdx = GKUIMaker.GenLayerIdx();
            GKUIMaker.currentLayer = this;
            if (null != tex)
            {
                width = tex.width;
                height = tex.height;
                var l = GK.RectangleRecognition(t);
                for (int i = 0; i < l.Count; i++)
                {
                    UIEditorWidget widget = new UIEditorWidget(l[i], i, this);
                    widgets.Add(widget);
                    GKUIMaker.currentWidget = widget;
                }
            }
            list.Clear();
        }

        public UIEditorLayer(UIEditorLayer l)
        {
            CopyData(l);
        }

        public void CopyData(UIEditorLayer l)
        {

            show = l.show;
            layerIdx = l.layerIdx;
            enable = l.enable;
            name = l.name;
            tex = l.tex;
            if (null != tex)
            {
                width = tex.width;
                height = tex.height;
            }

            // When cloning the layer, if it is cloned as the current layer, the reference relation is transferred.
            if (null != GKUIMaker.currentLayer && layerIdx == GKUIMaker.currentLayer.layerIdx && -1 != layerIdx)
            {
                GKUIMaker.currentLayer = this;
            }

            widgets.Clear();
            foreach (var e in l.widgets)
            {
                UIEditorWidget w = new UIEditorWidget(e);
                w.layer = this;
                widgets.Add(w);

                // Refresh the current widget reference.
                if (null != GKUIMaker.currentLayer && layerIdx == GKUIMaker.currentLayer.layerIdx && -1 != layerIdx)
                    GKUIMaker.currentWidget = w;
            }

            l.list.Clear();
            foreach (var layer in l.list)
            {

                layer.parentLayer = this;
                layer.ResetParentWidget(this);
                list.Add(layer);

            }

            defaultSkin = l.defaultSkin;

            parentWidget = l.parentWidget;
            parentLayer = l.parentLayer;
            editorChildmode = l.editorChildmode;

        }

        public int CompareTo(object obj)
        {
            UIEditorLayer l = obj as UIEditorLayer;
            return layerIdx.CompareTo(l.layerIdx);
        }

        public void Draw()
        {
            if (!enable)
                return;

            EditorGUILayout.BeginVertical("Box");
            {
                EditorGUILayout.BeginHorizontal("Box");
                {
                    var options = new[] { GUILayout.Width(16), GUILayout.Height(16) };
                    show = GUILayout.Toggle(show, "", options);

                    editorChildmode = GUILayout.Toggle(editorChildmode, "C", options);

                    options = new[] { GUILayout.Width(100), GUILayout.Height(16) };
                    if (GUILayout.Button(string.Format("{0} [{1}]", name, layerIdx), options))
                    {
                        GKUIMaker.currentLayer = this;
                        GKUIMaker.currentWidget = GetFocusWidget();
                    }

                    Color c = GUI.color;
                    options = new[] { GUILayout.Width(22), GUILayout.Height(16) };
                    GUI.color = Color.red;

                    EditorGUI.BeginDisabledGroup(0 == layerIdx);
                    {
                        if (GUILayout.Button("-", options))
                        {
                            GKUIMaker.ModifyLayer(this, -1);
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    GUI.color = Color.green;

                    EditorGUI.BeginDisabledGroup(GKUIMaker.GetLayerCount() - 1 == layerIdx);
                    {
                        if (GUILayout.Button("+", options))
                        {
                            GKUIMaker.ModifyLayer(this, 1);
                        }
                    }
                    EditorGUI.EndDisabledGroup();

                    // Skin.
                    if (null == defaultSkin)
                    {
                        options = new[] { GUILayout.Width(40), GUILayout.Height(16) };
                        defaultSkin = EditorGUILayout.ObjectField(defaultSkin, typeof(GKUISkin), false, options) as GKUISkin;
                    }
                    else
                    {
                        GUI.color = Color.yellow;
                        if (GUILayout.Button("R", options))
                        {
                            ResetData();
                        }
                    }

                    GUI.color = c;
                }
                EditorGUILayout.EndHorizontal();

                if (editorChildmode)
                {


                    List<UIEditorLayer> tLayerList = GKUIMaker.GetAllLayerList();
                    Dictionary<string, UIEditorLayer> tLayerNames = new Dictionary<string, UIEditorLayer>();
                    Dictionary<string, UIEditorWidget> tWidgetNames = new Dictionary<string, UIEditorWidget>();
                    int tParentIdx = 0;

                    // Set layer.
                    tLayerList.Remove(this);
                    foreach (var ll in tLayerList)
                    {
                        tLayerNames.Add(ll.name, ll);
                    }

                    if (tLayerNames.Count > 0)
                    {

                        if (null != parentLayer)
                        {

                            foreach (var k in tLayerNames.Keys)
                            {
                                if (parentLayer.name.Equals(k))
                                {
                                    break;
                                }
                                tParentIdx++;
                            }

                        }
                        tParentIdx = EditorGUILayout.Popup("Parent layer: ", tParentIdx, tLayerNames.Keys.ToArray());
                        parentLayer = tLayerList[tParentIdx];

                        // Set widget.
                        tParentIdx = 0;
                        foreach (var w in parentLayer.widgets)
                        {
                            tWidgetNames.Add(w.name, w);
                        }
                        if (null != parentWidget && null != parentLayer)
                        {

                            foreach (var w in tWidgetNames.Keys)
                            {
                                if (parentWidget.name.Equals(w))
                                {
                                    break;
                                }
                                tParentIdx++;
                            }

                        }

                        if (tParentIdx >= tWidgetNames.Keys.Count)
                        {
                            Debug.Log("tParentIdx" + tParentIdx + "| tWidgetNames count: " + tWidgetNames.Keys.Count);
                            if (null != parentWidget)
                            {
                                Debug.Log("widget name: " + parentWidget.name);
                            }
                            tParentIdx = 0;
                        }

                        if (tWidgetNames.Keys.Count > 0)
                        {

                            tParentIdx = EditorGUILayout.Popup("Parent widget: ", tParentIdx, tWidgetNames.Keys.ToArray());
                            parentWidget = parentLayer.widgets[tParentIdx];

                        }

                        if (GUILayout.Button("Change"))
                        {

                            if (null != parentLayer && null != parentWidget)
                            {
                                GKUIMaker.ChangeLayerParent(this, parentLayer, parentWidget);
                            }
                            else
                            {
                                GKUIMaker.SetLog(MessageType.Error, "Please set layer objects and widget objects.");
                            }

                        }
                    }

                }

                // Sub layer.
                EditorGUILayout.BeginVertical();
                {
                    foreach (var subLayer in list)
                    {

                        if (!DrawSubLayer(subLayer))
                        {
                            return;
                        }

                    }
                }
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndVertical();
        }

        private bool DrawSubLayer(UIEditorLayer layer)
        {

            if (null == layer)
            {
                GKUIMaker.SetLog(MessageType.Error, string.Format("DrawSubLayer failure. Layer is null."));
                return false;
            }

            EditorGUILayout.BeginHorizontal("Box");
            {

                Color c = GUI.color;
                var options = new[] { GUILayout.Width(40), GUILayout.Height(16) };
                GUI.color = Color.white;

                if (GUILayout.Button("<---", options))
                {
                    GKUIMaker.RollBackLayer(layer);
                    return false;
                }

                GUI.color = c;

                options = new[] { GUILayout.Width(16), GUILayout.Height(16) };
                layer.show = GUILayout.Toggle(layer.show, "", options);

                if (null != layer.parentWidget)
                {

                    options = new[] { GUILayout.Width(100), GUILayout.Height(16) };
                    if (GUILayout.Button(string.Format("{0}", layer.parentWidget.name), options))
                    {
                        GKUIMaker.currentLayer = layer;
                        GKUIMaker.currentWidget = layer.GetFocusWidget();
                    }

                }

            }
            EditorGUILayout.EndHorizontal();

            if (layer.list.Count > 0)
            {

                EditorGUILayout.BeginVertical("Box");
                {
                    foreach (var sub in layer.list)
                    {

                        if (!DrawSubLayer(sub))
                        {
                            return false;
                        }

                    }
                }
                EditorGUILayout.EndVertical();

            }

            return true;

        }

        public void ResetData()
        {
            defaultSkin = null;
        }

        // Generating layer data based on JSON.
        public void SyncJsonData()
        {

            foreach (var l in subLayerGIDlist)
            {

                UIEditorLayer tLayer = GKUIMaker.GetLayerByName(l);
                if (null != tLayer)
                {
                    list.Add(tLayer);
                }

            }

            parentLayer = GKUIMaker.GetLayerByName(parentLayerGID);
            if (null != parentLayer)
            {
                parentWidget = GKUIMaker.GetWidgetByName(parentLayer, parentWidgetGID);
            }

        }

        public void ResetParentWidget(UIEditorLayer layer)
        {
            if (null == parentWidget)
            {
                return;
            }

            foreach (var w in layer.widgets)
            {
                if (w.name.Equals(parentWidget.name))
                {
                    parentWidget = w;
                    return;
                }
            }
        }

        public UIEditorWidget GetFocusWidget()
        {

            if (widgets.Count > 0)
            {
                return widgets[0];
            }
            else
            {
                return null;
            }

        }

        public static UIEditorLayer CreateFromJSON(string json)
        {
            return JsonUtility.FromJson<UIEditorLayer>(json);
        }

        // Building reference relationships for Json.
        public void GenGIDList()
        {

            subLayerGIDlist.Clear();
            foreach (var s in list)
            {
                subLayerGIDlist.Add(s.name);
            }

            parentLayerGID = (null != parentLayer) ? parentLayer.name : "";
            parentWidgetGID = (null != parentWidget) ? parentWidget.name : "";

        }
    }

}