using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;

[TaskDescription("判断角色是否死亡.")]
[TaskCategory("Unit")]
public class GKIsDead : Conditional 
{
    [BehaviorDesigner.Runtime.Tasks.Tooltip("Optionally store the third sent argument")]
    [SharedRequired]
    public GKCustomVariables.BDUnit target;
	
	public override TaskStatus OnUpdate()
	{
        if(null == target || ((GKUnit)target.GetValue()).GetAttribute(EObjectAttr.Hp).ValInt <= 0)
            return TaskStatus.Success;
        return TaskStatus.Failure;
	}
}
