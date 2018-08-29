using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GKBase;

/**
 * 游戏模块编辑器基类.
 * 各模块编辑器窗口基类.
 * 功能支持: 1) 视口窗口移动缩放; 
 *          2) Action/Condition创建及基础功能;
 *          3) 信息界面基础功能及接口;
 */
namespace GKToy
{
    public class GKToyMakerBase : EditorWindow
    {
        #region PublicField
        protected static Editor_Settings _settings;
        public static Editor_Settings Settings
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
        public static Editor_Settings.ToyMakerBase toyMakerBase = null;

        // GUI 数据备份.
        public static Color _lastColor;
        public static Color _lastBgColor;
        #endregion

        #region PrivateField

        #region Cache
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

        // 事件内容滚动条位置.
        protected Vector2 _contentScrollPos = new Vector2(0.5f, 0.5f);

        // 鼠标是否拖拽中.
        protected bool _isDrag = false;
        protected bool _isLinking = false;
		// 点击到的元素.
		protected ClickedElement _clickedElement = ClickedElement.NodeElement;
        protected bool _isScale = false;

        // 材质球.
        protected static Material _myMat = null;
        protected static Material MyMat
        {
            get
            {
                if(null == _myMat)
                {
                    Shader shader = Shader.Find("Standard");
                    _myMat = new Material(shader);
                }
                return _myMat;
            }
        }

        // 当前信息界面类型.
        protected InformationType _infoType = InformationType.Detail;
        protected ModuleType _moduleType = ModuleType.Base;
        protected string _name = "Hello";
        protected string _comment = "";
        protected bool _startWhenEnable = false;

        // Node链表.
        protected Dictionary<int, GKToyNode> _nodeLst = new Dictionary<int, GKToyNode>();
        protected GKToyNode _selectNode = null;

        // 当前Node索引. 用于产生GUID.
        protected int _curNodeIdx = 0;

		// 当前Link索引，用于产生Link的GUID.
		protected int _curLinkIdx = 0;
		// 当前选中Link的Id.
		protected Link _selectLink = null;
        // 视口区域.
        protected static Rect _contentView;
        protected Rect _contentRect = new Rect();

        // 临时变量集.
        protected Vector2 _tmpScalePos = Vector2.zero;
        // 链接变更列表.
        // !!!逻辑渲染分离, 等渲染完毕后再进行逻辑处理, 规避渲染时变更渲染内容所产生的异常.
        protected Dictionary <int, List<GKToyNode>> _newLinkLst = new Dictionary<int, List<GKToyNode>>();
        protected static Dictionary<int, List<GKToyNode>> _removeLinkLst = new Dictionary<int, List<GKToyNode>>();
		// 绘制当前拖拽连线.
		protected static List<Vector2> points = new List<Vector2>();
		// 鼠标距节点中心的偏移量.
		protected Vector2 _mouseOffset = Vector2.zero;
		#endregion

		#endregion

		#region PublicMethod
		[MenuItem("GK/ToyMaker/Toy Maker Base", false, GKEditorConfiger.MenuItemPriorityA)]
        public static void MenuItem_Window()
        {
            EditorWindow.GetWindow<GKToyMakerBase>("Logic Maker", true);
        }

        // 删除链接.
        public static void RemoveLink(int id, GKToyNode node)
        {
            if (!_removeLinkLst.ContainsKey(id))
                _removeLinkLst[id] = new List<GKToyNode>();

            _removeLinkLst[id].Add(node);
        }

