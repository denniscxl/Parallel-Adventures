using System.Collections.Generic;
using System.Linq;
using GKStateMachine;

namespace GKToy
{
	[NodeTypeTree("Action/Base/End")]
	public class GKToyActionEnd : GKToyNode
	{
		public GKToyActionEnd(int _id) : base(_id) { }
		public override int Update()
		{
			List<GKStateMachineStateBase<int>> states =  machine.GetCurrentState();
			machine.StopAll();
			state = NodeState.Success;
			return base.Update();
		}
	}
}
