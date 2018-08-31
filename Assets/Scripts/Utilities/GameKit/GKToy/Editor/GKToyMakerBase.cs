using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GKBase;

/**
 * 游戏模块编辑器基类.
 * 各模块编辑器窗口基类.
 * 核心功能: 支持模块逻辑编辑.
 */
namespace GKToy
{
    public class GKToyMakerBase : EditorWindow
    {
        #region PublicField
        public static GKToyMakerBase instance;
        protected Editor_Settings _settings;
        public Editor_Settings Settings
        {
            get
            {
                if (_settings == null)
                {
                    _settings = GK.LoadResource<Editor_Settings>("UI/Settings/Editor_Settings");
                }
                return _settings;
            }
        }
        public Editor_Settings.ToyMakerBase toyMakerBase = null;
        protected GKToyBaseOverlord _overlord;
        #endregion

        #region PrivateField
        // 节点内容缩放因子.
        protected float _contentSacle = 1;
        protected float Scale
        {
            get { return _contentSacle; }
            set
            {
                if(toyMakerBase._minScale > value)
                {
                    _contentSacle = toyMakerBase._minScale;
                    return;
                }
                if(toyMakerBase._maxScale < value)
                {
                    _contentSacle = toyMakerBase._maxScale;
                    return;
                }
                _contentSacle = value;
            }
        }
        // 当前Node索引. 用于产生GUID.
        protected int _curNodeIdx = 0;
        // 当前Link索引，用于产生Link的GUID.
        protected int _curLinkIdx = 0;
        // 事件内容滚动条位置.
        protected Vector2 _contentScrollPos = new Vector2(0f, 0f);
        // 鼠标是否拖拽中.
        protected bool _isDrag = false;
        protected bool _isLinking = false;
        protected bool _isScale = false;
        // 当前信息界面类型.
        protected InformationType _infoType = InformationType.Detail;
        // Node链表.
        //protected Dictionary<int, GKToyNode> _nodeLst = new Dictionary<int, GKToyNode>();
        protected GKToyNode _selectNode = null;
        // 临时节点缓存.
        protected GKToyNode _tmpSelectNode = null;
		// 当前选中Link的Id.
		protected Link _selectLink = null;
        // 点击到的元素.
        protected ClickedElement _clickedElement = ClickedElement.NodeElement;
        // 视口区域.
        protected Rect _contentView;
        protected Rect _contentRect = new Rect();
        protected Rect _nonContentRect;
        // 缩放视口偏移缓存.
        protected Vector2 _tmpScalePos = Vector2.zero;
        // 鼠标距节点中心的偏移量.
        protected Vector2 _mouseOffset = Vector2.zero;
        // 链接变更列表.
        // !!!逻辑渲染分离, 等渲染完毕后再进行逻辑处理, 规避渲染时变更渲染内容所产生的异常.
        protected Dictionary <int, List<GKToyNode>> _newLinkLst = new Dictionary<int, List<GKToyNode>>();
        protected Dictionary<int, List<GKToyNode>> _removeLinkLst = new Dictionary<int, List<GKToyNode>>();
        // GUI 数据备份.
        protected Color _lastColor;
        protected Color _lastBgColor;
		#endregion

		#region PublicMethod
		[MenuItem("GK/ToyMaker/Toy Maker Base", false, GKEditorConfiger.MenuItemPriorityA)]
        public static void MenuItem_Window()
        {
            instance = GetWindow<GKToyMakerBase>("Logic Maker", true);
        }
		#endregion

		private void OnEnable()
        {
            Init();
            wantsMouseMove = true;
            minSize = new Vector2(toyMakerBase._minWidth, toyMakerBase._minHeight);
            maxSize = new Vector2(toyMakerBase._minWidth, toyMakerBase._minHeight);
            Show();
        }

        private void OnGUI()
        {
            EventProcess();
            Render();
        }

        private void Update()
        {
            SelectChanged();

            if (null == _overlord)
                return;

            Changed();
            UpdateLinks();
        }

