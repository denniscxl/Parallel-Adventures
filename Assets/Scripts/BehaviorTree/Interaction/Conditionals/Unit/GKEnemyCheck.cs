using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;

[TaskDescription("角色周围敌人检测.")]
[TaskCategory("Unit")]
public class GKEnemyCheck : Conditional 
{
    private GKUnit _unit;
    private CampType _camp = CampType.Blue;

    public override void OnAwake()
    {
        _unit = transform.GetComponent<GKUnit>();
        _camp = (CampType)_unit.GetAttribute(EObjectAttr.Camp).ValInt;
    }

	public override TaskStatus OnUpdate()
	{
        if (null == _unit)
            return TaskStatus.Failure;
        
        var lst = LevelController.Instance().GetCampSightUnit(_camp);

        // 检测阵营视野列表中非本方阵营角色.
        foreach(var u in lst)
        {
            if(null != u && u.GetAttribute(EObjectAttr.Camp).ValInt != (int)_camp)
            {
                Owner.GetVariable("AttackUnit").SetValue(u);
                Owner.GetVariable("Tile").SetValue(u.Grid);
                return TaskStatus.Success;
            }
        }

        return TaskStatus.Failure;
	}
}