		public static void DrawLine(Vector2 src, Vector2 dest)
		{
			bool vertical = false;
			var lst = ClacLinePoint(src, dest, out vertical);
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

		public static void DrawLine(Vector2 src, Vector2 dest, bool vertical)
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

		// 计算连线节点.
		public static List<Vector2> ClacLinePoint(Vector2 src, Vector2 dest, out bool vertical)
		{
			points.Clear();
			vertical = true;

			float distanceX = Mathf.Abs(src.x - dest.x);
			float distanceY = Mathf.Abs(src.y - dest.y);

			// 2 points.
			//if (distanceX < 8 || distanceY < 8)
			//{
			//    vertical = distanceX < distanceY;
			//    points.Add(src);
			//    points.Add(dest);
			//    return points;
			//}

			// 4 points.
			points.Add(src);
			Vector2 point = new Vector2(src.x + 6, src.y);
			points.Add(point);
			point = new Vector2(src.x + 6, dest.y);
			points.Add(point);
			points.Add(dest);
			return points;
		}
		#endregion

		#region PrivateMethod
		private void OnEnable()
        {
            Init();
            wantsMouseMove = true;
            minSize = new Vector2(toyMakerBase._minWidth, toyMakerBase._minHeight);
            maxSize = new Vector2(toyMakerBase._minWidth, toyMakerBase._minHeight);
            Show();
        }

        // 初始化.
        // 初始化数据必须需要再Enable中执行.
        private static void Init()
        {
            // 数据备份.
            _lastColor = GUI.color; ;
            _lastBgColor = GUI.backgroundColor;

            // 数据导入.
            toyMakerBase = Settings.toyMakerBase;

            // 数据计算.
            _contentView = new Rect(toyMakerBase._informationWidth + toyMakerBase._layoutSpace * 3,
                                    toyMakerBase._lineHeight + toyMakerBase._layoutSpace,
                                    toyMakerBase._minWidth - toyMakerBase._informationWidth - toyMakerBase._layoutSpace * 4,
                                    toyMakerBase._minHeight - toyMakerBase._lineHeight - toyMakerBase._layoutSpace * 3);
        }

        private void OnGUI()
        {
            Logic();
			Render();
		}

		private void Render()
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

        // 按键响应.
        private void Logic()
        {
            if (null == Event.current)
                return;

			// 缓存内容坐标. 防止缩放时移动.
			_isScale = Event.current.alt;
            if(_isScale)
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
					if(0 == Event.current.button)
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
                    if(null != _selectNode)
                        _selectNode.isMove = false;
					break;
            }

            UpdateLinks();
			LinkChanged();
		}

		#region Logic
        private void UpdateTouch()
        {
            if (!UpdateNodeTouch())
                return;
            UpdateLinkTouch();
        }

