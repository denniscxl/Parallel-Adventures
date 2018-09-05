using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace GKToy
{
	public class GKToyMakerTypeManager
	{
		// 类型节点词典，Key-类型名称，Value-节点路径.
		public Dictionary<string, NodeTypeTreeAttribute> typeAttributeDict;
		public GKToyMakerTypeManager(Type parent)
		{
			LoadTypeTreeItem(parent);
		}

		private void LoadTypeTreeItem(Type parent)
		{
			if (typeAttributeDict != null)
				return;
			typeAttributeDict = new Dictionary<string, NodeTypeTreeAttribute>();
			List<Type> subTypes = parent.Assembly.GetTypes().Where(type => type.IsSubclassOf(parent)).ToList();
			foreach (Type type in subTypes)
			{
				var attributes = type.GetCustomAttributes(typeof(NodeTypeTreeAttribute), false);
				if (attributes.Length > 0)
				{
					typeAttributeDict.Add(type.Name, (NodeTypeTreeAttribute)attributes[0]);
					//Debug.Log(type.Name);
				}
			}
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public class NodeTypeTreeAttribute : Attribute
	{
		public string treePath;
		public string iconPath;

		public NodeTypeTreeAttribute(string _treePath, string _iconPath = "")
		{
			treePath = _treePath;
			iconPath = _iconPath;
		}
	}
}
