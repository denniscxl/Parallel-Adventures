using System.Collections;
using System.Linq;

namespace GKToy
{
	[NodeTypeTree("Decoration/Repeat")]
	public class GKToyDecorationRepeat : GKToyNode
	{
		public GKToyDecorationRepeat(int _id) : base(_id) { }
		public override int Update()
		{
			machine.GoToState(id, links.Select(x => x.next).ToList());
			return base.Update();
		}
	}
}
