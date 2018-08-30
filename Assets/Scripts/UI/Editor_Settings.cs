using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Editor_Settings : ScriptableObject
{
    [System.Serializable]
    public class ToyMakerBase
    {
        // 目录优先级.
        public int MenuItemPriority = 900;
        // 最小宽高.
        public int _minWidth = 960;
        public int _minHeight = 640;
        // 最大宽高.
        public int _maxWidth = 960 * 2;
        public int _maxHeight = 640 * 2;
        // 标准行高度高度.
        public int _lineHeight = 20;
        // 信息面板宽度.
        public int _informationWidth = 240;
        // 布局间距.
        public int _layoutSpace = 4;
        // 信息栏行最大字符数.
        public int _infoMaxLineChar = 25;
        // 字符宽度.
        public int _charWidth = 12;
        // 字符高度.
        public int _charHeight = 16;
        // Action 最小宽度.
        public int _nodeMinWidth = 80;
        // Action 最小高度.
        public int _nodeMinHeight = 30;
        // 内容背景色.
        public Color _bgColor = new Color(0.6f, 0.6f, 0.6f);
        // 行为节点背景色.
        public Color _actionColor = new Color(0.6f, 0.6f, 0.6f);
        // 条件节点背景色.
        public Color _conditionColor = new Color(0.6f, 0.6f, 0.6f);
        // 最小缩放因子.
        public float _minScale = 1f;
        // 最大缩放因子.
        public float _maxScale = 2;
        // 样式.
        public GUIStyle _titleStyle;
        public GUIStyle _nodeStyle;
        public GUIStyle _commentStyle;
		[HideInInspector]
		public float _commentContentMargin;
		// 连接点击范围扩展.
		public float linkClickOffset = 3;
		// 图标.
		public Texture[] _icons;
    }
    public  ToyMakerBase toyMakerBase = new ToyMakerBase();
}