		private void Render()
		{
            if(null == _overlord)
            {
                
                GUILayout.BeginArea(_nonContentRect);
                {
                    GUILayout.BeginVertical("Box");
                    {
                            GUILayout.Label("Create a new task or select a record.");
                            if (GUILayout.Button("Create", GUILayout.Width(200), GUILayout.Height(30)))
                            {
                                CreateData();
                            }
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndArea();
                
            }
            else
            {
                GUILayout.BeginHorizontal();
                {
                    DrawInformation();
                    GUILayout.BeginVertical("Box", GUILayout.ExpandHeight(true));
                    {
                        DrawToolBar();
                        DrawContent();
                    }
                    GUILayout.EndVertical();
                }
                GUILayout.EndHorizontal();
            }
		}

		#region Event
        // 按键响应.
        private void EventProcess()
        {
            if (null == Event.current || null == _overlord)
                return;

            // 缓存内容坐标. 防止缩放时移动.
            _isScale = Event.current.alt;
            if (_isScale)
                _tmpScalePos = _contentScrollPos;

            // Zoom in or out.
            if (Event.current.alt && Event.current.isScrollWheel)
            {
                if (Event.current.delta.y < 0)
                {
                    Scale -= 0.01f;
                }

                if (Event.current.delta.y > 0)
                {
                    Scale += 0.01f;
                }
                Repaint();
            }

            switch (Event.current.rawType)
            {
                case EventType.MouseDown:
                    _clickedElement = ClickedElement.NoElement;
                    if (0 == Event.current.button)
                    {
                        _isDrag = true;
                    }
                    UpdateTouch();
                    break;
                //case EventType.MouseDrag:
                //break;
                case EventType.MouseUp:
                    if (0 == Event.current.button)
                    {
                        _isDrag = false;
                        if (_isLinking)
                        {
                            _isLinking = false;
                            CheckLink();
                        }
                    }
                    if (null != _selectNode)
                        _selectNode.isMove = false;
                    break;
            }
        }

        // 触摸事件响应.
        private void UpdateTouch()
        {
            if (UpdateNodeTouch())
                return;
            UpdateLinkTouch();
        }

        // 更新节点点击逻辑.
        private bool UpdateNodeTouch()
        {
            // 链接时不可点击.
            if (_isLinking)
                return false;

            Vector2 mousePos = Event.current.mousePosition + _contentScrollPos;

            foreach (var node in _overlord.data.nodeLst)
            {
                if (node.inputRect.Contains(mousePos))
                {
                    _tmpSelectNode = node;
                    return true;
                }
                else if (node.outputRect.Contains(mousePos))
                {
                    _isLinking = true;
                    _tmpSelectNode = node;
                    return true;
                }
                else if (node.rect.Contains(mousePos))
                {
                    _clickedElement = ClickedElement.NodeElement;
                    _tmpSelectNode = node;
                    _tmpSelectNode.isMove = true;
                    _selectLink = null;
                    _mouseOffset = mousePos - node.rect.position;
                    return true;
                }
            }
            return false;
        }

		// 链接线段点选检测.
		private bool UpdateLinkTouch()
		{
            if (0 == _overlord.data.nodeLst.Count)
				return false;

            foreach (var node in _overlord.data.nodeLst)
			{
				var links = node.links;
				if (0 == links.Count)
					continue;

				foreach (Link link in links)
				{
					for (int i = 0; i < link.points.Count - 1; ++i)
					{
						bool isVertical = 0 == (i & 1) ^ link.isFirstVertical;

						Rect lineRect;
						float x = Mathf.Min(link.points[i].x, link.points[i + 1].x) - toyMakerBase.linkClickOffset;
						float y = Mathf.Min(link.points[i].y, link.points[i + 1].y) - toyMakerBase.linkClickOffset;
						if (isVertical)
						{
							lineRect = new Rect(x, y, toyMakerBase.linkClickOffset * 2, Mathf.Abs(link.points[i].y - link.points[i + 1].y));
						}
						else
						{
							lineRect = new Rect(x, y, Mathf.Abs(link.points[i].x - link.points[i + 1].x), toyMakerBase.linkClickOffset * 2);
						}
						if (lineRect.Contains(Event.current.mousePosition))
						{
							_selectLink = link;
                            _tmpSelectNode = node;
							_clickedElement = ClickedElement.LinkElement;
							// 如果是选中的连接，则跳过最后再画，否则高亮色会被遮住.
							return true;
						}
					}
				}
			}
            return false;
		}

        // 检测是否链接。
        private void CheckLink()
        {
            if (null == Event.current && null == _selectNode)
                return;
            Vector2 mousePos = Event.current.mousePosition + _contentScrollPos;
            foreach (var node in _overlord.data.nodeLst)
            {
                if (node.id == _selectNode.id)
                    continue;

                if (node.inputRect.Contains(mousePos) || node.rect.Contains(mousePos) && null == _selectNode.FindLinkFromNode(node))
                {
                    if (!_newLinkLst.ContainsKey(_selectNode.id))
                        _newLinkLst[_selectNode.id] = new List<GKToyNode>();

                    _newLinkLst[_selectNode.id].Add(node);
                    return;
                }
            }
        }
		#endregion

		#region Information
		// 绘制信息界面.
		private void DrawInformation()
        {
            GUILayout.BeginVertical("Box",GUILayout.Width(toyMakerBase._informationWidth), GUILayout.ExpandHeight(true));
            {
                // 绘制Tab.
                GUILayout.BeginHorizontal();
                {
                    foreach(var type in GK.EnumValues<InformationType>()) 
                    {
                        if ( GUILayout.Toggle(type == _infoType, type.ToString(), EditorStyles.toolbarButton, GUILayout.Height(toyMakerBase._lineHeight)))
                        {
                            _infoType = type;
                        }
                    }
                }
                GUILayout.EndHorizontal();
                // 绘制内容.
                DrawInformationContent();
            }
            GUILayout.EndVertical();
        }

        // 绘制信息界面.
        private void DrawInformationContent()
        {
            switch (_infoType)
            {
                case InformationType.Detail:
                    DrawDetail();
                    break;
                case InformationType.Actions:
                    DrawTasks();
                    break;
                case InformationType.Variables:
                    break;
                case InformationType.Inspector:
                    DrawInspector();
                    break;
            }
        }
        #endregion

        #region Content
        // 绘制工具栏.
        private void DrawToolBar()
        {
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.Height(toyMakerBase._lineHeight)))
                {

                }
                if (GUILayout.Button("Back to center", EditorStyles.toolbarButton, GUILayout.Width(120), GUILayout.Height(toyMakerBase._lineHeight)))
                {

                }
                if (GUILayout.Button("Lock", EditorStyles.toolbarButton, GUILayout.Width(120), GUILayout.Height(toyMakerBase._lineHeight)))
                {

                }
                if (GUILayout.Button("Export", EditorStyles.toolbarButton, GUILayout.Width(120), GUILayout.Height(toyMakerBase._lineHeight)))
                {

                }
                if (GUILayout.Button("Take screenshot", EditorStyles.toolbarButton, GUILayout.Width(160), GUILayout.Height(toyMakerBase._lineHeight)))
                {

                }
            }
            GUILayout.EndHorizontal();
        }

