using UnityEngine;
using System.Linq;

namespace GKToy
{
	[NodeTypeTree("Action/Time/Wait", "Assets/Art/GKToyMaker/Icon/Wait.png")]
	public class GKToyActionWait : GKToyNode
    {
		[SerializeField]
        private GKToySharedFloat m_WaitTime = 0;
        public GKToySharedFloat WaitTime
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
            if (curTime >= (float)m_WaitTime.GetValue())
			{
				machine.GoToState(id, links.Select(x => x.next).ToList());
				state = NodeState.Success;
			}
			return base.Update();
		}
	}
}
