using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;
using GKMap;
using GKFOW;

[TaskDescription("判断未探索地块是否在阈值之上. 阈值为百分比.")]
[TaskCategory("Unit")]
public class GKExploredCheck : Conditional 
{
    [SerializeField]
    private float _threshold = 100;
    private GKUnit _unit;
    private int _camp = 0;
    private int _unDiscoverCount = 0;
    private int _size = 0;

    public override void OnAwake()
    {
        _unit = transform.GetComponent<GKUnit>();
        _camp = _unit.GetAttribute(EObjectAttr.Camp).ValInt;
        _unDiscoverCount = FOW.Instance().GetUnDiscoverLst(_camp).Count;
        _size = GKMapManager.GetTileCount(PolygonType.Hexagon, MyGame.Instance.MapSize);
    }

	public override TaskStatus OnUpdate()
	{
        if (null == _unit)
            return TaskStatus.Failure;

        float curVal = (float) _unDiscoverCount / _size;
        curVal = curVal * 100;
        if (curVal > _threshold)
            return TaskStatus.Success;

        return TaskStatus.Failure;
	}
}
