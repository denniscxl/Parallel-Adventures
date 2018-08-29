using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;
using GKMap;
using GKFOW;

// 决策探索地块.
[TaskCategory("Unit")]
public class GKSelectExploredTile : Action
{
	private GKUnit _unit;
    private CampType _camp = CampType.Blue;
    private GKTerrainGrid _tile;

    public override void OnAwake()
    {
        _unit = transform.GetComponent<GKUnit>();
        _camp = (CampType)_unit.GetAttribute(EObjectAttr.Camp).ValInt;
    }

    public override void OnStart()
    {
        base.OnStart();

        if (null != _unit)
        {
            var lst = FOW.Instance().GetUnDiscoverLst((int)_camp);
            if(null != lst && 0 < lst.Count)
            {
                int rand = Random.Range(0, lst.Count);
                int idx = lst[rand];
                _tile = GKMapManager.Instance().GetGridByKey(idx);
                Owner.GetVariable("Tile").SetValue(_tile);
            }
        }
    }

    public override TaskStatus OnUpdate()
    { 
        if(null == _unit || null == _tile)
			return TaskStatus.Failure;

		return TaskStatus.Success;
    }
}