        // 绘制事件内容.
        private void DrawContent()
        {
            _contentRect.x = toyMakerBase._informationWidth + toyMakerBase._layoutSpace * 3;
            _contentRect.y = toyMakerBase._lineHeight + toyMakerBase._layoutSpace;
            _contentRect.width = (toyMakerBase._maxWidth - toyMakerBase._informationWidth - toyMakerBase._layoutSpace * 4) * Scale;
            _contentRect.height = (toyMakerBase._maxHeight - toyMakerBase._lineHeight - toyMakerBase._layoutSpace * 3) * Scale;
            _contentScrollPos = GUI.BeginScrollView(_contentView, _contentScrollPos, _contentRect);
            {

                if (_isScale)
                {
                    _contentScrollPos = _tmpScalePos;
                }

                DrawBlackGroundGrid();
                DrawLinks();

                // 绘制行为节点.
                foreach (var node in _overlord.data.nodeLst)
                {
                    DrawNode(node, Scale, _isDrag, _mouseOffset, _selectNode);
                    if (_isDrag)
                        Repaint();
                }

                DrawMenu(_contentRect);

            }
            GUI.EndScrollView();

            DrawContentInformation();
        }

        // 绘制内容信息.
        private void DrawContentInformation()
        {
            // 标题绘制.
            GUI.Label(new Rect(toyMakerBase._informationWidth + toyMakerBase._layoutSpace * 3 + 10,
                               toyMakerBase._lineHeight + toyMakerBase._layoutSpace, 400, 100),
                      string.Format("{0}-{1}", _overlord.data.moduleType.ToString(), _overlord.data.name), toyMakerBase._titleStyle);
            // 缩放比列尺绘制.
            GUI.Label(new Rect(toyMakerBase._minWidth - 140, toyMakerBase._lineHeight + toyMakerBase._layoutSpace + 10, 100, 100),
                      string.Format("X {0:N1} ", Scale), toyMakerBase._titleStyle);
        }

