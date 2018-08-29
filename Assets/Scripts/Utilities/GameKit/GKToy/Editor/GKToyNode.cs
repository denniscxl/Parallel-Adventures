using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GKBase;
using System.Linq;

namespace GKToy
{
    public class GKToyNode
    {
        public int id;
        public int iconIdx;
        public GKToyMakerBase.NodeType nodeType = GKToyMakerBase.NodeType.Node;
        public Vector2 pos;
        public int width;
        public int height;
        public GKToyMakerBase.ModuleType type;
        public string name;
        public string comment;
        public Rect rect;
        public Rect inputRect;
        public Rect outputRect;
        public bool isMove = false;
		public Dictionary<int, Link> links = new Dictionary<int, Link>();

		#region Links Management
		/// <summary>
		/// 添加连线
		/// </summary>
		/// <param name="linkId">连线GUID</param>
		/// <param name="nextNode">连接到的节点</param>
		public void AddLink(int linkId, GKToyNode nextNode)
		{
			bool vertical = false;
			Vector2 src = new Vector2(outputRect.x + outputRect.width, outputRect.y + outputRect.height * 0.5f);
			Vector2 dest = new Vector2(nextNode.inputRect.x, nextNode.inputRect.y + nextNode.inputRect.height * 0.5f);
			links.Add(linkId, new Link(linkId, GKToyMakerBase.ClacLinePoint(src, dest, out vertical), vertical, nextNode));
		}
		public void RemoveLink(GKToyNode removeNode)
		{
			int linkId = findLinkIdFromAction(removeNode);
			if (linkId >= 0)
			{
				links.Remove(linkId);
			}
			else
			{
				Debug.LogError(string.Format("RemoveLink fail, linkId:", linkId));
			}
		}
		/// <summary>
		/// 返回连接某个节点的连接Id
		/// </summary>
		/// <param name="node">被连接的节点</param>
		/// <returns>连接Id</returns>
		public int findLinkIdFromAction(GKToyNode node)
		{
			var res = links.Where(x => x.Value.next == node).FirstOrDefault();
			if (!default(KeyValuePair<int, Link>).Equals(res))
			{
				return res.Key;
			}
			return -1;
		}
		#endregion

		#region Draw links
		
        public void DrawCurrentLink()
        {
            float x = outputRect.x + outputRect.width;
            float y = outputRect.y + outputRect.height * 0.5f;
			GKToyMakerBase.DrawLine(new Vector2(x, y), Event.current.mousePosition);
        }
		// 绘制所有链接.
		public void DrawLinks(Link selectLink)
        {
            if (0 == links.Count)
                return;

            foreach (Link link in links.Values)
            {
				// 高连线段最后绘制.
				if (null != selectLink && link.id == selectLink.id)
					continue;

				for (int i = 0; i < link.points.Count - 1; ++i)
				{
					bool isVertical = 0 == (i & 1) ^ link.isFirstVertical;
					GKToyMakerBase.DrawLine(link.points[i], link.points[i + 1], isVertical);
				}
			}
		}
        
		/// <summary>
		/// 鼠标拖拽本节点时，更新所有连线的坐标
		/// </summary>
		public void UpdateAllLinks()
		{
			Vector2 src = new Vector2(outputRect.x + outputRect.width, outputRect.y + outputRect.height * 0.5f);
			Vector2 dest;
			bool vertical;
			foreach (Link link in links.Values)
			{
				vertical = false;
				dest = new Vector2(link.next.inputRect.x, link.next.inputRect.y + link.next.inputRect.height * 0.5f);
				link.points = new List<Vector2>(GKToyMakerBase.ClacLinePoint(src, dest, out vertical));
				link.isFirstVertical = vertical;
			}
		}
		/// <summary>
		/// 更新单根连线
		/// </summary>
		/// <param name="linkId">要更新连线的Id</param>
		public void UpdateOneLink(int linkId)
		{
			Vector2 src = new Vector2(outputRect.x + outputRect.width, outputRect.y + outputRect.height * 0.5f);
			Vector2 dest = new Vector2(links[linkId].next.inputRect.x, links[linkId].next.inputRect.y + links[linkId].next.inputRect.height * 0.5f);
			bool vertical = false;
			links[linkId].points = new List<Vector2>(GKToyMakerBase.ClacLinePoint(src, dest, out vertical));
			links[linkId].isFirstVertical = vertical;
		}
		#endregion

        #region virtual
        // 绘制Node.
        protected Rect _tmpRect;