		// 链接线段点选检测.
		private bool UpdateLinkTouch()
		{
			if (0 == _nodeLst.Count)
				return false;

			foreach (var node in _nodeLst)
			{
				var links = node.Value.links;
				if (0 == links.Count)
					continue;

				foreach (Link link in links.Values)
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
							_selectNode = node.Value;
							_clickedElement = GKToyMakerBase.ClickedElement.LinkElement;
							// 如果是选中的连接，则跳过最后再画，否则高亮色会被遮住.
							return true;
						}
					}
				}
			}
            return false;
		}

        // 更新节点点击逻辑.
        private bool UpdateNodeTouch()
        {
            // 链接时不可点击.
            if (_isLinking)
                return false;
            
            Vector2 mousePos = Event.current.mousePosition + _contentScrollPos;

            foreach (var node in _nodeLst.Values)
            {
                if (node.inputRect.Contains(mousePos))
                {
                    _selectNode = node;
                    return true;
                }
                else if (node.outputRect.Contains(mousePos))
                {
                    _isLinking = true;
                    _selectNode = node;
                    return true;
                }
                else if (node.rect.Contains(mousePos))
                {
                    _clickedElement = ClickedElement.NodeElement;
                    _selectNode = node;
                    _selectNode.isMove = true;
                    _selectLink = null;
					_mouseOffset = mousePos - node.rect.position;
					return true;
                }
            }
            return false;
        }

        // 拖拽、缩放过程中更新链接线段.
        private void UpdateLinks()
        {
            if (_isDrag && !_isLinking)
            {
                _selectNode.UpdateAllLinks();
                foreach (GKToyNode node in _nodeLst.Values)
                {
                    int index = node.findLinkIdFromAction(_selectNode);
                    if (index >= 0)
                    {
                        node.UpdateOneLink(index);
                    }
                }
            }
            else if (_isScale)
            {
                foreach (GKToyNode node in _nodeLst.Values)
                {
                    node.UpdateAllLinks();
                }
            }
        }
		#endregion

		#region Render
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
                    DrawNode();
                    break;
                case InformationType.Variables:
                    break;
                case InformationType.Inspector:
                    DrawInspector();
                    break;
            }
        }

        // 绘制工具栏.
        private void DrawToolBar()
        {
            GUILayout.BeginHorizontal();
                {
                    if (GUILayout.Button("", EditorStyles.toolbarButton, GUILayout.Height(toyMakerBase._lineHeight)))
                    {

                    }
                    if(GUILayout.Button("Back to center", EditorStyles.toolbarButton, GUILayout.Width(120), GUILayout.Height(toyMakerBase._lineHeight)))
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
                foreach(var node in _nodeLst.Values)
                {
                    node.DrawNode(Scale, _isDrag, _mouseOffset, _selectNode);
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
            GUI.Label(new Rect(toyMakerBase._informationWidth + toyMakerBase._layoutSpace* 3 + 10, 
                               toyMakerBase._lineHeight + toyMakerBase._layoutSpace, 400, 100), 
                      string.Format("{0}-{1}", _moduleType.ToString(), _name), toyMakerBase._titleStyle);
            // 缩放比列尺绘制.
            GUI.Label(new Rect(toyMakerBase._minWidth - 140, toyMakerBase._lineHeight + toyMakerBase._layoutSpace + 10, 100, 100), 
                      string.Format("X {0:N1} ", Scale), toyMakerBase._titleStyle);
        }

        // 绘制背景网格.
        private void DrawBlackGroundGrid()
        {
            GUI.backgroundColor = toyMakerBase._bgColor;
            GUI.Box(new Rect(0, 0, toyMakerBase._maxWidth * 2 * Scale, toyMakerBase._maxHeight * 2 * Scale), "");
            GUI.backgroundColor = _lastBgColor;

            Handles.color = Color.black;
            int x1 = toyMakerBase._informationWidth + toyMakerBase._layoutSpace * 3;
            int x2 = (int)((toyMakerBase._maxWidth * 2 - toyMakerBase._layoutSpace) * Scale);
            int y1 =  toyMakerBase._layoutSpace;
            int space = 0;
            for (int i = 0; y1 + i * 25 / Scale < toyMakerBase._maxHeight * 2 * Scale - toyMakerBase._layoutSpace; i++)
            {
                space = (int)(y1 + i * 25 / Scale);

                //每3根加粗.
                if(i % 3 == 0)
                {
                    Handles.DrawLine(new Vector2(x1, y1 + space - 0.5f), new Vector3(x2, y1 + space - 0.5f));
                    Handles.DrawLine(new Vector2(x1, y1 + space + 0.5f), new Vector3(x2, y1 + space + 0.5f));
                }
                Handles.DrawLine(new Vector2(x1, y1 + space), new Vector3(x2, y1 + space));
            }

            y1 = toyMakerBase._layoutSpace;
            int y2 = toyMakerBase._maxHeight * 2 - toyMakerBase._layoutSpace;
            x1 =  toyMakerBase._layoutSpace * 3;
            for (int i = 0; x1 + i * 25 / Scale < toyMakerBase._maxWidth * 2 * Scale - toyMakerBase._layoutSpace; i++)
            {
                space = (int)(x1 + i * 25 / Scale);

                //每3根加粗.
                if (i % 3 == 0)
                {
                    Handles.DrawLine(new Vector2(x1 + space - 0.5f, y1), new Vector3(x1 + space - 0.5f, y2));
                    Handles.DrawLine(new Vector2(x1 + space + 0.5f, y1), new Vector3(x1 + space + 0.5f, y2));
                }

                Handles.DrawLine(new Vector2(x1 + space, y1), new Vector3(x1 + space, y2));
            }
        }

        //判断鼠标右键事件.
        private void DrawMenu(Rect rect)
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

        private void HandleMenuAddAction(object userData)
        {
            GKToyNode node = new GKToyNode();
            node.id = _curNodeIdx++;
            node.nodeType = NodeType.Action;
            node.iconIdx = 0;
            node.pos.x = (((Vector2)userData).x) / Scale;
            node.pos.y = (((Vector2)userData).y) / Scale;
            CreateNode(node);
        }

        private void HandleMenuAddCondition(object userData)
        {
            GKToyNode node = new GKToyNode();
            node.id = _curNodeIdx++;
            node.nodeType = NodeType.Condition;
            node.iconIdx = 0;
            node.pos.x = (((Vector2)userData).x) / Scale;
            node.pos.y = (((Vector2)userData).y) / Scale;
            CreateNode(node);
        }

        private void HandleMenuReset(object userData)
        {
            _nodeLst.Clear();
        }

		private void HandleMenuDeleteLink(object userData)
		{
			RemoveLink(_selectNode.id, _selectLink.next);
		}

		// 渲染链接线段.
		private void DrawLinks()
        {
            if (null == Event.current || null == _selectNode)
                return;
			
			Handles.color = Color.white;
			// Draw current link line.
			if (_isLinking && null != _selectNode)
			{
				_selectNode.DrawCurrentLink();
				Repaint();
			}

			// Draw links.
			foreach (var node in _nodeLst.Values)
            {
				node.DrawLinks(_selectLink);
            }

			// 绘制高亮连接.
			if (null != _selectLink)
			{
				Handles.color = Color.yellow;
				for (int i = 0; i < _selectLink.points.Count - 1; ++i)
				{
					DrawLine(_selectLink.points[i], _selectLink.points[i + 1], 0 == (i & 1) ^ _selectLink.isFirstVertical);
				}
				Handles.color = Color.white;
			}
		}

        // 检测是否链接。
        private void CheckLink()
        {
            if (null == Event.current && null == _selectNode)
                return;
			Vector2 mousePos = Event.current.mousePosition + _contentScrollPos;
            foreach (var node in _nodeLst.Values)
            {
                if(node.id == _selectNode.id)
                    continue;

                if(node.inputRect.Contains(mousePos) || node.rect.Contains(mousePos) && _selectNode.findLinkIdFromAction(node) < 0)
                {
                    if (!_newLinkLst.ContainsKey(_selectNode.id))
                        _newLinkLst[_selectNode.id] = new List<GKToyNode>();

                    _newLinkLst[_selectNode.id].Add(node);
                    return;
                }
            }
        }

        // 绑定新链接.
        protected void LinkChanged()
        {
            if (0 != _newLinkLst.Count)
            {
                foreach (var link in _newLinkLst)
                {
                    foreach (var l in link.Value)
                    {
                        _nodeLst[link.Key].AddLink(_curLinkIdx++, l);
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
                        _nodeLst[link.Key].RemoveLink(l);
                    }
                }
                _removeLinkLst.Clear();
            }
        }
        #endregion

        #region virtual
        // 绘制简介.
        virtual protected void DrawDetail()
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name ", GUILayout.Height(toyMakerBase._lineHeight));
                _name = GUILayout.TextField(_name, toyMakerBase._infoMaxLineChar, GUILayout.Height(toyMakerBase._lineHeight));
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("Comment", GUILayout.Height(toyMakerBase._lineHeight));
            _comment = GUILayout.TextArea(_comment, GUILayout.Height(toyMakerBase._lineHeight * 5));
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Start When Enable ", GUILayout.Height(toyMakerBase._lineHeight));
                _startWhenEnable = GUILayout.Toggle(_startWhenEnable, "", GUILayout.Height(toyMakerBase._lineHeight));
            }
            GUILayout.EndHorizontal();
        }

        // 绘制节点列表.
        virtual protected void DrawNode()
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

        // 绘制Inspector.
        virtual protected void DrawInspector()
        {
            if (null == _selectNode)
                return;

            _selectNode.DrawInspector(ref _selectLink);
        }

        // 增加节点.
        virtual protected void CreateNode(GKToyNode node)
        {
            _selectNode = node;
            node.iconIdx = 0;
            node.name = string.Format("{0}-{1}", node.type, node.id);
            node.comment = "";
            _nodeLst.Add(node.id, node);
        }

        // 删除节点.
        virtual protected void RemoveNode(GKToyNode node)
        {
            if (_nodeLst.ContainsKey(node.id))
            {
                _nodeLst.Remove(node.id);
            }
        }

        // 重置节点.
        virtual protected void ResetNode()
        {
            _nodeLst.Clear();
            _selectNode = null;
        }
        #endregion

		#endregion

		public enum InformationType
        {
            Detail = 0,
            Actions,
            Variables,
            Inspector
        }

        public enum ModuleType
        {
            Base = 0,
            Tutorial, 
        }

        public enum NodeType
        {
            Node = 0,
            Action,
            Condition,
        }

		public enum ClickedElement
		{
			NoElement = 0,
			NodeElement,
			LinkElement
		};
	}
}


