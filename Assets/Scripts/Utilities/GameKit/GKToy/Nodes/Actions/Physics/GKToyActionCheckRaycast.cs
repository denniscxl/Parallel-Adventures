using UnityEngine;
using System.Linq;

namespace GKToy
{
	[NodeTypeTree("Action/Physics/CheckRaycast")]
	public class GKToyActionCheckRaycast : GKToyNode
	{
		private Transform m_Trans;
		[SerializeField]
		private GKToySharedFloat m_MaxDistance = new GKToySharedFloat();
		[SerializeField]
		private GKToySharedVector3 m_Direction = new GKToySharedVector3();
		public GKToySharedFloat MaxDistance
		{
			get { return m_MaxDistance; }
			set { m_MaxDistance = value; }
		}
		public GKToySharedVector3 Direction
		{
			get { return m_Direction; }
			set { m_Direction = value; }
		}

		public GKToyActionCheckRaycast(int _id) : base(_id) { }

		public override void Init(GKToyBaseOverlord ovelord)
		{
			base.Init(ovelord);
			m_Trans = ovelord.gameObject.GetComponentInChildren<Transform>();
		}

		public override int Update()
		{
			RaycastHit hit;
			if (Physics.Raycast(m_Trans.position, m_Direction.Value, out hit, m_MaxDistance.Value))
			{
				machine.GoToState(id, links.Select(x => x.next).ToList());
				state = NodeState.Success;
			}
			else
			{
				machine.LeaveState(id);
				state = NodeState.Fail;
			}
			return base.Update();
		}
	}
}
