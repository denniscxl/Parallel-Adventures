using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GKToy
{
	public class GKNodeStateMachine : GKStateListMachineBase<int>
	{
		public GKNodeStateMachine(List<object> nodes)
		{
			foreach (GKToyNode node in nodes)
			{
				if (node.GetType() == typeof(GKToyActionStart))
				{
					AddState(node, true);
				}
				else
				{
					AddState(node, false);
				}
				node.machine = this;
			}
		}
	}
}
