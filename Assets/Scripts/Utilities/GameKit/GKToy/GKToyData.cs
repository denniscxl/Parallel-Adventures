using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor.Callbacks;
using UnityEditor;
using System.Text.RegularExpressions;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace GKToy
{
    [System.Serializable]
    public class GKToyData
    {
        public ModuleType moduleType = ModuleType.Base;
        public string name = "Hello";
        public string comment = "";
        // 启动时运行.
        public bool startWhenEnable = false;
        // 完成后删除.
        public bool destoryWhenCompleted = true;
        public int nodeGuid = 0;
        public int linkGuid = 0;
		// Node链表.
		public List<string> nodeData = new List<string>();
		public List<string> nodeTypeData = new List<string>();
		public List<object> nodeLst = new List<object>();
        public bool varuableChanged = false;
        public List<string> variableData = new List<string>();
        public List<string> variableTypeData = new List<string>();
        public Dictionary<string, List<object>> variableLst = new Dictionary<string, List<object>>();

        // 通过ID查找节点.
        public GKToyNode GetNodeByID(int id)
        {
			foreach (GKToyNode n in nodeLst)
            {
                if (n.id == id)
                    return n;
            }
            return null;
        }

        // 变量元素.
        public void RemoveVariable(string key, object val)
        {
            if (variableLst.ContainsKey(key))
            {
                variableLst[key].Remove(val);
            }
        }

        // 变量转化为Json存储.
        public void SaveVariable()
        {
            variableData.Clear();
            variableTypeData.Clear();
            foreach (var objs in variableLst)
            {
                foreach (var obj in objs.Value)
                {
                    variableData.Add(JsonUtility.ToJson(obj));
                    variableTypeData.Add(objs.Key);
                }
            }
			// 设置场景有更新.
			if (!Application.isPlaying)
			{
				Scene scene = SceneManager.GetActiveScene();
				EditorSceneManager.MarkSceneDirty(scene);
			}
		}

        // Json转化为变量.
        public void LoadVariable()
        {
            variableLst.Clear();
            int i = 0;
            foreach (var d in variableData)
            {
                Type t = Type.GetType(variableTypeData[i]);
                var v = JsonUtility.FromJson(d, t) as GKToyVariable;
                if (variableLst.ContainsKey(v.PropertyMapping))
                {
                    variableLst[v.PropertyMapping].Add(v);
                }
                else
                {
                    List<object> lst = new List<object>();
                    lst.Add(v);
                    variableLst.Add(v.PropertyMapping, lst);
                }
                i++;
            }
        }

		// 节点转化为Json存储.
		public void SaveNodes()
		{
			nodeTypeData.Clear();
			List<string> tmpNodeData = new List<string>();
			foreach (var obj in nodeLst)
			{
				string iconPath = AssetDatabase.GetAssetPath(((GKToyNode)obj).icon);
				string data = Regex.Replace(JsonUtility.ToJson(obj), "\"icon\":{.*?}", string.Format("\"icon\":\"{0}\"", iconPath));
				tmpNodeData.Add(data);
				nodeTypeData.Add(((GKToyNode)obj).className);
			}
			// 设置场景有更新.
			if (!Application.isPlaying)
			{
				bool isChanged = false;
				if (tmpNodeData.Count == nodeData.Count)
				{
					for(int i = 0; i < tmpNodeData.Count; ++i)
					{
						if (!tmpNodeData[i].Equals(nodeData[i]))
							isChanged = true;
					}
				}
				else
					isChanged = true;
				if (isChanged)
				{
					Scene scene = SceneManager.GetActiveScene();
					EditorSceneManager.MarkSceneDirty(scene);
				}
			}
			nodeData = tmpNodeData;
		}

		// Json转化为节点.
		public void LoadNodes()
		{
			nodeLst.Clear();
			int i = 0;
			foreach (var d in nodeData)
			{
				Type t = Type.GetType(nodeTypeData[i]);
				int start = d.IndexOf("\"icon\":\"") + 8;
				int end = d.IndexOf("\",", start);
				string iconPath = d.Substring(start, end - start);
				Texture icon = AssetDatabase.LoadAssetAtPath(iconPath, typeof(Texture)) as Texture;
				string data = Regex.Replace(d, "\"icon\":\".*?\"", "\"icon\":{\"instanceID\":" + icon.GetInstanceID() + "}");
				nodeLst.Add(JsonUtility.FromJson(data, t));
				i++;
			}
		}
	}
}
