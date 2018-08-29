using BehaviorDesigner.Runtime;
using GKMap;
using GKRole;
using System.Collections.Generic;
using GKPathFinding;

public class GKCustomVariables {

    // 指令.
    [System.Serializable]
    public class BDCommandType : SharedVariable<CommandType>
    {
        public static implicit operator BDCommandType(CommandType value) { return new BDCommandType { Value = value }; }
    }

    // 地块.
    [System.Serializable]
    public class BDTile : SharedVariable<GKTerrainGrid>
    {
        public static implicit operator BDTile(GKTerrainGrid value) { return new BDTile { Value = value }; }
    }

    // 移动路径.
    [System.Serializable]
    public class BDPath : SharedVariable<List<AStarPoint>>
    {
        public static implicit operator BDPath(List<AStarPoint> value) { return new BDPath { Value = value }; }
    }

    // 移动节点.
    [System.Serializable]
    public class BDAStarNode : SharedVariable<AStarPoint>
    {
        public static implicit operator BDAStarNode(AStarPoint value) { return new BDAStarNode { Value = value }; }
    }

    // 角色.
    [System.Serializable]
    public class BDUnit : SharedVariable<GKUnit>
    {
        public static implicit operator BDUnit(GKUnit value) { return new BDUnit { Value = value }; }
    }

}