        // 绘制背景网格.
        private void DrawBlackGroundGrid()
        {
            GUI.backgroundColor = toyMakerBase._bgColor;
            GUI.Box(new Rect(0, 0, toyMakerBase._maxWidth * 2 * Scale, toyMakerBase._maxHeight * 2 * Scale), "", toyMakerBase._contentStyle);
            GUI.backgroundColor = _lastBgColor;
            Handles.color = toyMakerBase._fgColor;
            int x1 = toyMakerBase._informationWidth + toyMakerBase._layoutSpace * 3;
            int x2 = (int)((toyMakerBase._maxWidth * 2 - toyMakerBase._layoutSpace) * Scale);
            int y1 = toyMakerBase._layoutSpace;
            int space = 0;
            for (int i = 0; y1 + i * 25 / Scale < toyMakerBase._maxHeight * 2 * Scale - toyMakerBase._layoutSpace; i++)
            {
                space = (int)(y1 + i * 25 / Scale);
                //每3根加粗.
                if (i % 3 == 0)
                {
                    Handles.DrawLine(new Vector2(x1, y1 + space - 80 / Screen.dpi), new Vector3(x2, y1 + space - 80 / Screen.dpi));
                    Handles.DrawLine(new Vector2(x1, y1 + space + 80 / Screen.dpi), new Vector3(x2, y1 + space + 80 / Screen.dpi));
                }
                Handles.DrawLine(new Vector2(x1, y1 + space), new Vector3(x2, y1 + space));
            }

            y1 = toyMakerBase._layoutSpace;
            int y2 = toyMakerBase._maxHeight * 2 - toyMakerBase._layoutSpace;
            x1 = toyMakerBase._layoutSpace * 3;
            for (int i = 0; x1 + i * 25 / Scale < toyMakerBase._maxWidth * 2 * Scale - toyMakerBase._layoutSpace; i++)
            {
                space = (int)(x1 + i * 25 / Scale);

                //每3根加粗.
                if (i % 3 == 0)
                {
                    Handles.DrawLine(new Vector2(x1 + space - 80 / Screen.dpi, y1), new Vector3(x1 + space - 80 / Screen.dpi, y2));
                    Handles.DrawLine(new Vector2(x1 + space + 80 / Screen.dpi, y1), new Vector3(x1 + space + 80 / Screen.dpi, y2));
                }
                Handles.DrawLine(new Vector2(x1 + space, y1), new Vector3(x1 + space, y2));
            }
        }
        #endregion

