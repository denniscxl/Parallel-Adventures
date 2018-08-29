using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKBase;
using GKRole;

namespace GKMap
{
    [System.Serializable]
    public class GKTerrainGrid : MonoBehaviour
    {

        #region PublicField
        public GKTile tile;
        public int tileID = 0;
        public int row = 0;
        public int col = 0;
        public NextNode[] _nexts;

        public GKVillage Villager
        {
            get 
            { 
                return _villager;
            }
            set 
            { 
                _villager = value; 
            }
        }
        public GKFood Food
        {
            get
            {
                return _food;
            }
            set
            {
                _food = value;
            }
        }

        public GameObject HighLight
        {
            get 
            {
                if(null == _highLight)
                {
                    // 如果发生异常情况, 无法获取地块数据. 使用默认高亮效果.
                    if(null == tile)
                    {
                        _highLight = GK.LoadGameObject("Prefabs/Partile/FX_HighLightRoad");
                    }
                    else
                    {
                        switch (tile.terrain)
                        {
                            case TerrainType.Grass:
                                _highLight = GK.LoadGameObject("Prefabs/Partile/FX_HighLightGrass");
                            break;
                            case TerrainType.River:
                                _highLight = GK.LoadGameObject("Prefabs/Partile/FX_HighLightRiver");
                                break;
                            case TerrainType.Mountain:
                                _highLight = GK.LoadGameObject("Prefabs/Partile/FX_HighLightMountain");
                                break;
                        }
                    }
                    GK.SetParent(_highLight, gameObject, false);
                }
                return _highLight;
            }
        }
		#endregion

		#region PrivateField
        private bool[] _existElements = new bool[(int)TerrainElementType.Count];
        private GKVillage _villager;
        private GKFood _food;
        private GameObject _highLight = null;
        private List<GKUnit> _unitList = new List<GKUnit>();
        private GameObject _cloud = null;
		#endregion

		#region PublicMethod
		public void Awake()
		{
            _nexts = new NextNode[(int)GKMapManager.Instance().PolygonType];
            _unitList.Clear();

            int cloudRand = Random.Range(0, 2);
            _cloud = GK.TryLoadGameObject("Prefabs/Cloud/Cloud_" + cloudRand);
            GK.SetParent(_cloud, gameObject, false);
		}

        // 地块被探索.
        public void Discover()
        {
            if (null == _cloud)
                return;
            var animation = _cloud.GetComponent<Animation>().Play();
            MyGame.Instance.StartCoroutine(DestoryCloud());
        }

        public void LinkTile(GameObject t)
		{
            GK.SetParent(t, gameObject, false);
            tile = GK.GetOrAddComponent<GKTile>(t);
		}

        public void ReplaceTile(GKTile tile)
        {
            if(null != this.tile)
            {
                this.tile.MoveObject(tile);
                GK.Destroy(this.tile.gameObject);
            }
            GK.SetParent(tile.gameObject, gameObject, false);
            this.tile = tile;
            // Relink nexts tile;
            for (int i = 0; i < _nexts.Length; i++) 
            {
                int idx = (i + 3) % 6;
                if(null == _nexts[i] || null == _nexts[i].grid || _nexts[i].grid._nexts.Length <= idx || null == _nexts[i].grid._nexts[idx])
                {
                    continue;
                }

                _nexts[i].grid._nexts[idx].tile = tile;
            }

        }

        public void SetElement(TerrainElementType type, bool exsit)
        {
            _existElements[(int)type] = exsit;
        }
        public bool GetElement(TerrainElementType type)
        {
            return _existElements[(int)type];
        }

        /// <summary>
        /// Gets the next nodes array.
        /// 0: Road. 1: River.
        /// </summary>
        /// <returns>nodes array.</returns>
        public NextNode[] GetNextNodes(int type = 0)
        {
            List<NextNode> result = new List<NextNode>();
            foreach (var n in _nexts)
            {
                switch(type)
                {
                    // Road.
                    case 0:
                        if (null != n && n.grid.GetElement(TerrainElementType.Road))
                            result.Add(n);
                        break;
                    case 1:
                        if (null == n || n.grid.GetElement(TerrainElementType.River))
                            result.Add(n);
                        break;
                }

            }
            return result.ToArray();
        }

