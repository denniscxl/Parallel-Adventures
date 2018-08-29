using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;

[TaskDescription("检测是否有本方角色需要支援.")]
[TaskCategory("Unit")]
public class GKSupportCheck : Conditional 
{
    private GKUnit _unit;

    public override void OnAwake()
    {
        _unit = transform.GetComponent<GKUnit>();
    }

	public override TaskStatus OnUpdate()
	{
        if (null == _unit)
            return TaskStatus.Failure;

        foreach(var guid in LevelController.Instance().GetCampData((CampType)_unit.GetAttribute(EObjectAttr.Camp).ValInt).Values)
        {
            var go = LevelController.Instance().GetTargetByID(guid);
            if (null != go)
            {
                var friend = go.GetComponent<GKUnit>();
                if (null != friend && friend.bSupport && friend != _unit)
                {
                    SetSupportUnit(friend);
                    return TaskStatus.Success;
                }
            }
        }

        SetSupportUnit(null);

        return TaskStatus.Failure;
	}

    private void SetSupportUnit(GKUnit unit)
    {
        Owner.GetVariable("SupportUnit").SetValue(unit);
    }
}