        #region Update
        // 缓存数据赋值..
        protected void Changed()
        {
            if (0 != _newLinkLst.Count)
            {
                foreach (var link in _newLinkLst)
                {
                    foreach (var l in link.Value)
                    {
                        GKToyNode n = _overlord.data.GetNodeByID(link.Key);
                        if(null != n)
                            n.AddLink(_curLinkIdx++, l);
                    }

                }
                _newLinkLst.Clear();
            }

            if (0 != _removeLinkLst.Count)
            {
                foreach (var link in _removeLinkLst)
                {
                    foreach (var l in link.Value)
                    {
                        GKToyNode n = _overlord.data.GetNodeByID(link.Key);
                        if (null != n)
                            n.RemoveLink(l);
                    }
                }
                _removeLinkLst.Clear();
            }

            if (null != _tmpSelectNode)
            {
                _selectNode = _tmpSelectNode;
                _tmpSelectNode = null;
            }
        }

        // 拖拽、缩放过程中更新链接线段.
        private void UpdateLinks()
        {
            if (_isDrag && !_isLinking && null != _selectNode)
            {
                _selectNode.UpdateAllLinks();
                foreach (GKToyNode node in _overlord.data.nodeLst)
                {
                    Link l = node.FindLinkFromNode(_selectNode);
                    if (null != l)
                        node.UpdateLink(l);
                }
            }
            else if (_isScale)
            {
                foreach (GKToyNode node in _overlord.data.nodeLst)
                {
                    node.UpdateAllLinks();
                }
            }
        }
        #endregion

