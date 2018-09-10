using System.Collections.Generic;
using UnityEngine;
using System;

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
        public List<GKToyNode> nodeLst = new List<GKToyNode>();
        public bool varuableChanged = false;
        public List<string> variableData = new List<string>();
        public List<string> variableTypeData = new List<string>();
        public Dictionary<string, List<object>> variableLst = new Dictionary<string, List<object>>();

        // 通过ID查找节点.
        public GKToyNode GetNodeByID(int id)
        {
            foreach (var n in nodeLst)
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
            Debug.Log("SaveVariable");
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
        }

        // Json转化为变量.
        public void LoadVariable()
        {
            Debug.Log("LoadVariable");
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
    }
}
