using System.Linq;

namespace GKToy
{
	[NodeTypeTree("Action/Start")]
    public class GKToyActionStart : GKToyNode
    {

		public GKToyActionStart(int _id) : base(_id) { }
		public override void Enter()
		{
			base.Enter();
		}

		public override int Update()
		{
			machine.GoToState(id, links.Select(x => x.next).ToList());
			return base.Update();
		}
	}
}
