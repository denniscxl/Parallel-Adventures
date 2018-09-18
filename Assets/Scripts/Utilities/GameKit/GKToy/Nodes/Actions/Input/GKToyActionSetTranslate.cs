using UnityEngine;
using System.Linq;

namespace GKToy
{
	[NodeTypeTree("Action/Input/Set Translate")]
	public class GKToyActioSetTranslate : GKToyNode
    {
		[SerializeField]
		private GKToySharedFloat m_Speed = 0;
		private Transform m_transform;
		public GKToySharedFloat Speed
		{
			get { return m_Speed; }
			set { m_Speed = value; }
		}

		public GKToyActioSetTranslate(int _id) : base(_id) { }

		public override void Init(GKToyBaseOverlord ovelord)
		{
			base.Init(ovelord);
			m_transform = ovelord.gameObject.GetComponent<Transform>();
		}

		public override int Update()
		{
			if (m_transform != null)
			{
				if (Input.GetKey(KeyCode.UpArrow))
					m_transform.Translate(Vector3.up * Time.deltaTime * m_Speed.Value);
				else if (Input.GetKey(KeyCode.DownArrow))
					m_transform.Translate(- Vector3.up * Time.deltaTime * m_Speed.Value);
				if (Input.GetKey(KeyCode.RightArrow))
					m_transform.Translate(Vector3.right * Time.deltaTime * m_Speed.Value);
				else if (Input.GetKey(KeyCode.LeftArrow))
					m_transform.Translate(-Vector3.right * Time.deltaTime * m_Speed.Value);
			}
			machine.GoToState(id, links.Select(x => x.next).ToList());
			return base.Update();
		}
	}
}
