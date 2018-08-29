using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;
using GKMap;

[TaskCategory("Level")]
public class GKGetUnitCost : Action
{
    private CampType _camp = CampType.Yellow;

    public override void OnStart()
    {
        base.OnStart();
        _camp = (CampType)Owner.GetVariable("Camp").GetValue();
    }

    public override TaskStatus OnUpdate()
    {
        // 中立尚未初始化完成.
        if (CampType.Yellow == _camp)
            return TaskStatus.Failure;

        var lst = LevelController.Instance().GetRemainderCards(_camp);
        if (null == lst || 0 == lst.Count)
            return TaskStatus.Failure;
        
        int idx = (int)Owner.GetVariable("Index").GetValue();

        var enemyData = DataController.Data.GetEnemyData(lst[idx]);
        if (null == enemyData)
            return TaskStatus.Failure;

        var unitData = DataController.Data.GetUnitData(enemyData.unit);
        if (null == unitData)
            return TaskStatus.Failure;

        Owner.GetVariable("DemandFood").SetValue(unitData.costFood);
        Owner.GetVariable("DemandBelief").SetValue(unitData.costBelief);

        return TaskStatus.Success;
    }
}