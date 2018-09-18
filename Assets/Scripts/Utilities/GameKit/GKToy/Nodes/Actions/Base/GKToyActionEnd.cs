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
			machine.StopAll();
			machine.LeaveState(id);
			return base.Update();
		}
	}
}
