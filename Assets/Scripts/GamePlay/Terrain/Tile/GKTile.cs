using System.Collections.Generic;
using UnityEngine;
using GKBase;

namespace GKMap
{
    public enum PolygonType
    {
        Hexagon = 6
    }
    public enum TerrainType
    {
        Grass       = 0,
        River       = 1,
        Mountain    = 2,
        Count
    }

    abstract public class GKTile : MonoBehaviour
    {

        #region PublicField
        public PolygonType Polygon
        {
            get
            {
                return _polygon;
            }
            set 
            {
                _polygon = value;
                SetPolygon();
            }
        }
        public TerrainType terrain = TerrainType.Grass;
        public int terrainIdx = 0;
        [HideInInspector]
        public NextNode[] preNode = null;
        public int _rotation = 0;
        #endregion

        #region PrivateField
        protected PolygonType _polygon = PolygonType.Hexagon;
        protected int[] _polygonID;
        [SerializeField]
        protected GameObject [] _nodes;
        [SerializeField]
        protected bool[] _roadFlag;
        protected List<NodeItem> objects = new List<NodeItem>();
        #endregion

        #region PublicMethod
        /// <summary>
        /// Init the tile data.
        /// </summary>
        /// <param name="startIdx">Global guid start index.</param>
        public int Init(int startIdx) {
            for (int i = 0; i < (int)_polygon && i < _polygonID.Length; i++)
            {
                _polygonID[0] = startIdx + i;
            }
            return _polygonID[_polygonID.Length - 1];
        }
        /// <summary>
        /// Gets the node gameobject.
        /// </summary>
        /// <returns>Gameobject</returns>
        /// <param name="idx">0 is center, 1~6 clockwise 7~n items.</param>
        public GameObject GetNode(int idx)
        {
            if(idx < 0 || idx >= _nodes.Length)
                return null;
            return _nodes[idx];
        }

        public void AddObject(int idx, GameObject go)
        {
            GK.SetParent(go, GetNode(idx), false);
            objects.Add(new NodeItem(idx, go));
        }

        /// <summary>
        /// Moves the objects to new tile.
        /// </summary>
        /// <param name="target">New tile.</param>
        public void MoveObject(GKTile target)
        {
            foreach(var o in objects)
            {
                target.AddObject(o.nodeIdx, o.target);
            }
        }

        public void Rotate(int angle)
        {
            _rotation = angle;
            transform.localRotation = Quaternion.Euler(0.0f, angle * 60, 0.0f);
            AdjustRoads(angle);
        } 

        public bool CanMove(int direction)
        {
            if (_roadFlag.Length <= direction)
                return false;
            return _roadFlag[direction];
        }

        public void GetBlockRoad(ref List<int> list)
        {
            for (int i = 0; i < _roadFlag.Length; i ++)
            {
                if (!_roadFlag[i])
                    list.Add(i);
            }
        }
        #endregion

        #region PrivateMethod
        abstract protected void SetPolygon();
        abstract protected void AdjustRoads(int angle); 
        #endregion

    }

    public class NodeItem
    {
        public NodeItem(int idx, GameObject go)
        {
            nodeIdx = idx;
            target = go;
        }
        public int nodeIdx;
        public GameObject target;
    }

}

