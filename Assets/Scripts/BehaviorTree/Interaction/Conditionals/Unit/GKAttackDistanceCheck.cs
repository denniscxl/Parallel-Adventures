using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;

[TaskDescription("攻击范围检测.")]
[TaskCategory("Unit")]
public class GKAttackDistanceCheck : Conditional 
{
    private GKUnit _unit;
    private float _distance;
    private GKUnit _target;

    public override void OnAwake()
    {
        _unit = transform.GetComponent<GKUnit>();
    }

    public override void OnStart()
    {
        base.OnStart();
        _target = (GKUnit)Owner.GetVariable("AttackUnit").GetValue();
        _distance = _unit.GetAttribute(EObjectAttr.AttackRange).ValInt;
    }

    public override TaskStatus OnUpdate()
	{
        if (null == _unit || null == _target)
            return TaskStatus.Failure;

        var value = Vector3.Distance(_target.myTransform.position, _unit.myTransform.position);

        if(value < _distance)
        {
            return TaskStatus.Success;
        }

        return TaskStatus.Failure;
	}
}
