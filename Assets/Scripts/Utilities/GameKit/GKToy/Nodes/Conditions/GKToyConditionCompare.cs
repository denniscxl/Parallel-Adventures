using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace GKToy
{
	[NodeTypeTree("Condition/Compare")]
	public class GKToyConditionCompare : GKToyNode
	{
		[SerializeField]
        string m_InputValue = string.Empty;
		[SerializeField]
        string m_TargetValue = string.Empty;
		[SerializeField]
		CompareType m_CompareType = CompareType.LessThan;
		private bool isSuccess = true;

		IComparable value1, value2;

		public string Input
		{
			get { return m_InputValue; }
			set { m_InputValue = value; }
		}
		public string Target
		{
			get { return m_TargetValue; }
			set { m_TargetValue = value; }
		}
		public CompareType Type
		{
			get { return m_CompareType; }
			set { m_CompareType = value; }
		}

		public GKToyConditionCompare(int _id) : base(_id) { }

		public override void Enter()
		{
			base.Enter();
			int intRes1, intRes2;
			if (int.TryParse(m_InputValue, out intRes1) && int.TryParse(m_TargetValue, out intRes2))
			{
				value1 = intRes1;
				value2 = intRes2;
				return;
			}
			float floatRes1, floatRes2;
			if (float.TryParse(m_InputValue, out floatRes1) && float.TryParse(m_TargetValue, out floatRes2))
			{
				value1 = floatRes1;
				value2 = floatRes2;
				return;
			}
			value1 = m_InputValue;
			value2 = m_TargetValue;
			return;
		}
		public override int Update()
		{
			bool res = false;
			switch (m_CompareType)
			{
				case CompareType.LessThan:
					res = IsLessThan();
					break;
				case CompareType.BiggerThan:
					res = IsBiggerThan();
					break;
				case CompareType.EqualTo:
					res = IsEqualTo();
					break;
				case CompareType.NotEqualTo:
					res = IsNotEqualTo();
					break;
			}
			if (res)
			{
				machine.GoToState(id, links.Select(x => x.next).ToList());
			}
			else
			{
				machine.LeaveState(id);
				isSuccess = false;
			}
			return base.Update();
		}

		public override void Exit()
		{
			if (isSuccess)
				state = NodeState.Success;
			else
				state = NodeState.Fail;
			base.Exit();
		}

		private bool IsLessThan()
		{
			if (value1.CompareTo(value2) < 0)
				return true;
			else
				return false;
		}

		private bool IsBiggerThan()
		{
			if (value1.CompareTo(value2) > 0)
				return true;
			else
				return false;
		}

		private bool IsEqualTo()
		{
			if (value1.CompareTo(value2) == 0)
				return true;
			else
				return false;
		}

		private bool IsNotEqualTo()
		{
			if (value1.CompareTo(value2) != 0)
				return true;
			else
				return false;
		}

		public enum CompareType
		{
			LessThan = 0,
			BiggerThan,
			EqualTo,
			NotEqualTo
		}
	}
}
