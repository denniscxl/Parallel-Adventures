using UnityEngine;
using System.Linq;

namespace GKToy
{
	[NodeTypeTree("Action/Input/Key Checker")]
	public class GKToyActioKeyChecker : GKToyNode
    {
		[SerializeField]
		private KeyCode m_Key = 0;
		private bool isSuccess = true;
		public KeyCode Key
		{
			get { return m_Key; }
			set { m_Key = value; }
		}

		public GKToyActioKeyChecker(int _id) : base(_id) { }

		public override int Update()
		{
			if (Input.GetKey(m_Key))
			{
				machine.GoToState(id, links.Select(x => x.next).ToList());
			}
			else
			{
				machine.LeaveState(id);
				isSuccess = false;
			}
			return base.Update();
		}

		public override void Exit()
		{
			if (isSuccess)
				state = NodeState.Success;
			else
				state = NodeState.Fail;
			base.Exit();
		}
	}
}
