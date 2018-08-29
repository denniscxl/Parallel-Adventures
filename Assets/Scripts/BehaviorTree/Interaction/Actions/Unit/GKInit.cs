using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;

// 初始化信息.
[TaskCategory("Unit")]
public class GKInit : Action
{
    public override void OnAwake()
    {
        var unit = transform.GetComponent<GKUnit>();
        Owner.GetVariable("Me").SetValue((GKUnit)unit);
    }

    public override TaskStatus OnUpdate()
    { 
        return TaskStatus.Success;
    }

}