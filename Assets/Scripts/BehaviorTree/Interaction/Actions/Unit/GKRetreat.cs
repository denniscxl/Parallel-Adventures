using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;
using GKMap;

[TaskCategory("Unit")]
public class GKRetreat : Action
{
	private GKUnit _unit;
    private GKTerrainGrid _tile;
    private float _tmpDistance = 0;

    public override void OnAwake()
    {
        _unit = transform.GetComponent<GKUnit>();
    }

    public override void OnStart()
    {
        base.OnStart();
        FindTarget();
        _unit.bSupport = true;
    }

    public override TaskStatus OnUpdate()
    {

        if (null == _unit || null == _tile)
            return TaskStatus.Failure;

        if(CheckArrive())
            return TaskStatus.Success;

        return TaskStatus.Running;
    }

    public override void OnEnd()
    {
        base.OnEnd();
        _unit.bSupport = false;
    }

    // 寻找目标据点.
    private bool FindTarget()
    {
        _tile = null;
        GKVillage villager = LevelController.Instance().GetCloseVillage(_unit.myTransform);
        if (null == villager)
            return false;
        _tile = villager.Grid;
        var pathLst = _unit.GetMovePath(_tile);
        // 检测是否能移动到指定点, 如果不能, 取移动的最后节点设为目标.
        if (pathLst.Count > 0 && pathLst[0].grid != _tile)
        {
            _tile = pathLst[0].grid;
            Owner.GetVariable("Tile").SetValue(_tile);
        }
        _unit.Move(pathLst);
        return true;
    }

    // 检查是否到达目标点.
    private bool CheckArrive()
    {
        _tmpDistance = Vector3.Distance(_unit.myTransform.position, _tile.transform.position);
        //  地形高度为1.
        return _tmpDistance < 1.1f;
    }
}