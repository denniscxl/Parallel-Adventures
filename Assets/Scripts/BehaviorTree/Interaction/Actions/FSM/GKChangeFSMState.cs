using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;

[TaskDescription("修改角色有限状态机状态.")]
[TaskCategory("FSM")]
public class GKChangeFSMState : Action {

    public MachineStateID state;
    private GKUnit _unit;

	public override void OnAwake()
	{
        _unit = transform.GetComponent<GKUnit> ();
	}

	public override TaskStatus OnUpdate()
	{
        if (null == _unit)
            return TaskStatus.Failure;

        _unit.GetStateMachine().GoToState(state);
		return TaskStatus.Success;
	}
}
