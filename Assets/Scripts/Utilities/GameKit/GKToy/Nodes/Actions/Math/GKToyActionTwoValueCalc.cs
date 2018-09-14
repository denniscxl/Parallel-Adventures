using System.Linq;
using UnityEngine;

namespace GKToy
{
    [NodeTypeTree("Action/Math/DoubleValueCalc")]
	public class GKToyActionTwoValueCalc : GKToyNode
	{
		[SerializeField]
		float m_InputValue1 = 0;
		[SerializeField]
		float m_InputValue2 = 0;
		[SerializeField]
		float m_OutputValue = 0;
		[SerializeField]
		TwoValueCalcType m_CalcType = TwoValueCalcType.Add;

		public float Input1
		{
			get { return m_InputValue1; }
			set { m_InputValue1 = value; }
		}
		public float Input2
		{
			get { return m_InputValue2; }
			set { m_InputValue2 = value; }
		}
		public float Output
		{
			get { return m_OutputValue; }
			set { m_OutputValue = value; }
		}
		public TwoValueCalcType Type
		{
			get { return m_CalcType; }
			set { m_CalcType = value; }
		}

		public GKToyActionTwoValueCalc(int _id) : base(_id) { }

		public override int Update()
		{
			switch (m_CalcType)
			{
				case TwoValueCalcType.Add:
					Add();
					break;
				case TwoValueCalcType.Minus:
					Minus();
					break;
				case TwoValueCalcType.Multiply:
					Multiply();
					break;
				case TwoValueCalcType.Divide:
					Divide();
					break;
				case TwoValueCalcType.Mod:
					Mod();
					break;
			}
			machine.GoToState(id, links.Select(x => x.next).ToList());
			state = NodeState.Success;
			return base.Update();
		}

		private void Add()
		{
			m_OutputValue = m_InputValue1 + m_InputValue2;
		}
		private void Minus()
		{
			m_OutputValue = m_InputValue1 - m_InputValue2;
		}
		private void Multiply()
		{
			m_OutputValue = m_InputValue1 * m_InputValue2;
		}
		private void Divide()
		{
			m_OutputValue = m_InputValue1 / m_InputValue2;
		}
		private void Mod()
		{
			m_OutputValue = m_InputValue1 % m_InputValue2;
		}
		public enum TwoValueCalcType
		{
			Add = 0,
			Minus,
			Multiply,
			Divide,
			Mod
		}
	}
}
