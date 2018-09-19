using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using GKBase;
using System;
using System.Reflection;
using System.Linq;

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
        protected List<Type> _variableType = new List<Type>();
        protected string[] _variableTypeNames;
        #endregion

        #region PrivateField
        // 节点内容缩放因子.
        protected float _contentSacle = 1;
        protected float Scale
        {
            get { return _contentSacle; }
            set
            {
                if (toyMakerBase._minScale > value)
                {
                    _contentSacle = toyMakerBase._minScale;
                    return;
                }
                if (toyMakerBase._maxScale < value)
                {
                    _contentSacle = toyMakerBase._maxScale;
                    return;
                }
                _contentSacle = value;
            }
        }
        // 当前Node索引. 用于产生GUID.
        protected int _curNodeIdx = 0;
        protected int curNodeIdx
        {
            get { return _curNodeIdx; }
            set
            {
                if (null != _overlord)
                {
                    _overlord.data.nodeGuid = value;
                }
                _curNodeIdx = value;
            }
        }
        // 当前Link索引，用于产生Link的GUID.
        protected int _curLinkIdx = 0;
        protected int curLinkIdx
        {
            get { return _curLinkIdx; }
            set
            {
                if (null != _overlord)
                {
                    _overlord.data.linkGuid = value;
                }
                _curLinkIdx = value;
            }
        }
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
		// !!!逻辑渲染分离, 等渲染完毕后再进行逻辑处理, 规避渲染时变更渲染内容所产生的异常.
		// 链接变更列表.
		protected Dictionary<int, List<GKToyNode>> _newLinkLst = new Dictionary<int, List<GKToyNode>>();
        protected Dictionary<int, List<GKToyNode>> _removeLinkLst = new Dictionary<int, List<GKToyNode>>();
		// 节点变更列表.
		protected Dictionary<int, object> _newNodeLst = new Dictionary<int, object>();
		protected List<int> _removeNodeLst = new List<int>();
		// GUI 数据备份.
		protected Color _lastColor;
        protected Color _lastBgColor;
        // 节点种类树根节点.
        private static TreeNode root = null;
        // 临时变量编辑缓存.
        protected string _newVariableName = "";
        protected int _newVariableIdx = 0;
        protected Dictionary<string, List<object>> _addVariableLst = new Dictionary<string, List<object>>();
        protected Dictionary<string, List<object>> _delVariableLst = new Dictionary<string, List<object>>();
		// 是否锁定中.
		private bool isLocking = false;
        #endregion

        #region PublicMethod
        [MenuItem("GK/ToyMaker/Toy Maker Base", false, GKEditorConfiger.MenuItemPriorityA)]
        public static void MenuItem_Window()
        {
            instance = GetWindow<GKToyMakerBase>("Logic Maker", false);
        }
        #endregion

        private void OnEnable()
        {
			if(null == instance)
			{
				instance = this;
				Init();
				wantsMouseMove = true;
				minSize = new Vector2(toyMakerBase._minWidth, toyMakerBase._minHeight);
				maxSize = new Vector2(toyMakerBase._minWidth, toyMakerBase._minHeight);
				instance.Show();
			}
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

        private void OnDestroy()
        {
            _overlord.data.SaveNodes();
            instance = null;
        }

        private void Render()
        {
            if (null == _overlord)
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
			if (null != _tmpSelectNode)
			{
				GUI.FocusControl(null);
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
#if UNITY_2017_1_OR_NEWER
            if (Event.current.alt && Event.current.isScrollWheel)
#else
            if (Event.current.alt)
#endif
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
                        UpdateTouch();
                    }
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
			if (!_contentRect.Contains(Event.current.mousePosition))
				return false;
			Vector2 mousePos = Event.current.mousePosition + _contentScrollPos;
			foreach (GKToyNode node in _overlord.data.nodeLst.Values)
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
                    Debug.Log("2131231");
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
            foreach (GKToyNode node in _overlord.data.nodeLst.Values)
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
            foreach (GKToyNode node in _overlord.data.nodeLst.Values)
            {
                if (node.id == _selectNode.id)
                    continue;

                if (node.inputRect.Contains(mousePos) || node.rect.Contains(mousePos) && null == _selectNode.FindLinkFromNode(node.id))
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
            GUILayout.BeginVertical("Box", GUILayout.Width(toyMakerBase._informationWidth), GUILayout.ExpandHeight(true));
            {
                // 绘制Tab.
                GUILayout.BeginHorizontal();
                {
                    foreach (var type in GK.EnumValues<InformationType>())
                    {
                        if (GUILayout.Toggle(type == _infoType, type.ToString(), EditorStyles.toolbarButton, GUILayout.Height(toyMakerBase._lineHeight)))
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
                    DrawVariables();
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
					_contentScrollPos = Vector2.zero;
				}
				isLocking = GUILayout.Toggle(isLocking, "Lock", EditorStyles.toolbarButton, GUILayout.Width(120), GUILayout.Height(toyMakerBase._lineHeight));
                if (GUILayout.Button("Export", EditorStyles.toolbarButton, GUILayout.Width(120), GUILayout.Height(toyMakerBase._lineHeight)))
                {

                }
                if (GUILayout.Button("Take screenshot", EditorStyles.toolbarButton, GUILayout.Width(160), GUILayout.Height(toyMakerBase._lineHeight)))
                {
					Texture2D texture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
					texture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0, false);
					GK.SaveTextureToFile(texture, String.Format("{0}/Screenshot-{1:yyMMddHHmmssfff}.png", Application.dataPath, DateTime.Now));
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
                foreach (GKToyNode node in _overlord.data.nodeLst.Values)
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
            // 变量数据更新.
            if (0 != _addVariableLst.Count)
            {
                _overlord.data.variableChanged = true;
                foreach (var v in _addVariableLst)
                {
                    foreach (var obj in v.Value)
                    {
                        if (_overlord.data.variableLst.ContainsKey(v.Key))
                        {
                            _overlord.data.variableLst[v.Key].Add(obj);
                        }
                        else
                        {
                            List<object> lst = new List<object>();
                            lst.Add(obj);
                            _overlord.data.variableLst.Add(v.Key, lst);
                        }
                    }
                }
                _addVariableLst.Clear();
            }

            if (0 != _delVariableLst.Count)
            {
                _overlord.data.variableChanged = true;
                foreach (var v in _delVariableLst)
                {
                    foreach (var obj in v.Value)
                    {
                        _overlord.data.RemoveVariable(v.Key, obj);
                    }
                }
                _delVariableLst.Clear();
            }

            if (_overlord.data.variableChanged)
            {
                _overlord.data.variableChanged = false;
                _overlord.data.SaveVariable();
            }

            // 链接数据更新.
            if (0 != _newLinkLst.Count)
            {
                foreach (var link in _newLinkLst)
                {
                    foreach (var l in link.Value)
                    {
                        GKToyNode n = (GKToyNode)_overlord.data.nodeLst[link.Key];
                        if (null != n)
                            n.AddLink(curLinkIdx++, l);
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
                        GKToyNode n = (GKToyNode)_overlord.data.nodeLst[link.Key];
                        if (null != n)
                            n.RemoveLink(l.id);
                    }
                }
                _removeLinkLst.Clear();
            }

			// 节点数据更新.
			if (0 != _newNodeLst.Count)
			{
				foreach (var node in _newNodeLst)
				{
					_overlord.data.nodeLst.Add(node.Key, node.Value);

				}
				_newNodeLst.Clear();
			}

			if (0 != _removeNodeLst.Count)
			{
				foreach (var id in _removeNodeLst)
				{
					_overlord.data.nodeLst.Remove(id);
				}
				_removeNodeLst.Clear();
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
                foreach (Link l in _selectNode.links)
                {
                    _selectNode.UpdateLink(l, (GKToyNode)_overlord.data.nodeLst[l.next]);
                }
                foreach (GKToyNode node in _overlord.data.nodeLst.Values)
                {
                    Link l = node.FindLinkFromNode(_selectNode.id);
                    if (null != l)
                        node.UpdateLink(l, (GKToyNode)_overlord.data.nodeLst[l.next]);
                }
            }
            else if (_isScale)
            {
                foreach (GKToyNode node in _overlord.data.nodeLst.Values)
                {
                    foreach (Link l in node.links)
                    {
                        node.UpdateLink(l, (GKToyNode)_overlord.data.nodeLst[l.next]);
                    }
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
            if (root != null)
            {
                GUILayout.BeginVertical();
                DrawTypeTree(root, 0, 0);
                GUILayout.EndVertical();
            }
        }
        private void DrawTypeTree(TreeNode node, int level, int treeIndex)
        {
            if (node == null)
            {
                return;
            }
            if (level != 0)
            {
                treeIndex++;
                if (node.nodeType == TreeNode.TreeNodeType.Switch)
                {
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(10 * (level - 1));
#if UNITY_2017_1_OR_NEWER
                    node.isOpen = EditorGUILayout.Foldout(node.isOpen, node.name, true);
#else
                    node.isOpen = EditorGUILayout.Foldout(node.isOpen, node.name);
#endif
                    EditorGUILayout.EndHorizontal();
				}
				else
				{
					if (GUILayout.Button(node.name))
					{
						Type t = typeof(GKToyNode);
						GKToyNode newNode = t.Assembly.CreateInstance(node.key, true, System.Reflection.BindingFlags.Default, null, new object[] { curNodeIdx }, null, null) as GKToyNode;
						newNode.className = node.key;
						newNode.pos.x = (_contentScrollPos.x + toyMakerBase._minWidth * 0.5f) / Scale;
						newNode.pos.y = (_contentScrollPos.y + toyMakerBase._minHeight * 0.5f) / Scale;
						CreateNode(newNode);
						//Debug.Log("GKToy."+node.key);
					}
				}
			}
			if (node == null || !node.isOpen || node.children == null)
			{
				return;
			}
			for (int i = 0; i < node.children.Count; i++)
			{
				DrawTypeTree(node.children[i], level + 1, treeIndex);
			}
		}
#endregion

#region Variables
        // 绘制节点列表.
        virtual protected void DrawVariables()
        {
            if (null == _variableTypeNames)
                return;

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name", GUILayout.Width(40));
                _newVariableName = GUILayout.TextField(_newVariableName, 12);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Type", GUILayout.Width(40));
                _newVariableIdx = EditorGUILayout.Popup(_newVariableIdx, _variableTypeNames);
                if(GUILayout.Button("Add", GUILayout.Width(40), GUILayout.Height(14)))
                {
                    if (string.IsNullOrEmpty(_newVariableName))
                    {
                        EditorUtility.DisplayDialog("Tip", "Variables can not be empty.", "OK");
                    }
                    else if(!IsExistVariable(_newVariableName))
                    {
                        string strType = "GKToy." + _variableTypeNames[_newVariableIdx];
                        Type t = typeof(GKToyVariable);
                        GKToyVariable v = (GKToyVariable)t.Assembly.CreateInstance(strType);
                        v.Name = _newVariableName;
                        v.InitializePropertyMapping(_overlord);
                        if(_addVariableLst.ContainsKey(v.PropertyMapping))
                        {
                            _addVariableLst[v.PropertyMapping].Add(v);
                        }
                        else
                        {
                            List<object> lst = new List<object>();
                            lst.Add(v);
                            _addVariableLst.Add(v.PropertyMapping, lst);
                        }
                        _newVariableName = "";
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Tip", "Rename the variables.", "OK");
                    }
                }
            }
            GUILayout.EndHorizontal();

            if(0 != _overlord.data.variableLst.Count)
            {
                foreach(var vl in _overlord.data.variableLst)
                {
                    GUILayout.BeginVertical("Box");
                    {
                        for (int i = 0; i < vl.Value.Count; i++)
                        {
                            if (0 != i)
                                GKEditor.DrawMiniInspectorSeperator();

                            GKToyVariable v = (GKToyVariable)vl.Value[i];

                            GUILayout.BeginHorizontal();
                            {
                                GUILayout.BeginVertical();
                                {
                                    GUILayout.Label(v.Name);
                                    GKEditor.DrawBaseControl(true, v.GetValue(), (obj) => { v.SetValue(obj); });
                                }
                                GUILayout.EndVertical();

                                GUI.backgroundColor = Color.red;
                                if (GUILayout.Button("X", GUILayout.Width(30), GUILayout.Height(30)))
                                {
                                    if (_delVariableLst.ContainsKey(vl.Key))
                                    {
                                        _delVariableLst[vl.Key].Add(vl.Value[i]);
                                    }
                                    else
                                    {
                                        List<object> lst = new List<object>();
                                        lst.Add(vl.Value[i]);
                                        _delVariableLst.Add(vl.Key, lst);
                                    }

                                }
                                GUI.backgroundColor = _lastBgColor;
                            }
                            GUILayout.EndHorizontal();
                        }
                    }
                    GUILayout.EndVertical();  
                }
            }
        }

        // 检测变量是否重名.
        protected bool IsExistVariable(string key)
        {
            if (null == _overlord || 0 == _overlord.data.variableLst.Count)
                return false;

            foreach (var v in _overlord.data.variableLst.Values)
            {
                foreach(var n in v)
                {
                    if (((GKToyVariable)n).Name.Equals(key))
                        return true;
                }
            }
            return false;
        }
#endregion

#region Content

        //------------------------------ Link ------------------------------

        // 渲染链接线段.
        private void DrawLinks()
        {
            if (null == Event.current)
                return;

            Handles.color = Color.black;
            // Draw current link line.
            if (_isLinking && null != _selectNode)
            {
                DrawCurrentLink(_selectNode);
                Repaint();
            }

			// Draw links.
			foreach (GKToyNode node in _overlord.data.nodeLst.Values)
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
				case NodeType.Decoration:
					GUI.backgroundColor = toyMakerBase._decorationColor;
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
            GUI.Label(_tmpRect, node.name, toyMakerBase._nodeStyle);

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
			if (node.icon != null)
			{
				float tmpSize = node.height * Scale - (float)toyMakerBase._charWidth - 8;
				_tmpRect.x += _tmpRect.width * 0.5f - tmpSize * 0.5f;
				_tmpRect.y = node.pos.y * Scale + 4;
				_tmpRect.width = tmpSize;
				_tmpRect.height = tmpSize;
				GUI.DrawTexture(_tmpRect, node.icon);
			}
			// 绘制状态标志.
			if (node.state != NodeState.Inactive)
			{
				float tmpSize2 = node.height * Scale * 0.4f;
				_tmpRect.x = node.pos.x * Scale + 4;
				_tmpRect.y = node.pos.y * Scale + 4;
				_tmpRect.width = tmpSize2;
				_tmpRect.height = tmpSize2;
				switch (node.state)
				{
					case NodeState.Activated:
						GUI.DrawTexture(_tmpRect, toyMakerBase._activatedMark);
						break;
					case NodeState.Success:
						GUI.DrawTexture(_tmpRect, toyMakerBase._successMark);
						break;
					case NodeState.Fail:
						GUI.DrawTexture(_tmpRect, toyMakerBase._failMark);
						break;
				}
			}
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

			// 绘制节点属性.
            if (null != node.props && 0 != node.props.Length)
			{
				GKEditor.DrawInspectorSeperator();
				GUILayout.Label("Action Variables");
                for (int i = 0; i < node.props.Length; i++)
                {
                    var prop = node.props[i];
                    GUILayout.BeginVertical();
                    {
                        GUILayout.Label(prop.Name);

                        GUILayout.BeginHorizontal();
                        {
                            var v = prop.GetValue(node, null);
                            if(-1 != node.propStates[i])
                            {
                                var lst = _overlord.GetVariableNameListByType(v);
                                node.propStates[i] = EditorGUILayout.Popup(node.propStates[i], lst.ToArray());
                                GUI.backgroundColor = Color.gray;
                            }
                            else
                            {
                                
                                // 处理自定义变量.
                                if (v is GKToyVariable)
                                {
                                    GKEditor.DrawBaseControl(true, ((GKToyVariable)v).GetValue(), (obj) => { ((GKToyVariable)v).SetValue(obj); });
                                }
                                else
                                {
                                    GKEditor.DrawBaseControl(true, v, (obj) => { prop.SetValue(node, obj, null); });
                                }
                            }
                            if (GUILayout.Button("●", GUILayout.Width(18), GUILayout.Height(16)))
                            {
                                node.propStates[i] = ((-1 == node.propStates[i]) ? 0 : -1);
                            }
                            GUI.backgroundColor = _lastBgColor;
                        }
                        GUILayout.EndHorizontal();
                    }
                    GUILayout.EndVertical();
                }
			}
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
                if (GUILayout.Button(((GKToyNode)_overlord.data.nodeLst[l.next]).name))
                {
                    selected = l;
                }
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(toyMakerBase._lineHeight)))
                {
                    RemoveLink(node.id, (GKToyNode)_overlord.data.nodeLst[l.next]);
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
                GenericMenu menu = new GenericMenu();
                var _tSelectNode = (null == _tmpSelectNode) ? _selectNode : _tmpSelectNode;
                switch (_clickedElement)
                {
                    case ClickedElement.NoElement:
						foreach (var item in GKToyMakerTypeManager.Instance().typeAttributeDict.Where(x => x.Value.isVisible))
						{
							menu.AddItem(new GUIContent(item.Value.treePath), false, HandleMenuAddNode, new object[]{ Event.current.mousePosition, item.Key });
						}
						menu.AddSeparator("");
						menu.AddItem(new GUIContent("Reset"), false, HandleMenuReset, Event.current.mousePosition);
						break;
                    case ClickedElement.NodeElement:
						// 不可自建的节点不可删除.
						if (GKToyMakerTypeManager.Instance().typeAttributeDict[_tSelectNode.className].isVisible)
							menu.AddItem(new GUIContent(string.Format("Delete Node: {0}", _tSelectNode.name)), false, HandleMenuDeleteNode);
						break;
                    case ClickedElement.LinkElement:
                        menu.AddItem(new GUIContent(string.Format("Delete Link: {0} -> {1}", _tSelectNode.name, ((GKToyNode)_overlord.data.nodeLst[_selectLink.next]).name)), false, HandleMenuDeleteLink);
                        break;
                }
                menu.ShowAsContext();
                // 设置该事件被使用.
                Event.current.Use();
            }
        }

		protected void HandleMenuAddNode(object userData)
        {
			Vector2 mousePos = (Vector2)((object[])userData)[0];
			string key = (string)((object[])userData)[1];
			Type t = typeof(GKToyNode);
			GKToyNode node = t.Assembly.CreateInstance(key, true, System.Reflection.BindingFlags.Default, null, new object[] { curNodeIdx}, null, null) as GKToyNode;
			node.className = key;
            node.pos.x = (mousePos.x) / Scale;
            node.pos.y = (mousePos.y) / Scale;
            CreateNode(node);
        }

        protected void HandleMenuReset(object userData)
        {
            ResetNode();
        }

        protected void HandleMenuDeleteLink()
        {
            RemoveLink(_selectNode.id, (GKToyNode)_overlord.data.nodeLst[_selectLink.next]);
        }

		protected void HandleMenuDeleteNode()
		{
			RemoveNode(_selectNode);
		}

		// 增加节点.
		protected void CreateNode(GKToyNode node)
        {
			node.id = curNodeIdx++;
			string[] paths = GKToyMakerTypeManager.Instance().typeAttributeDict[node.className].treePath.Split('/');
			if (paths.Length > 0)
			{
				switch (paths[0])
				{
					case "Action":
						node.nodeType = NodeType.Action;
						break;
					case "Condition":
						node.nodeType = NodeType.Condition;
						break;
					case "Decoration":
						node.nodeType = NodeType.Decoration;
						break;
				}
				node.name = string.Format("{0}-{1}", paths[paths.Length - 1], node.id);
			}
			else
			{
				Debug.LogError("Incorrect node path:" + node.className);
			}
            node.Init(_overlord);
			// 图标被序列化后，在场景重载时无法加载，重载节点图标.
			UpdateNodeIcon(node);
			node.comment = "";
			_newNodeLst.Add(node.id, node);
			_tmpSelectNode = node;
		}
		// 加载节点图标.
		protected void UpdateNodeIcon(GKToyNode node)
		{
			string[] paths = GKToyMakerTypeManager.Instance().typeAttributeDict[node.className].treePath.Split('/');
			string iconPath = GKToyMakerTypeManager.Instance().typeAttributeDict[node.className].iconPath;
			if (paths.Length > 0)
			{
				switch (paths[0])
				{
					case "Action":
						if (iconPath != "")
							node.icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
						else
							node.icon = toyMakerBase._actionIcon;
						break;
					case "Condition":
						if (iconPath != "")
							node.icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
						else
							node.icon = toyMakerBase._conditionIcon;
						break;
					case "Decoration":
						if (iconPath != "")
							node.icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
						else
							node.icon = toyMakerBase._decorationIcon;
						break;
					default:
						if (iconPath != "")
							node.icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
						else
							node.icon = toyMakerBase._defaultIcon;
						break;
				}
			}
		}

        // 删除节点.
        protected void RemoveNode(GKToyNode node)
        {
            if (_overlord.data.nodeLst.ContainsValue(node))
            {
				foreach (GKToyNode n in _overlord.data.nodeLst.Values)
				{
					Link l = n.FindLinkFromNode(node.id);
					if (l != null)
						RemoveLink(n.id, (GKToyNode)_overlord.data.nodeLst[l.next]);
				}
				if(!_removeNodeLst.Contains(node.id))
					_removeNodeLst.Add(node.id);
            }
			_selectNode = null;
        }

        // 重置节点.
        protected void ResetNode()
        {
            _overlord.data.nodeLst.Clear();
            _selectNode = null;
            _selectLink = null;
			GKToyNode node = new GKToyActionStart(curNodeIdx);
			node.className = "GKToy." + node.GetType().Name;
			node.pos.x = (_contentScrollPos.x + toyMakerBase._minWidth * 0.5f) / Scale;
			node.pos.y = (_contentScrollPos.y + toyMakerBase._minHeight * 0.5f) / Scale;
			CreateNode(node);
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
				_overlord = null;
                ResetSelected(_overlord);
			}
#if UNITY_2017_2_OR_NEWER
			EditorApplication.playModeStateChanged += SaveDataWhenPlayModeChange;
#else
			EditorApplication.playmodeStateChanged += SaveDataWhenPlayModeChange;
#endif
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

			// 子类树生成.
			root = TreeNode.Get().GenerateFileTree(GKToyMakerTypeManager.Instance().typeAttributeDict);

            // 初始化变量类型.
            _variableType.Clear();
            foreach(var t in typeof(GKToyVariable).Assembly.GetTypes())
            {
                if(t.IsSubclassOf(typeof(GKToyVariable)))
                {
                    _variableType.Add(t);
                }
            }
            _variableType.RemoveAt(0);
            _variableTypeNames = GK.TypesToString(_variableType.ToArray());
            for (int i = 0; i < _variableTypeNames.Length; i++)
            {
                _variableTypeNames[i] = _variableTypeNames[i].Replace("GKToy.", "");
            }
		}

		// 序列化点选对象变更.
		protected void SelectChanged()
		{
			if (!isLocking)
			{ 
				var assets = Selection.GetFiltered(typeof(GKToyBaseOverlord), SelectionMode.Assets);
				if (0 == assets.Length)
					return;

				if (null == _overlord)
					ResetSelected((GKToyBaseOverlord)assets[0]);

				if (assets[0] != _overlord)
				{
					ResetSelected((GKToyBaseOverlord)assets[0]);
				}

				_isScale = true;
				UpdateLinks();
				Repaint();
			}
        }

        // 重置选择.
        protected void ResetSelected(GKToyBaseOverlord target)
        {
			if (null != _overlord)
			{
				_overlord.data.SaveNodes();
			}
			_overlord = target;
            _selectNode = null;
            _selectLink = null;
            Scale = 1;
            _contentScrollPos = Vector2.zero;
            if(null != _overlord)
            {
                curNodeIdx = _overlord.data.nodeGuid;
                curLinkIdx = _overlord.data.linkGuid;
                if(!Application.isPlaying)
                {
                    _overlord.data.Init(_overlord);
                    foreach (GKToyNode node in _overlord.data.nodeLst.Values)
                    {
                        UpdateNodeIcon(node);
                    }
                }
            }
        }

        // 创建实例.
        virtual protected void CreateData()
        {
            GameObject go = new GameObject();
            var tmpOverload = GK.GetOrAddComponent<GKToyBaseOverlord>(go);
			Selection.activeGameObject = go;
			ResetSelected(tmpOverload);
			GKToyNode node = new GKToyActionStart(curNodeIdx);
			node.className = "GKToy." + node.GetType().Name;
			node.pos.x = (_contentScrollPos.x + toyMakerBase._minWidth * 0.5f) / Scale;
			node.pos.y = (_contentScrollPos.y + toyMakerBase._minHeight * 0.5f) / Scale;
			CreateNode(node);
        }
#if UNITY_2017_2_OR_NEWER
		private void SaveDataWhenPlayModeChange(PlayModeStateChange state)
		{
			if (state == PlayModeStateChange.ExitingEditMode)
			{
				if (_overlord != null)
				{
					_overlord.data.SaveNodes();
				}
			}
			else if (state == PlayModeStateChange.EnteredPlayMode || state == PlayModeStateChange.EnteredEditMode)
			{
				instance = this;
				var assets = Selection.GetFiltered(typeof(GKToyBaseOverlord), SelectionMode.Assets);
				if (0 == assets.Length)
					return;
				else
					ResetSelected((GKToyBaseOverlord)assets[0]);
			}
		}
#else
		private void SaveDataWhenPlayModeChange()
		{
			if (!EditorApplication.isPlaying && _overlord != null)
			{
				_overlord.data.SaveNodes();
			}
			else
			{
				instance = this;
				var assets = Selection.GetFiltered(typeof(GKToyBaseOverlord), SelectionMode.Assets);
				if (0 == assets.Length)
					return;
				else
					ResetSelected((GKToyBaseOverlord)assets[0]);
			}
		}
#endif
#endregion

        public enum InformationType
        {
            Detail = 0,
            Actions,
            Variables,
            Inspector,
        }

		public enum ClickedElement
		{
			NoElement = 0,
			NodeElement,
			LinkElement
		};
	}
}


