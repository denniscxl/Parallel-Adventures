using System.Collections.Generic;
using UnityEngine;
using GKBase;
using System.Linq;
using GKStateMachine;
using System.Reflection;
using System;

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
        public PropertyInfo[] props;
        public int [] propStates;
        public List<Link> links = new List<Link>();
		public GKNodeStateMachine machine;
		public NodeState state;
        private GKToyBaseOverlord _overlord;
		#endregion

		#region PrivateField

		#endregion

		#region PublicMethod
		public GKToyNode(int _id):base(_id)
        {
        }

        virtual public void Init(GKToyBaseOverlord ovelord)
        {
            _overlord = ovelord;

            Type t = GetType();
            props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            var fs = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if(null == propStates)
            {
                propStates = new int[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    propStates[i] = -1;
                }
            }
			state = NodeState.Inactive;

            // 引用变量赋值.
            for (int i = 0; i < props.Length; i++)
            {
                if(-1 != propStates[i])
                {
                    var v = props[i].GetValue(this, null);
                    var vlst = _overlord.GetVariableListByType(v);
                    if(vlst.Count > propStates[i])
                    {
                        props[i].SetValue(this, ((GKToyVariable)vlst[propStates[i]]), null);
                    }
                }
            }
        }

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
			state = NodeState.Activated;
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

	public enum NodeState
	{
		Inactive = 0,
		Activated,
		Success,
		Fail
	}
}
