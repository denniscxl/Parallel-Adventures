using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using GKBase;
using System.Linq;
using GKStateMachine;

namespace GKToy
{
    [System.Serializable]
    public class GKToyNode : GKStateMachineStateBase<int>
    {
        #region PublicField
        public int id;
        public Texture icon;
        public NodeType nodeType = NodeType.Node;
        public Vector2 pos;
        public int width;
        public int height;
        public ModuleType type;
		public string className;
        public string name;
        public string comment;
        public Rect rect;
        public Rect inputRect;
        public Rect outputRect;
        public bool isMove;
        public List<Link> links = new List<Link>();
		public GKNodeStateMachine machine;
		#endregion

		#region PrivateField
		#endregion

		#region PublicMethod
		public GKToyNode(int _id):base(_id){}

        // 通过ID查找链接.
        public Link GetLinkByID(int id)
        {
            foreach (var l in links)
            {
                if (l.id == id)
                    return l;
            }
            return null;
        }

        /// <summary>
        /// 更新单根连线
        /// </summary>
        /// <param name="linkId">要更新连线的Id</param>
        public void UpdateLink(Link link, GKToyNode nextNode)
        {
            Vector2 src = new Vector2(outputRect.x + outputRect.width, outputRect.y + outputRect.height * 0.5f);
            Vector2 dest = new Vector2(nextNode.inputRect.x, nextNode.inputRect.y + nextNode.inputRect.height * 0.5f);
            bool vertical = false;
            link.points = new List<Vector2>(GK.ClacLinePoint(src, dest, out vertical));
            link.isFirstVertical = vertical;
        }

		///// <summary>
		///// 鼠标拖拽本节点时，更新所有连线的坐标
		///// </summary>
		//public void UpdateAllLinks()
		//{
		//	Vector2 src = new Vector2(outputRect.x + outputRect.width, outputRect.y + outputRect.height * 0.5f);
		//	Vector2 dest;
		//	bool vertical;
		//	foreach (Link link in links)
		//	{
		//		vertical = false;
		//		dest = new Vector2(link.next.inputRect.x, link.next.inputRect.y + link.next.inputRect.height * 0.5f);
		//		link.points = new List<Vector2>(GK.ClacLinePoint(src, dest, out vertical));
		//		link.isFirstVertical = vertical;
		//	}
		//}

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
            links.Add( new Link(linkId, GK.ClacLinePoint(src, dest, out vertical), vertical, nextNode.id));
        }

        public void RemoveLink(int removeNodeId)
        {
            Link link = FindLinkFromNode(removeNodeId);
            if (null != link)
                links.Remove(link);
        }

        /// <summary>
        /// 返回连接某个节点的连接Id
        /// </summary>
        /// <param name="node">被连接的节点</param>
        /// <returns>连接Id</returns>
        public int FindLinkIdFromNode(int nodeId)
        {
            var res = links.Where(x => x.next == nodeId).FirstOrDefault();
            if (!default(KeyValuePair<int, Link>).Equals(res))
            {
                return res.id;
            }
            return -1;
        }

        public Link FindLinkFromNode(int nodeId)
        {
            var res = links.Where(x => x.next == nodeId).FirstOrDefault();
            if (!default(KeyValuePair<int, Link>).Equals(res))
            {
                return res;
            }
            return null;
        }

		public override void Enter()
		{
			Debug.Log("Enter " + name);
		}

		public override int Update()
		{
			return 0;
		}

		public override void Exit()
		{
			Debug.Log("Exit " + name);
		}
		#endregion

		#region PrivateMethod
		#endregion
	}

	/// <summary>
	/// 连线类
	/// </summary>
    [System.Serializable]
	public class Link
	{
		public int id;
		public bool isFirstVertical;
		public List<Vector2> points;
		public int next;

		public Link(int _id, List<Vector2> _points, bool _isFirstVertical, int _next)
		{
			id = _id;
			points = new List<Vector2>(_points);
			isFirstVertical = _isFirstVertical;
			next = _next;
		}
	}
}
