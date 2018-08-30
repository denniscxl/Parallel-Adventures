using System.Collections.Generic;
using UnityEngine;
using GKBase;

namespace GKToy
{
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

        public List<GKToyShardVariable<object>> variableLst = new List<GKToyShardVariable<object>>();
        // Node链表.
        //protected Dictionary<int, GKToyNode> _nodeLst = new Dictionary<int, GKToyNode>();
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
}
