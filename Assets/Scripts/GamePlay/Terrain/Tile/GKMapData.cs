using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GKMap
{
    public class GKMapData
    {
        #region PublicField
        /// <summary>
        /// Key is tile idx, value is village type.
        /// </summary>
        public Dictionary<int, VillageData> villages = new Dictionary<int, VillageData>();
        public Dictionary<int, FoodData> foods = new Dictionary<int, FoodData>();
        public List<int> rivers = new List<int>();
        public Size size = Size.Epic;
        public MapType type = MapType.Plain;
        public PolygonType polygon = PolygonType.Hexagon;

        #endregion

        #region PublicField 
        private int _unspawnVliiageCount = 0;
        private int _unspawnFoodCount = 0;
        private int _riverCount = 0;
        #endregion

        #region PublicMethod
        public GKMapData(Size size)
        {
            this.size = size;
            villages.Clear();
            foods.Clear();
            rivers.Clear();
            switch(size) 
            {
                case Size.Small:
                    _unspawnVliiageCount = 3;
                    _unspawnFoodCount = 3;
                    _riverCount = 1;
                    break;
                case Size.Normal:
                    _unspawnVliiageCount = 4;
                    _unspawnFoodCount = 4;
                    _riverCount = 1;
                    break;
                case Size.Large:
                    _unspawnVliiageCount = 5;
                    _unspawnFoodCount = 5;
                      _riverCount = 2;   
                    break;
                case Size.World:
                    _unspawnVliiageCount = 6;
                    _unspawnFoodCount = 6;
                    _riverCount = 2;
                    break;
                case Size.Epic:
                    _unspawnVliiageCount = 7;
                    _unspawnFoodCount = 7;
                    _riverCount = 3;
                    break;
            }
        }

        public void RandomRiverPos(int randomType = 0)
        {
            switch (randomType)
            {
                case 0:

                    for (int i = 0; i < _riverCount; i ++)
                    {
                        // node 4 ~ 8.
                        int nodeCount = Random.Range(4, 9);
                        for (int j = 0; j < nodeCount; j ++)
                        {
                            int idx = Random.Range(1, GKMapManager.GetTileCount(PolygonType.Hexagon, this.size));
                            if(!rivers.Contains(idx))
                            {
                                rivers.Add(idx);
                            }
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Build a bridge on the river.
        /// The length of the river is increased one grid, the possibility of building the bridge is increased by 10%.
        /// </summary>
        /// <param name="length">river length.</param>
        public int RandomBride(int length)
        {
            int ret = -1;
            if (length > ((int)size * 0.5f))
            {
                int roll = Random.Range(0, 100);
                if(roll > 50)
                    ret = Random.Range(0, length);
            }
            return ret;
        }

        // 0 village, 1 food.
        public void RandomBuildPos(int type = 0)
        {
            var grids = GKMapManager.Instance().GetGrids();
            int unspawnBuildCount = (0 == type) ? _unspawnVliiageCount : _unspawnFoodCount;
            int half = (int)((int)size * 0.5f);
            int residue = 0;

            // 生成数必须大于多边形辐射面.
            if(unspawnBuildCount >= 6)
            {
                int smallCount = Mathf.RoundToInt(unspawnBuildCount / 6);
                int centerIdx = GKMapManager.GetTileCount(PolygonType.Hexagon, this.size);
                centerIdx = (int)((centerIdx + 1) * 0.5f);
                for (int i = 0; i < 6; i++)
                {
                    int subCenter = FindSubWebCener(grids[centerIdx], half, i);
                    residue += GenerateWeb(half, smallCount, subCenter, type);
                }
                smallCount = unspawnBuildCount - (smallCount * 6);
                if(0 != smallCount)
                    residue += GenerateWeb(half, smallCount, centerIdx, type);
            }
            else
            {
                residue = unspawnBuildCount;
            }

            //Debug.Log(string.Format("RandomBuildPos residue: {0}", residue));

            for (int i = 0; i < residue; i++)
            {
                var buildLst = GKMapManager.Instance().GetCanBuildTile();
                int idx = Random.Range(0, buildLst.Count);
                idx = buildLst[idx];

                if (!grids.ContainsKey(idx))
                {
                    Debug.LogWarning(string.Format("Can't create type: {0}, idx: {1}", type, i));
                    continue;
                }
                    

                if (!grids[idx].GetElement(TerrainElementType.Village) 
                    && !grids[idx].GetElement(TerrainElementType.Food) 
                    && !grids[idx].GetElement(TerrainElementType.River) )
                {
                    if(0 == type)
                        villages[idx] = new VillageData(idx, VillageType.Neutrality);
                    else 
                        foods[idx] = new FoodData(idx); 

                    // 可建造队列中剔除.
                    if (buildLst.Contains(idx))
                        buildLst.Remove(idx);
                }
                else
                {
                    Debug.LogWarning(string.Format("Can't create type: {0}, idx: {1}", type, i));
                }
            }
        }
        #endregion

        #region PrivateMethod
        /// <summary>
        /// Generate Web type Village.
        /// </summary>
        /// <param name="half">radius.</param>
        /// <param name="count">villages count.</param>
        /// <param name="centerIdx">village pos.</param>
        private int GenerateWeb(int half, int count, int centerIdx,int type)
        {
            int residue = 0;
            int raw = 1;
            if (count + 1 < half)
            {
                raw = half / count + 1;
            }
            int lastDestination = 0;
            var grids = GKMapManager.Instance().GetGrids();
            List<Vector2> list = new List<Vector2>();   // destination, offest;
            for (int i = 0; i < count; i++)
            {
                int min = raw * i;
                int offest = Random.Range(min, min + raw);
                int destination = Random.Range(0, 6);
                if (destination == lastDestination)
                {
                    destination = (destination + 3) % 6;
                }
                lastDestination = destination;
                list.Add(new Vector2(destination, offest));
            }

            if (!grids.ContainsKey(centerIdx))
            {
                return residue;
            }
            GKTerrainGrid centerGrid = grids[centerIdx];
            foreach (var l in list)
            {
                NextNode next = centerGrid._nexts[(int)l.x];
                for (int i = 0; i < l.y - 1; i++)
                {
                    next = next.grid._nexts[(int)l.x];
                    if (null == next)
                        break;
                }
                if (null != next && !next.grid.GetElement(TerrainElementType.River)
                    && !next.grid.GetElement(TerrainElementType.Village) && !next.grid.GetElement(TerrainElementType.Food))
                {
                    if(0 == type)
                        villages[next.linkIdx] = new VillageData(next.linkIdx, VillageType.Neutrality);
                    else
                        foods[next.linkIdx] = new FoodData(next.linkIdx);
                }
                else
                {
                    residue++;
                }
            }

            return residue;
        }

        /// <summary>
        /// Calc sub center pos.
        /// </summary>
        /// <returns>The sub web cener.</returns>
        /// <param name="center">map center pos.</param>
        /// <param name="half">radius.</param>
        /// <param name="destination">destination.</param>
        private int FindSubWebCener(GKTerrainGrid center, int half, int destination)
        {
            int count = (int)(half * 0.5f);
            //Debug.Log(string.Format("FindSubWebCener count: {0}", count));
            int ret = 0;
            NextNode next = center._nexts[destination];
            for (int i = 0; i < count; i++)
            {
                next = next.grid._nexts[destination];
                if (null == next)
                    break;
            }
            if (null != next)
                ret = next.linkIdx;
            return ret;
        }
        #endregion
    }
}
