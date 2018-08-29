using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System;
using UnityEngine.UI;

namespace GKUI
{
    [System.Serializable]
    public class UIEditorWidget : IComparable
    {
        [System.Serializable]
        public class TextureWidget
        {
            public TextureWidget()
            {

            }

            public TextureWidget(TextureWidget t)
            {
                type = t.type;
                tex = t.tex;
                sprite = t.sprite;
                color = t.color;
                imageType = t.imageType;
                raycast = t.raycast;
            }

            private int _type = 0;  // 0: texture, 1: sprite.
            public int type
            {
                set
                {
                    if (_type != value)
                    {
                        _type = value;

                        if (null != GKUIMaker.currentWidget)
                        {
                            GKUIMaker.currentWidget.GenName();
                        }
                    }
                }
                get
                {
                    return _type;
                }
            }
            public Texture2D tex;
            public Sprite sprite;
            public Color color = Color.white;

            public Image.Type imageType = Image.Type.Simple;
            public bool raycast = false;
        }

        [System.Serializable]
        public class ButtonWidget
        {
            public ButtonWidget()
            {

            }

            public ButtonWidget(ButtonWidget t)
            {
                type = t.type;
                tex = t.tex;
                sprite = t.sprite;
                for (int i = 0; i < 4; i++)
                {
                    color[i] = t.color[i];
                }

                animation = t.animation;
                clickClip = t.clickClip;

                imageType = t.imageType;
                transition = t.transition;
            }

            public int _type = 0;   // 0: texture, 1: sprite, 2: color.
            public int type
            {
                set
                {
                    if (_type != value)
                    {
                        _type = value;

                        if (null != GKUIMaker.currentWidget)
                        {
                            GKUIMaker.currentWidget.GenName();
                        }
                    }
                }
                get
                {
                    return _type;
                }
            }
            public Texture2D tex;
            public Sprite sprite;

            // Transition.
            public Color[] color = { Color.white, Color.white, Color.white, Color.white };
            public Sprite[] sprites = new Sprite[3];
            public Animation animation;
            public AudioClip clickClip;

            public Image.Type imageType = Image.Type.Simple;
            public Selectable.Transition transition = Selectable.Transition.ColorTint;
        }

        [System.Serializable]
        public class TextWidget
        {
            public TextWidget()
            {

            }

            public TextWidget(TextWidget t)
            {
                content = t.content;
                size = t.size;
                lineSpace = t.lineSpace;
                alignmentLR = t.alignmentLR;
                alignmentTB = t.alignmentTB;
                color = t.color;
                richText = t.richText;
                raycast = t.raycast;
                font = t.font;
            }

            public Font font = null;
            public string content = "";
            public int size = 24;
            public int lineSpace = 1;
            public int alignmentLR = 1; // 0: Left 1: Middle 2: Right.
            public int alignmentTB = 1; // 0: Top 1: Middle 2: Bottom.
            public bool richText = true;
            public Color color = Color.black;
            public bool raycast = false;

            public TextAnchor GetAnchor()
            {
                if (0 == alignmentLR && 0 == alignmentTB)
                {
                    return TextAnchor.UpperLeft;
                }
                else if (1 == alignmentLR && 0 == alignmentTB)
                {
                    return TextAnchor.UpperCenter;
                }
                else if (2 == alignmentLR && 0 == alignmentTB)
                {
                    return TextAnchor.UpperRight;
                }
                else if (0 == alignmentLR && 1 == alignmentTB)
                {
                    return TextAnchor.MiddleLeft;
                }
                else if (0 == alignmentLR && 2 == alignmentTB)
                {
                    return TextAnchor.LowerLeft;
                }
                else if (1 == alignmentLR && 1 == alignmentTB)
                {
                    return TextAnchor.MiddleCenter;
                }
                else if (1 == alignmentLR && 2 == alignmentTB)
                {
                    return TextAnchor.LowerCenter;
                }
                else if (2 == alignmentLR && 1 == alignmentTB)
                {
                    return TextAnchor.MiddleRight;
                }
                else if (2 == alignmentLR && 2 == alignmentTB)
                {
                    return TextAnchor.LowerRight;
                }

                return TextAnchor.MiddleCenter;
            }
        }

        [System.Serializable]
        public class CustomWidget
        {
            public CustomWidget()
            {

            }

            public CustomWidget(CustomWidget t)
            {
                prefab = t.prefab;
            }

