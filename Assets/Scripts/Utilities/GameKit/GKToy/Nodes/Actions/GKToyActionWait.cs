using UnityEngine;
using System.Linq;

namespace GKToy
{
	[NodeTypeTree("Action/Wait","Assets/Art/GKToyMaker/Icon/Wait.png")]
	public class GKToyActionWait : GKToyNode
    {
		[SerializeField]
		private float m_WaitTime;
		public float WaitTime
		{
			get { return m_WaitTime; }
			set { m_WaitTime = value; }
		}

		private float curTime;

		public GKToyActionWait(int _id) : base(_id) { }
		public override void Enter()
		{
			base.Enter();
			curTime = 0;
		}

		public override int Update()
		{
			curTime += Time.deltaTime;
			if (curTime >= m_WaitTime)
			{
				machine.GoToState(id, links.Select(x => x.next).ToList());
			}
			return base.Update();
		}
	}
}
