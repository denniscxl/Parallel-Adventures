using UnityEngine;
using System.Linq;

namespace GKToy
{
	[NodeTypeTree("Action/Input/CheckInputKey")]
	public class GKToyActioCheckInputKey : GKToyNode
    {
		[SerializeField]
		private KeyCode m_Key = 0;
		public KeyCode Key
		{
			get { return m_Key; }
			set { m_Key = value; }
		}

		public GKToyActioCheckInputKey(int _id) : base(_id) { }

		public override int Update()
		{
			if (Input.GetKey(m_Key))
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