            public GameObject prefab;
        }

        public bool show;
        public string name;
        private bool _bRename = false;
        public bool bRename
        {
            set
            {
                if (_bRename != value)
                {
                    _bRename = value;

                    if (null != GKUIMaker.currentWidget && !_bRename)
                    {
                        GKUIMaker.currentWidget.GenName();
                    }
                }
            }
            get
            {
                return _bRename;
            }
        }
        public GKUIEditor.UIAncherType anchors = GKUIEditor.UIAncherType.MiddleCenter;
        public int sort;
        public UIEditorLayer layer;
        public Vector2 position;
        public bool rePosition;
        public int width;
        public int height;
        public bool resize;
        private GKUIMaker.UIWidgetType _type = GKUIMaker.UIWidgetType.Texture;
        public GKUIMaker.UIWidgetType type
        {
            set
            {
                if (_type != value)
                {
                    _type = value;

                    if (null != GKUIMaker.currentWidget)
                    {
                        GKUIMaker.currentWidget.GenName();
                    }

                    if (GKUIMaker.UIWidgetType.Button == _type || GKUIMaker.UIWidgetType.Custom == _type)
                    {
                        serialize = true;
                    }
                    else
                    {
                        serialize = false;
                    }
                }
            }
            get
            {
                return _type;
            }
        }
        public TextureWidget texData = new TextureWidget();
        public ButtonWidget btnData = new ButtonWidget();
        public TextWidget textData = new TextWidget();
        public CustomWidget customData = new CustomWidget();

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