        #region Detail
        // 绘制简介.
        virtual protected void DrawDetail()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name ", GUILayout.Height(toyMakerBase._lineHeight));
                _overlord.data.name = GUILayout.TextField(_overlord.data.name, toyMakerBase._infoMaxLineChar, GUILayout.Height(toyMakerBase._lineHeight));
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("Comment", GUILayout.Height(toyMakerBase._lineHeight));
            _overlord.data.comment = GUILayout.TextArea(_overlord.data.comment, GUILayout.Height(toyMakerBase._lineHeight * 5));
        }
        #endregion

        #region Task
        // 绘制节点列表.
        virtual protected void DrawTasks()
        {
            if (GUILayout.Button("Create", GUILayout.Height(toyMakerBase._lineHeight)))
            {
                GKToyNode node = new GKToyNode();
                node.id = _curNodeIdx++;
                node.pos.x = (_contentScrollPos.x + toyMakerBase._minWidth * 0.5f) / Scale;
                node.pos.y = (_contentScrollPos.y + toyMakerBase._minHeight * 0.5f) / Scale;
                CreateNode(node);
            }
        }
        #endregion

        #region Content

        //------------------------------ Link ------------------------------

        // 渲染链接线段.
        private void DrawLinks()
        {
            if (null == Event.current || null == _selectNode)
                return;

            Handles.color = Color.black;
            // Draw current link line.
            if (_isLinking && null != _selectNode)
            {
                DrawCurrentLink(_selectNode);
                Repaint();
            }

            // Draw links.
            foreach (var node in _overlord.data.nodeLst)
            {
                DrawLinks(node, _selectLink);
            }

            // 绘制高亮连接.
            if (null != _selectLink)
            {
                Handles.color = Color.yellow;
                for (int i = 0; i < _selectLink.points.Count - 1; ++i)
                {
                    DrawLine(_selectLink.points[i], _selectLink.points[i + 1], 0 == (i & 1) ^ _selectLink.isFirstVertical);
                }
                Handles.color = Color.black;
            }
        }

        // 绘制当前拖拽链接线.
        public void DrawCurrentLink(GKToyNode node)
        {
            float x = node.outputRect.x + node.outputRect.width;
            float y = node.outputRect.y + node.outputRect.height * 0.5f;
            DrawLine(new Vector2(x, y), Event.current.mousePosition);
        }

        // 绘制所有链接.
        public void DrawLinks(GKToyNode node, Link selectLink)
        {
            if (0 == node.links.Count)
                return;

            foreach (Link link in node.links)
            {
                // 高连线段最后绘制.
                if (null != selectLink && link.id == selectLink.id)
                    continue;

                for (int i = 0; i < link.points.Count - 1; ++i)
                {
                    bool isVertical = 0 == (i & 1) ^ link.isFirstVertical;
                    DrawLine(link.points[i], link.points[i + 1], isVertical);
                }
            }
        }

        protected void DrawLine(Vector2 src, Vector2 dest)
        {
            bool vertical = false;
            var lst = GK.ClacLinePoint(src, dest, out vertical);
            switch (lst.Count)
            {
                case 2:
                    DrawLine(lst[0], lst[1], vertical);
                    break;
                case 4:
                    DrawLine(lst[0], lst[1], false);
                    DrawLine(lst[1], lst[2], true);
                    DrawLine(lst[2], lst[3], false);
                    break;
                default:
                    Debug.LogError(string.Format("DrawCurrentLink faile. point Count: {0}", lst.Count));
                    break;
            }
        }

        protected void DrawLine(Vector2 src, Vector2 dest, bool vertical)
        {
            float val = 1;
            for (int i = 0; i < 5; i++)
            {
                if (vertical)
                {
                    Handles.DrawLine(new Vector2(src.x + val, src.y), new Vector3(dest.x + val, dest.y));
                }
                else
                {
                    Handles.DrawLine(new Vector2(src.x, src.y + val), new Vector3(dest.x, dest.y + val));
                }
                val -= 0.5f;
            }
        }

        //------------------------------ Node ------------------------------

        // 绘制Node.
        protected Rect _tmpRect;
        virtual public void DrawNode(GKToyNode node, float Scale, bool drag, Vector2 mouseOffset, GKToyNode selected)
        {
            if (null == Event.current)
                return;

            switch (node.nodeType)
            {
                case NodeType.Action:
                    GUI.backgroundColor = toyMakerBase._actionColor;
                    break;
                case NodeType.Condition:
                    GUI.backgroundColor = toyMakerBase._conditionColor;
                    break;
                default:
                    GUI.backgroundColor = Color.red;
                    break;
            }

            if (null != selected && selected.id == node.id)
            {
                GUI.backgroundColor = Color.yellow;
            }

            // 如果当前为拖拽状态. 更新对象坐标.
            if (node.isMove && drag)
            {
                node.pos.x = (Event.current.mousePosition.x - mouseOffset.x) / Scale;
                node.pos.y = (Event.current.mousePosition.y - mouseOffset.y) / Scale;
            }

            // 计算Node宽高.
            int w = name.Length * toyMakerBase._charWidth + 4;
            if (w <= toyMakerBase._nodeMinWidth)
                w = toyMakerBase._nodeMinWidth;
            node.width = w;
            node.height = toyMakerBase._nodeMinHeight;

            // Right.
            _tmpRect.width = 12 * Scale;
            _tmpRect.height = (node.height - 24) * Scale;
            _tmpRect.x = (node.width + node.pos.x - 6) * Scale;
            _tmpRect.y = (node.height * 0.5f + node.pos.y) * Scale - _tmpRect.height * 0.5f;
            node.outputRect = _tmpRect;
            GUI.Button(_tmpRect, "");

            // Left.
            _tmpRect.x = (node.pos.x - 6) * Scale;
            node.inputRect = _tmpRect;
            GUI.Button(_tmpRect, "");

            // Bg.
            _tmpRect.width = node.width * Scale;
            _tmpRect.height = node.height * Scale;
            _tmpRect.x = node.pos.x * Scale;
            _tmpRect.y = node.pos.y * Scale;
            node.rect = _tmpRect;
            //判断是正在否移动对象.
            GUI.Button(_tmpRect, node.name, toyMakerBase._nodeStyle);

            GUI.backgroundColor = Color.white;
            // 批注.
            if (!string.IsNullOrEmpty(node.comment))
            {
                _tmpRect.y = _tmpRect.y + _tmpRect.height;
                string commentWord;
                int lines = GK.AutoLineFeed(node.comment, out commentWord, (int)((_tmpRect.width - toyMakerBase._commentContentMargin) / toyMakerBase._commentStyle.fontSize));
                _tmpRect.height = (lines + 1) * toyMakerBase._commentStyle.lineHeight + 6;
                GUI.Box(_tmpRect, commentWord, toyMakerBase._commentStyle);
            }

            // 绘制图标.
            float tmpSize = node.height * Scale - (float)toyMakerBase._charWidth - 8;
            _tmpRect.x += _tmpRect.width * 0.5f - tmpSize * 0.5f;
            _tmpRect.y = node.pos.y * Scale + 4;
            _tmpRect.width = tmpSize;
            _tmpRect.height = tmpSize;
            GUI.DrawTexture(_tmpRect, toyMakerBase._icons[node.iconIdx]);
        }

        //------------------------------ Inspector ------------------------------

        // 绘制Inspector.
        virtual protected void DrawInspector()
        {
            if (null == _selectNode)
                return;

            DrawInspector(_selectNode, ref _selectLink);
        }

        // 绘制详情.
        virtual public void DrawInspector(GKToyNode node, ref Link selected)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name ", GUILayout.Height(toyMakerBase._lineHeight));
                node.name = GUILayout.TextField(node.name, toyMakerBase._infoMaxLineChar, GUILayout.Height(toyMakerBase._lineHeight));
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Comment", GUILayout.Height(toyMakerBase._lineHeight));
            node.comment = GUILayout.TextArea(node.comment, GUILayout.Height(toyMakerBase._lineHeight * 5));

            if (0 != node.links.Count)
            {
                GKEditor.DrawInspectorSeperator();

                GUILayout.Label("Links");

                GUILayout.BeginVertical("Box");
                {
                    bool isFirstLink = true;
                    foreach (var l in node.links)
                    {
                        DrawNextDetail(node, ref isFirstLink, l, ref selected);
                    }
                }
                GUILayout.EndVertical();
            }
        }

        // 绘制连接点详情.
        virtual protected void DrawNextDetail(GKToyNode node, ref bool isFirst, Link l, ref Link selected)
        {
            if (!isFirst)
                GKEditor.DrawMiniInspectorSeperator();
            else
                isFirst = false;

            GUILayout.BeginHorizontal();
            {
                if (null != selected && selected == l)
                {
                    GUI.backgroundColor = Color.yellow;
                }
                if (GUILayout.Button(l.next.name))
                {
                    selected = l;
                }
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(toyMakerBase._lineHeight)))
                {
                    RemoveLink(node.id, l.next);
                }
                GUI.backgroundColor = _lastBgColor;
            }
            GUILayout.EndHorizontal();
        }
        #endregion

        #region ContentMenu
        //判断鼠标右键事件.
        protected void DrawMenu(Rect rect)
        {
            if (Event.current.type == EventType.ContextClick && rect.Contains(Event.current.mousePosition))
            {
                //Debug.Log(Event.current.mousePosition);
                GenericMenu menu = new GenericMenu();
                switch (_clickedElement)
                {
                    case ClickedElement.NoElement:
                        menu.AddItem(new GUIContent("Add Action"), false, HandleMenuAddAction, Event.current.mousePosition);
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Add Condition"), false, HandleMenuAddCondition, Event.current.mousePosition);
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Reset"), false, HandleMenuReset, Event.current.mousePosition);
                        break;
                    case ClickedElement.NodeElement:
                        break;
                    case ClickedElement.LinkElement:
                        menu.AddItem(new GUIContent(string.Format("Delete Link: {0} -> {1}", _selectNode.name, _selectLink.next.name)), false, HandleMenuDeleteLink, Event.current.mousePosition);
                        break;
                }
                menu.ShowAsContext();
                // 设置该事件被使用.
                Event.current.Use();
            }
        }

        protected void HandleMenuAddAction(object userData)
        {
            GKToyNode node = new GKToyNode();
            node.id = _curNodeIdx++;
            node.nodeType = NodeType.Action;
            node.iconIdx = 0;
            node.pos.x = (((Vector2)userData).x) / Scale;
            node.pos.y = (((Vector2)userData).y) / Scale;
            CreateNode(node);
        }

        protected void HandleMenuAddCondition(object userData)
        {
            GKToyNode node = new GKToyNode();
            node.id = _curNodeIdx++;
            node.nodeType = NodeType.Condition;
            node.iconIdx = 0;
            node.pos.x = (((Vector2)userData).x) / Scale;
            node.pos.y = (((Vector2)userData).y) / Scale;
            CreateNode(node);
        }

        protected void HandleMenuReset(object userData)
        {
            ResetNode();
        }

        protected void HandleMenuDeleteLink(object userData)
        {
            RemoveLink(_selectNode.id, _selectLink.next);
        }

        // 增加节点.
        protected void CreateNode(GKToyNode node)
        {
            _tmpSelectNode = node;
            node.iconIdx = 0;
            node.name = string.Format("{0}-{1}", node.type, node.id);
            node.comment = "";
            _overlord.data.nodeLst.Add(node);
        }

        // 删除节点.
        protected void RemoveNode(GKToyNode node)
        {
            if (_overlord.data.nodeLst.Contains(node))
            {
                _overlord.data.nodeLst.Remove(node);
            }
        }

        // 重置节点.
        protected void ResetNode()
        {
            _overlord.data.nodeLst.Clear();
            _selectNode = null;
            _selectLink = null;
        }

        // 删除链接.
        protected void RemoveLink(int id, GKToyNode node)
        {
            if (!_removeLinkLst.ContainsKey(id))
                _removeLinkLst[id] = new List<GKToyNode>();

            _removeLinkLst[id].Add(node);
            _selectLink = null;
        }
        #endregion

        #region Other
        // 初始化.
        // 初始化数据必须需要再Enable中执行.
        protected void Init()
        {
            if (null != _overlord)
            {
                ResetSelected(_overlord);
            }


            // 数据备份.
            _lastColor = GUI.color; ;
            _lastBgColor = GUI.backgroundColor;

            // 数据导入.
            toyMakerBase = Settings.toyMakerBase;

            // 数据计算.
            toyMakerBase._commentContentMargin = toyMakerBase._commentStyle.padding.left 
                                                + toyMakerBase._commentStyle.padding.right
                                                + toyMakerBase._commentStyle.margin.left 
                                                + toyMakerBase._commentStyle.margin.right
                                                + toyMakerBase._commentStyle.contentOffset.x;


            _contentView = new Rect(toyMakerBase._informationWidth + toyMakerBase._layoutSpace * 3,
                                    toyMakerBase._lineHeight  + toyMakerBase._layoutSpace,
                                    toyMakerBase._minWidth - toyMakerBase._informationWidth - toyMakerBase._layoutSpace * 4,
                                    toyMakerBase._minHeight - toyMakerBase._lineHeight - toyMakerBase._layoutSpace * 3);
            
            _nonContentRect = new Rect((toyMakerBase._minWidth - 206) * 0.5f, (toyMakerBase._minHeight - 100) * 0.5f, 206, 100);
        }

        // 序列化点选对象变更.
        protected void SelectChanged()
        {
            var assets = Selection.GetFiltered<GKToyBaseOverlord>(SelectionMode.Assets);
            if (0 == assets.Length)
                return;

            if(assets[0] != _overlord)
            {
                ResetSelected(assets[0]);
            }

            _isScale = true;
            UpdateLinks();
            Repaint();
        }

        // 重置选择.
        protected void ResetSelected(GKToyBaseOverlord target)
        {
            _overlord = target;
            _selectNode = null;
            _selectLink = null;
            Scale = 1;
            _contentScrollPos = Vector2.zero;
        }

        // 创建实例.
        virtual protected void CreateData()
        {
            GameObject go = new GameObject();
            var tmpOverload = GK.GetOrAddComponent<GKToyBaseOverlord>(go);
            _overlord = tmpOverload;
        }
        #endregion

        public enum InformationType
        {
            Detail = 0,
            Actions,
            Variables,
            Inspector
        }

		public enum ClickedElement
		{
			NoElement = 0,
			NodeElement,
			LinkElement
		};
	}
}


