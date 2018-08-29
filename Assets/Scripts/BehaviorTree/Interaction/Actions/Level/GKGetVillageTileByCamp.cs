using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;
using GKMap;

[TaskCategory("Level")]
public class GKGetVillageTileByCamp : Action
{
    private CampType _camp = CampType.Blue;
    [BehaviorDesigner.Runtime.Tasks.Tooltip("是否采取随机选择.")]
    [SerializeField]
    private bool isRandom = true;

    public override void OnStart()
    {
        base.OnStart();
        _camp = (CampType)Owner.GetVariable("Camp").GetValue();
    }

    public override TaskStatus OnUpdate()
    {
        // 中立尚未初始化完成.
        if (CampType.Yellow == _camp)
            return TaskStatus.Failure;
        
        int tileIdx = -1;
        var lst  = LevelController.Instance().GetVillageTilesByCamp(_camp);
        if (null == lst)
            return TaskStatus.Failure;

        if(isRandom)
        {
            int randIdx = Random.Range(0, lst.Count);
            tileIdx = lst[randIdx];
        }

        if(-1 == tileIdx)
            return TaskStatus.Failure;

        Owner.GetVariable("VillageTileIdx").SetValue(tileIdx);

        return TaskStatus.Success;
    }
}