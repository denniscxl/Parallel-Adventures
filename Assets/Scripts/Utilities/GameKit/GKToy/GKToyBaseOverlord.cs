using System.Collections.Generic;
using UnityEngine;
using GKBase;
using System;
using System.Linq;

namespace GKToy
{
    [System.Serializable]
    public class GKToyBaseOverlord : MonoBehaviour
    {
        #region PublicField
        protected static Editor_Settings _settings;
        public static Editor_Settings Settings
        {
            get
            {
                if (_settings == null) 
                {
                    _settings = GK.LoadResource<Editor_Settings>("UI/Settings/Editor_Settings");
                }
                return _settings;
            }
        }
        public static Editor_Settings.ToyMakerBase toyMakerBase = null;
        public GKToyData data = new GKToyData();
		public GKNodeStateMachine stateMachine;
		public bool isPlaying = false;
        #endregion

        #region PrivateField

        #endregion

        #region PublicMethod
        // 根据对象类型获取相同类型变量名称列表.
        List<string> tmpVarNames = new List<string>();
        public List<string> GetVariableNameListByType(object val)
        {
            tmpVarNames.Clear();
            foreach (var v in data.variableLst)
            {
                if (val.GetType() == v.Value[0].GetType())
                {
                    foreach (var ele in v.Value)
                        tmpVarNames.Add(((GKToyVariable)ele).Name);
                    return tmpVarNames;
                }
            }
            return tmpVarNames;
        }

        // 根据对象类型获取相同类型变量列表.
        public List<object> GetVariableListByType(object val)
        {
            foreach (var v in data.variableLst)
            {
                if (val.GetType() == v.Value[0].GetType())
                {
                    return v.Value;
                }
            }
            return null;
        }
        #endregion

        #region PrivateMethod
        // Use this for initialization
        void Start()
        {
            toyMakerBase = Settings.toyMakerBase;
            data.Init(this);
            stateMachine = new GKNodeStateMachine(data.nodeLst.Values.ToList());
            isPlaying = data.startWhenEnable;
                
        }

        // Update is called once per frame
        void Update()
        {
			if (isPlaying)
			{
				stateMachine.Update();
			}
		}

        protected void Init()
        {
            
        }
        #endregion
    }

    public enum ModuleType
    {
        Base = 0,
        Tutorial,
    }

    public enum NodeType
    {
        Node = 0,
        Action,
        Condition,
		Decoration
    }
}
