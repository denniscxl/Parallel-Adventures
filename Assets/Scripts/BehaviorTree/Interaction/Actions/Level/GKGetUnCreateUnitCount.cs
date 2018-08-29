using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;
using GKMap;

[TaskCategory("Level")]
public class GKGetUnCreateUnitCount : Action
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
        if (null == lst)
            return TaskStatus.Failure;

        Owner.GetVariable("Count").SetValue(lst.Count);

        return TaskStatus.Success;
    }
}