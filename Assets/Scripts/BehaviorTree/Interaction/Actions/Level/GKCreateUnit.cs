using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;
using GKMap;

[TaskCategory("Level")]
public class GKCreateUnit : Action
{
    private CampType _camp = CampType.Blue;

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

        int tile = (int)Owner.GetVariable("VillageTileIdx").GetValue();
        int index = (int)Owner.GetVariable("Index").GetValue();

        var enemyData = DataController.Data.GetEnemyData(index);
        if(null == enemyData)
            return TaskStatus.Failure;
        
        GKData.GKDataBase data = LevelController.Instance().UseCard(_camp, enemyData.unit, enemyData.skills, enemyData.equips);

        if (null == data)
            return TaskStatus.Failure;

       var unit = LevelController.Instance().CreateUnit(_camp, data, tile);
        if (null == unit)
            return TaskStatus.Failure;

        return TaskStatus.Success;
    }
}