                if (null != _defaultSkin)
                {
                    if (null == texData.tex && null == texData.sprite)
                    {
                        texData.tex = _defaultSkin.texSkin.tex;
                        texData.sprite = _defaultSkin.texSkin.sprite;
                        texData.color = _defaultSkin.texSkin.color;
                        texData.imageType = _defaultSkin.texSkin.imageType;
                        texData.raycast = _defaultSkin.texSkin.raycast;
                    }

                    if (null != btnData)
                    {
                        if (null == btnData.tex && null == btnData.sprite)
                        {
                            btnData.tex = _defaultSkin.btnSkin.normalTex;

                            btnData.sprite = _defaultSkin.btnSkin.normalSprite;
                            btnData.sprites[0] = _defaultSkin.btnSkin.highLightSprite;
                            btnData.sprites[1] = _defaultSkin.btnSkin.pressSprite;
                            btnData.sprites[2] = _defaultSkin.btnSkin.disableSprite;

                            btnData.color[0] = _defaultSkin.btnSkin.normalColor;
                            btnData.color[1] = _defaultSkin.btnSkin.highLightColor;
                            btnData.color[2] = _defaultSkin.btnSkin.pressColor;
                            btnData.color[3] = _defaultSkin.btnSkin.disableColor;

                            btnData.animation = _defaultSkin.btnSkin.animation;
                            btnData.clickClip = _defaultSkin.btnSkin.clickClip;

                            btnData.imageType = _defaultSkin.btnSkin.imageType;
                        }
                    }

                    if (null != textData)
                    {
                        if (null == textData.font)
                        {
                            textData.font = _defaultSkin.textSkin.font;
                            textData.content = _defaultSkin.textSkin.content;
                            textData.size = _defaultSkin.textSkin.size;
                            textData.lineSpace = _defaultSkin.textSkin.lineSpace;
                            textData.alignmentLR = _defaultSkin.textSkin.alignmentLR;
                            textData.alignmentTB = _defaultSkin.textSkin.alignmentTB;
                            textData.richText = _defaultSkin.textSkin.richText;
                            textData.color = _defaultSkin.textSkin.color;
                        }
                    }
                }
            }
        }

        public bool serialize = false;
        public List<UIWidgetEvent> events = new List<UIWidgetEvent>();
        public List<UIWidgetComponent> components = new List<UIWidgetComponent>();


        public UIEditorWidget(Vector4 v4, int idx, UIEditorLayer l)
        {
            Reset();
            show = true;
            sort = idx;
            position.x = v4.x;
            position.y = v4.y;
            width = (int)v4.z;
            height = (int)v4.w;
            layer = l;
            GenName();
            defaultSkin = l.defaultSkin;
            serialize = false;
        }

        public UIEditorWidget(UIEditorWidget w)
        {
            show = w.show;
            name = w.name;
            anchors = w.anchors;
            sort = w.sort;
            layer = w.layer;
            position.x = w.position.x;
            position.y = w.position.y;
            width = w.width;
            height = w.height;
            type = w.type;
            texData = new TextureWidget(w.texData);
            btnData = new ButtonWidget(w.btnData);
            customData = new CustomWidget(w.customData);
            defaultSkin = w.defaultSkin;
            serialize = w.serialize;
        }

        public int CompareTo(object obj)
        {
            UIEditorWidget w = obj as UIEditorWidget;
            return sort.CompareTo(w.sort);
        }

        public void Reset()
        {
            show = false;
            name = "";
            sort = 0;
            layer = null;
            position = Vector2.zero;
            width = 0;
            height = 0;
            defaultSkin = null;
            serialize = false;
        }

        public void ResetData()
        {
            defaultSkin = null;
            texData = new TextureWidget();
            btnData = new ButtonWidget();
            customData = new CustomWidget();
        }

        public void GenName()
        {
            if (null == layer)
            {
                name = "UnknowWidget";
                return;
            }

            if (bRename)
            {
                return;
            }

            int showType = 0;
            switch (type)
            {
                case GKUIMaker.UIWidgetType.Texture:
                    showType = texData.type;
                    break;
                case GKUIMaker.UIWidgetType.Button:
                    showType = btnData.type;
                    break;
                case GKUIMaker.UIWidgetType.Text:
                    showType = 0;
                    break;
                case GKUIMaker.UIWidgetType.Custom:
                    showType = 0;
                    break;
                case GKUIMaker.UIWidgetType.EmptyObject:
                    showType = 0;
                    break;
            }

            name = string.Format("{0}_{1}_{2}_{3}", type.ToString(), showType, layer.layerIdx, sort);
        }

        public void Draw()
        {
            EditorGUILayout.BeginHorizontal("Box");

            var options = new[] { GUILayout.Width(16), GUILayout.Height(16) };
            show = GUILayout.Toggle(show, "", options);

            options = new[] { GUILayout.Width(120), GUILayout.Height(16) };
            if (GUILayout.Button(string.Format("{0} [{1}]", name, sort), options))
            {
                GKUIMaker.currentWidget = this;
            }

            Color c = GUI.color;
            options = new[] { GUILayout.Width(22), GUILayout.Height(16) };
            GUI.color = Color.red;

            EditorGUI.BeginDisabledGroup(0 == sort);
            {
                if (GUILayout.Button("-", options))
                {
                    GKUIMaker.ModifySort(this, -1);
                }
            }
            EditorGUI.EndDisabledGroup();

            GUI.color = Color.green;

            bool showModifySort = false;
            if (null == GKUIMaker.currentLayer || GKUIMaker.currentLayer.widgets.Count - 1 == sort)
            {
                showModifySort = true;
            }
            EditorGUI.BeginDisabledGroup(showModifySort);
            {
                if (GUILayout.Button("+", options))
                {
                    GKUIMaker.ModifySort(this, 1);
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
            EditorGUILayout.EndHorizontal();
        }

        public string GetSerializeName()
        {
            return GKUIMaker.GetSerializeName(type, texData);
        }

        public Color GetWidgetColor()
        {

            switch (type)
            {
                case GKUIMaker.UIWidgetType.Texture:
                    return Color.gray;
                case GKUIMaker.UIWidgetType.Button:
                    return Color.cyan;
                case GKUIMaker.UIWidgetType.Text:
                    return Color.white;
                case GKUIMaker.UIWidgetType.Custom:
                    return Color.blue;
                case GKUIMaker.UIWidgetType.EmptyObject:
                    return Color.red;
                default:
                    return Color.black;
            }

        }

        public static UIEditorWidget CreateFromJSON(string json)
        {
            return JsonUtility.FromJson<UIEditorWidget>(json);
        }

    }

    [System.Serializable]
    public class UIWidgetEvent
    {
        public GKUIMaker.UIWidgetEventType type = GKUIMaker.UIWidgetEventType.None;
        public string widgetName = "";
        public string TypeName = "";
        public string methodName = "";
        public List<object> paramList = new List<object>();
    }

    [System.Serializable]
    public class UIWidgetComponent
    {
        public GKUIMaker.UIWidgetComponentType type = GKUIMaker.UIWidgetComponentType.None;
        public List<object> paramList = new List<object>();
    }
}