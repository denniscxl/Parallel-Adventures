using System.Collections.Generic;
using UnityEngine;
using GKBase;

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
        public ToyData data = new ToyData();
        public List<GKToyShardVariable<object>> variableLst = new List<GKToyShardVariable<object>>();

        #endregion

        #region PrivateField

        #endregion

        #region PublicMethod

        #endregion

        #region PrivateMethod
        // Use this for initialization
        void Start()
        {
            toyMakerBase = Settings.toyMakerBase;
        }

        // Update is called once per frame
        void Update()
        {

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
    }

    [System.Serializable]
    public class ToyData
    {
        public ModuleType moduleType = ModuleType.Base;
        public string name = "Hello";
        public string comment = "";
        public int nodeGuid = 0;
        public int linkGuid = 0;
        // Node链表.
        //public Dictionary<int, GKToyNode> nodeLst = new Dictionary<int, GKToyNode>();
        public List<GKToyNode> nodeLst = new List<GKToyNode>();

        // 通过ID查找节点.
        public GKToyNode GetNodeByID(int id)
        {
            foreach(var n in nodeLst)
            {
                if (n.id == id)
                    return n;
            }
            return null;
        }
    }
}
