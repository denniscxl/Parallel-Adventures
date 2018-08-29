using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;
using GKMap;

// 角色攻击.
[TaskCategory("Unit")]
public class GKAttack : Action
{
	private GKUnit _unit;
    private GKUnit _target;

    public override void OnAwake()
    {
        _unit = transform.GetComponent<GKUnit>();
    }

    public override void OnStart()
    {
        base.OnStart();
        _target = (GKUnit)Owner.GetVariable("AttackUnit").GetValue();
    }

    public override TaskStatus OnUpdate()
    { 
        if(null == _unit || null == _target)
			return TaskStatus.Failure;

        _unit.Attack(_target);

        return TaskStatus.Running;
    }

}