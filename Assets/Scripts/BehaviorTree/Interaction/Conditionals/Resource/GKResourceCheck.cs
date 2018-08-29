using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("资源检测.")]
[TaskCategory("Resource")]
public class GKResourceCheck : Conditional 
{
    
    [BehaviorDesigner.Runtime.Tasks.Tooltip("资源对比值.")]
    [SerializeField]
    private SharedInt _foodValue;
    [BehaviorDesigner.Runtime.Tasks.Tooltip("资源对比值.")]
    [SerializeField]
    private SharedInt _beliefValue;

    private CampType _camp = CampType.Blue;

    public override void OnStart()
    {
        base.OnStart();
        _camp = (CampType)Owner.GetVariable("Camp").GetValue();
    }

    public override TaskStatus OnUpdate()
	{
        // 中立.
        if (CampType.Yellow == _camp)
            return TaskStatus.Failure;

        if (LevelController.Instance().GetFood(_camp) >= (int)_foodValue.GetValue())
            return TaskStatus.Success;

        if (LevelController.Instance().GetBelief(_camp) >= (int)_beliefValue.GetValue())
            return TaskStatus.Success;

        return TaskStatus.Failure;
	}
}
