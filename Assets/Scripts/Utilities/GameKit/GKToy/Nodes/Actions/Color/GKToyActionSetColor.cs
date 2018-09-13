using UnityEngine;
using System.Linq;

namespace GKToy
{
    [NodeTypeTree("Action/Color/SetColor")]
	public class GKToyActioSetColor : GKToyNode
    {
		[SerializeField]
		private Renderer m_renderer = new Renderer();
		static int colorIndex = 0;
		public Renderer Renderer
		{
			get { return m_renderer; }
			set { m_renderer = value; }
		}

        public GKToyActioSetColor(int _id) : base(_id) { }

		public override int Update()
		{
			if (m_renderer != null)
			{
				switch (colorIndex)
				{
					case 0:
						m_renderer.material.color = Color.red;
						colorIndex = 1;
						break;
					case 1:
						m_renderer.material.color = Color.blue;
						colorIndex = 2;
						break;
					case 2:
						m_renderer.material.color = Color.yellow;
						colorIndex = 0;
						break;
				}
			}
			machine.GoToState(id, links.Select(x => x.next).ToList());
			state = NodeState.Success;
			return base.Update();
		}
	}
}
