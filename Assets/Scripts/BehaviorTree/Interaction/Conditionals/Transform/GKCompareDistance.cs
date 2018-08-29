using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("判断你是否在有效距离内或外.")]
[TaskCategory("Transform")]
public class GKCompareDistance : Conditional 
{
    [SerializeField]
    private float _distance;
    [SerializeField]
    private bool _withInScopeOf = true;
    private Transform _target;

	public override TaskStatus OnUpdate()
	{
        if (null == _target)
            return TaskStatus.Failure;

        if(_withInScopeOf)
        {
            if (Vector3.Distance(transform.position, _target.position) < _distance)
                return TaskStatus.Success;
        }
        else
        {
            if (Vector3.Distance(transform.position, _target.position) > _distance)
                return TaskStatus.Success;
        }

        return TaskStatus.Failure;
	}
}