        virtual public void DrawNode(float Scale, bool drag, Vector2 mouseOffset, GKToyNode selected)
        {
            if (null == Event.current)
                return;

            var settings = GKToyMakerBase.Settings.toyMakerBase;

            switch (nodeType)
            {
                case GKToyMakerBase.NodeType.Action:
                    GUI.backgroundColor = settings._actionColor;
                    break;
                case GKToyMakerBase.NodeType.Condition:
                    GUI.backgroundColor = settings._conditionColor;
                    break;
                default:
                    GUI.backgroundColor = Color.red;
                    break;
            } 

            if (selected == this)
            {
                GUI.backgroundColor = Color.yellow;
            }

            // 如果当前为拖拽状态. 更新对象坐标.
            if (isMove && drag)
            {
                pos.x = (Event.current.mousePosition.x - mouseOffset.x) / Scale;
                pos.y = (Event.current.mousePosition.y - mouseOffset.y) / Scale;
            }

            // 计算Node宽高.
            int w = name.Length * settings._charWidth + 4;
            if (w <= settings._nodeMinWidth)
                w = settings._nodeMinWidth;
            width = w;
            height = settings._nodeMinHeight;

            // Right.
            _tmpRect.width = 12 * Scale;
            _tmpRect.height = (height - 24) * Scale;
            _tmpRect.x = (width + pos.x - 6) * Scale;
            _tmpRect.y = (height * 0.5f + pos.y) * Scale - _tmpRect.height * 0.5f;
            outputRect = _tmpRect;
            GUI.Button(_tmpRect, "");

            // Left.
            _tmpRect.x = (pos.x - 6) * Scale;
            inputRect = _tmpRect;
            GUI.Button(_tmpRect, "");

            // Bg.
            _tmpRect.width = width * Scale;
            _tmpRect.height = height * Scale;
            _tmpRect.x = pos.x * Scale;
            _tmpRect.y = pos.y * Scale;
            rect = _tmpRect;
            //判断是正在否移动对象.
            GUI.Button(_tmpRect, name, settings._nodeStyle);

            GUI.backgroundColor = Color.white;
            // 批注.
            if (!string.IsNullOrEmpty(comment))
            {
                _tmpRect.y = _tmpRect.y + _tmpRect.height;
                int lines = (comment.Length * settings._commentCharWidth - 4) / (int)_tmpRect.width;
                _tmpRect.height = (lines + 1) * settings._commentCharHeight + 6;
                GUI.Box(_tmpRect, comment, settings._commentStyle);
            }

            // 绘制图标.
            float tmpSize = height * Scale - (float)settings._charWidth - 8;
            _tmpRect.x += _tmpRect.width * 0.5f - tmpSize * 0.5f;
            _tmpRect.y = pos.y * Scale + 4;
            _tmpRect.width = tmpSize;
            _tmpRect.height = tmpSize;
            GUI.DrawTexture(_tmpRect, settings._icons[iconIdx]);
		}
		// 绘制详情.
		virtual public void DrawInspector(ref Link selected)
        {
            GUILayout.BeginHorizontal();
            {
                GUILayout.Label("Name ", GUILayout.Height(GKToyMakerBase.toyMakerBase._lineHeight));
                name = GUILayout.TextField(name, GKToyMakerBase.toyMakerBase._infoMaxLineChar, GUILayout.Height(GKToyMakerBase.toyMakerBase._lineHeight));
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Comment", GUILayout.Height(GKToyMakerBase.toyMakerBase._lineHeight));
            comment = GUILayout.TextArea(comment, GUILayout.Height(GKToyMakerBase.toyMakerBase._lineHeight * 5));
			
            if (0 != links.Count)
            {
				GKEditor.DrawInspectorSeperator();

				GUILayout.Label("Links");

				GUILayout.BeginVertical("Box");
                {
					bool isFirstLink = true;
					foreach (int i in links.Keys)
                    {
						DrawNextDetail(ref isFirstLink, i, ref selected);
					}
                }
                GUILayout.EndVertical();
			}
        }

        // 绘制连接点详情.
        virtual protected void DrawNextDetail(ref bool isFirst, int idx, ref Link selected)
        {
			if (!isFirst)
				GKEditor.DrawMiniInspectorSeperator();
			else
				isFirst = false;

			GUILayout.BeginHorizontal();
            {
				if (null != selected && selected.id == idx)
				{
					GUI.backgroundColor = Color.yellow;
				}
				if (GUILayout.Button(links[idx].next.name))
				{
					selected = links[idx];
				}
				GUI.backgroundColor = Color.red;
				if (GUILayout.Button("X", GUILayout.Width(GKToyMakerBase.toyMakerBase._lineHeight)))
                {
                    GKToyMakerBase.RemoveLink(id, links[idx].next);
                }
				GUI.backgroundColor = GKToyMakerBase._lastBgColor;
			}
            GUILayout.EndHorizontal();
        }
        #endregion
    }

	/// <summary>
	/// 连线类
	/// </summary>
	public class Link
	{
		public int id;
		public bool isFirstVertical;
		public List<Vector2> points;
		public GKToyNode next;

		public Link(int _id, List<Vector2> _points, bool _isFirstVertical, GKToyNode _next)
		{
			id = _id;
			points = new List<Vector2>(_points);
			isFirstVertical = _isFirstVertical;
			next = _next;
		}
	}
}
