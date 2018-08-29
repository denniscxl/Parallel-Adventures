using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("设置指令")]
[TaskCategory("Command")]
public class GKSetCommand : Action 
{
    [SerializeField]
    private CommandType _command;

	public override TaskStatus OnUpdate()
	{
        Owner.GetVariable("Command").SetValue(_command);
        return TaskStatus.Success;
	}
}
