using System.Linq;

namespace GKToy
{
	[NodeTypeTree("Action/Base/Start", "", false)]
    public class GKToyActionStart : GKToyNode
    {

		public GKToyActionStart(int _id) : base(_id) { }

		public override int Update()
		{
			machine.GoToState(id, links.Select(x => x.next).ToList());
			return base.Update();
		}
	}
}