        public NextNode[] GetInversNextNodes(int type = 0)
        {
            List<NextNode> result = new List<NextNode>();
            foreach (var n in _nexts)
            {
                switch (type)
                {
                    // Road.
                    case 0:
                        if (null != n && !n.grid.GetElement(TerrainElementType.Road))
                            result.Add(n);
                        break;
                    case 1:
                        if (null != n && !n.grid.GetElement(TerrainElementType.River))
                            result.Add(n);
                        break;
                }

            }
            return result.ToArray();
        }

        public void SetNextNode(int direction, NextNode node)
        {
            if (0 > direction || direction >= _nexts.Length)
            {
                return;
            }
            _nexts[direction] = node;
        }

        // 能否移动到这一点.
        // direction 上一节点到这一节点的方向.
        public bool CanMoveToTargetGrid(int direction, int layermask)
        {
            bool m2road  = false;
            bool m2grass = false;
            bool m2river = false;
            m2road = ((layermask & (int)MoveType.Road) == (int)MoveType.Road);

            switch(tile.terrain)
            {
                case TerrainType.Grass:
                    m2grass = ((layermask & (int)MoveType.Grass) == (int)MoveType.Grass);
                    // Can walk on the grass.
                    if ( m2grass)
                    {
                        return true;
                    }
                    else if(m2road && GetElement(TerrainElementType.Road) && ExistRoadEntranceByLastDirection(direction))
                    {
                        return true;
                    }
                    break;
                case TerrainType.River:
                    m2river = ((layermask & (int)MoveType.River) == (int)MoveType.River);
                    // Can walk on the river.
                    if (m2river)
                    {
                        return true;
                    }
                    // Bridge on river.
                    else if (m2road && GetElement(TerrainElementType.Road) && ExistRoadEntranceByLastDirection(direction))
                    {
                        return true;
                    }

                    break;
            }
            return false;
        }

        /// <summary>
        /// Gets the invers direction.
        /// </summary>
        /// <returns>The invers direction.</returns>
        public int GetInversDirection()
        {
            if (null == tile)
                return 0;
            return (tile._rotation + 3) % 6;
        }

        // 判断是否在非基本地块上存在可通行设施.
        public bool ExistRoadEntranceByLastDirection(int direction)
        {
            if (!GetElement(TerrainElementType.Road))
            {
                //Debug.Log(string.Format("Can't find entrance. id: {0}, direction: {1}", tileID, direction));
                return false;
            }
                
            // 求相邻边界.
            direction = (direction + 3) % 6;
            if (null == tile)
                return false;
            return tile.CanMove(direction);
        }

        public void GetBlockExit(ref List<int> ret)
        {
            ret.Clear();
            if(GetElement(TerrainElementType.Road) && null != tile )
            {
                tile.GetBlockRoad(ref ret);
            }
        }

        #region Unit
        // 获取地块内角色列表.
        public List<GKUnit> GetUnitList()
        {
            return _unitList;
        }

        // 角色进入地块.
        public void Enter(GKUnit unit)
        {
            if(_unitList.Contains(unit))
                return;
            _unitList.Add(unit);
        }

        // 角色离开地块.
        public void Leave(GKUnit unit)
        {
            if (!_unitList.Contains(unit))
                return;
            _unitList.Remove(unit);
        }
        #endregion

		#endregion

		#region PrivateMethod
        // 删除云对象.
        private IEnumerator DestoryCloud()
        {
            yield return new WaitForSeconds(1);
            GK.Destroy(_cloud);
        }
		#endregion
	}

    [System.Serializable]
    public class NextNode
    {
        public NextNode(int direction, int idx, GKTerrainGrid grid)
        {
            this.grid = grid;
            linkIdx = idx;
            this.tile = grid.tile;
            this.direction = direction;
        }
        public int direction;
        public GKTerrainGrid grid;
        public GKTile tile;
        public int linkIdx;
    }

    // 移动取模值. 对应角色移动LayerMask.
    public enum MoveType
    {
        Road  = 1 ,         // 1.
        Grass = 1 << 1,     // 2.
        River = 1 << 2,     // 4.
        Mountain = 1 << 3,  // 8.

        Village = 1 << 10,  // 1024.
        Food = 1 << 11,     // 2048.
    }

    // 地块中包含元素枚举.
    public enum TerrainElementType
    {
        Grass = 0,
        River,
        Mountain,
        Road,       // 包含河面上的桥. 所有地块可通行路径泛指路径. 减少不必要的枚举.
        Village,
        Food,
        Count
    }

    public enum HighLightType
    {
        NoHighLght = 0,
        Road,
        Grass,
        River,
        mountain,
    }
}
