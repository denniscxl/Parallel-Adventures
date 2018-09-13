using UnityEngine;
using System.Linq;

namespace GKToy
{
	[NodeTypeTree("Action/Input/Move with Control")]
	public class GKToyActioMoveWithControl : GKToyNode
    {
		[SerializeField]
		private float m_Speed = 0;
		[SerializeField]
		private Transform m_transform = Camera.main.transform;
		public float Speed
		{
			get { return m_Speed; }
			set { m_Speed = value; }
		}
		public Transform Transform
		{
			get { return m_transform; }
			set { m_transform = value; }
		}

		public GKToyActioMoveWithControl(int _id) : base(_id) { }

		public override int Update()
		{
			if (m_transform != null)
			{
				if (Input.GetKey(KeyCode.UpArrow))
					m_transform.Translate(Vector3.up * Time.deltaTime * m_Speed);
				else if (Input.GetKey(KeyCode.DownArrow))
					m_transform.Translate(- Vector3.up * Time.deltaTime * m_Speed);
				if (Input.GetKey(KeyCode.RightArrow))
					m_transform.Translate(Vector3.right * Time.deltaTime * m_Speed);
				else if (Input.GetKey(KeyCode.LeftArrow))
					m_transform.Translate(-Vector3.right * Time.deltaTime * m_Speed);
			}
			machine.GoToState(id, links.Select(x => x.next).ToList());
			state = NodeState.Success;
			return base.Update();
		}
	}
}
