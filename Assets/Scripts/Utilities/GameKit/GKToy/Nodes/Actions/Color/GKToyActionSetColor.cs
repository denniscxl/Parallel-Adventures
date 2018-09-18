using UnityEngine;
using System.Linq;

namespace GKToy
{
    [NodeTypeTree("Action/Color/Set Color")]
	public class GKToyActioSetColor : GKToyNode
    {
		[SerializeField]
		private Renderer m_renderer;
		[SerializeField]
		private GKToySharedColor m_Color = new GKToySharedColor();
		public GKToySharedColor SetColor
		{
			get { return m_Color; }
			set { m_Color = value; }
		}
		//public Renderer Renderer
		//{
		//	get { return m_renderer; }
		//	set { m_renderer = value; }
		//}

		public GKToyActioSetColor(int _id) : base(_id) { }

		public override void Init(GKToyBaseOverlord ovelord)
		{
			base.Init(ovelord);
			m_renderer = ovelord.gameObject.GetComponentInChildren<Renderer>();
		}

		public override int Update()
		{
			if (m_renderer != null && m_Color != null)
			{
				m_renderer.material.color = m_Color.Value;
			}
			machine.GoToState(id, links.Select(x => x.next).ToList());
			return base.Update();
		}
	}
}
