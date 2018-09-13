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
			}
			return base.Update();
		}

		private bool IsLessThan()
		{
			if (m_InputValue.CompareTo(m_TargetValue) < 0)
				return true;
			else
				return false;
		}

		private bool IsBiggerThan()
		{
			if (m_InputValue.CompareTo(m_TargetValue) > 0)
				return true;
			else
				return false;
		}

		private bool IsEqualTo()
		{
			if (m_InputValue.CompareTo(m_TargetValue) == 0)
				return true;
			else
				return false;
		}

		private bool IsNotEqualTo()
		{
			if (m_InputValue.CompareTo(m_TargetValue) != 0)
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
