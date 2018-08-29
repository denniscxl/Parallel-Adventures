using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;

[TaskDescription("判断血量是否在阈值之上. 阈值为百分比.")]
[TaskCategory("Unit")]
public class GKHpCheck : Conditional 
{
    [SerializeField]
    private float _threshold = 100;
    private GKUnit _unit;

    public override void OnAwake()
    {
        _unit = transform.GetComponent<GKUnit>();
    }

	public override TaskStatus OnUpdate()
	{
        if (null == _unit)
            return TaskStatus.Failure;

        float curVal = (float) _unit.GetAttribute(EObjectAttr.Hp).ValInt / _unit.GetAttribute(EObjectAttr.MaxHp).ValInt;
        curVal = curVal * 100;
        //Debug.Log(string.Format("curVal: {0}, Hp:{1}, MaxHp:{2}", curVal, _unit.GetAttribute(EObjectAttr.Hp).ValInt, _unit.GetAttribute(EObjectAttr.MaxHp).ValInt));
        if (curVal > _threshold)
            return TaskStatus.Failure;

        return TaskStatus.Success;
	}
}
