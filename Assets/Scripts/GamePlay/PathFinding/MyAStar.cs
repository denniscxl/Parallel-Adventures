using System.Collections;
using System.Collections.Generic;
using GKMap;
using GKPathFinding;
using UnityEngine;

public class MyAStar : GKAStar 
{

    #region PublicField
    #endregion

    #region PrivateField
    protected static MyAStar _instance = null;
    protected FindWayType _type = 0;
    // 当前角色可移动层. 
    // 此值与TerrainType进行取模
    protected int _layerMask = 0;
    #endregion

    #region PublicMethod
    public static MyAStar Instance()
    {
        if (_instance == null)
        {
            _instance = new MyAStar();
        }
        return _instance;
    }

    public List<AStarPoint> FindWay(AStarPoint start, AStarPoint destination, FindWayType type, int layerMask = 0)
    {
        _type = type;

        // 如果为角色寻路. 就需要根据角色当前可移动层进行路径规划. 可移动层可以动态修改.
        if(FindWayType.Unit == type)
        {
            _layerMask = layerMask;
        }
        return FindWay(start, destination);
    }
    #endregion

    #region PrivateMethod
    // 判断周围方向上Grid是否可通行.
    // g, 某方向上对应Grid. from 对应方向.
    override protected bool IsBlock(GKTerrainGrid g, int from)  
        {
            switch(_type)
            {
                // Road.
                case FindWayType.Road:
                    {
                        if (g.GetElement(TerrainElementType.River))
                        {
                            if (g.ExistRoadEntranceByLastDirection(from))
                            {
                                //Debug.Log(string.Format("Use the bridge: grid id: {0}", g.tileID));
                                return false;
                            }
                            return true;
                        }
                        else if (g.GetElement(TerrainElementType.Mountain))
                        {
                            return true;
                        }
                    }
                    return false;
                // River.   
                case FindWayType.River:
                    {
                    if (g.GetElement(TerrainElementType.Mountain))
                        return true;
                    }
                    return false;
                // Unit.
                case FindWayType.Unit:
                return !g.CanMoveToTargetGrid(from, _layerMask);

            }
            return false;
        }  
         
    override protected int GetG(AStarPoint target)  
        {  
            if (null == target.last) 
                return 0;

            switch(_type)
            {
                // 建造道路时. 由于还没创建tile. 故只能检测数据元素.
                case FindWayType.Road:
                    if (target.grid.GetElement(TerrainElementType.Road))
                        return target.G + 5;
                    return target.G + 105;
                // 建造河流时. 由于还没创建tile. 故只能检测数据元素.
                case FindWayType.River:
                    if (target.grid.GetElement(TerrainElementType.River))
                        return target.G + 5;
                    return target.G + 135;
                // 角色寻路.
                case FindWayType.Unit:
                {
                    if (target.grid.GetElement(TerrainElementType.Road))
                        return target.G + 15;
                    switch(target.grid.tile.terrain)
                    {
                        case TerrainType.Grass:
                            return target.G + DataController.Data.GetTerrainData((int)TerrainType.Grass).cost; 
                        case TerrainType.River:
                            return target.G + DataController.Data.GetTerrainData((int)TerrainType.River).cost; 
                        case TerrainType.Mountain:
                            return target.G + DataController.Data.GetTerrainData((int)TerrainType.Mountain).cost; 
                    }
                    return target.G + 200;
                }
            }

            return 0;
        } 
    #endregion
}

public enum FindWayType
{
    Road = 0,   //  建造道路.
    River,      //  建造河流.
    Unit,       //  角色寻路.
}
