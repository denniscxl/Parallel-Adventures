using UnityEngine;
using System.Linq;

namespace GKToy
{
	[NodeTypeTree("Action/Input/SetPosition")]
	public class GKToyActioSetPosition : GKToyNode
    {
		[SerializeField]
		private GKToySharedVector3 m_Position = new GKToySharedVector3();
		private Transform m_transform;
		public GKToySharedVector3 Position
		{
			get { return m_Position; }
			set { m_Position = value; }
		}

		public GKToyActioSetPosition(int _id) : base(_id) { }

		public override void Init(GKToyBaseOverlord ovelord)
		{
			base.Init(ovelord);
			m_transform = ovelord.gameObject.GetComponent<Transform>();
		}

		public override int Update()
		{
			if (m_transform != null)
			{
				m_transform.position = m_Position.Value;
			}
			machine.GoToState(id, links.Select(x => x.next).ToList());
			state = NodeState.Success;
			return base.Update();
		}
	}
}
