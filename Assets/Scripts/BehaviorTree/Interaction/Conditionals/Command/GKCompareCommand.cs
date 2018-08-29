using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

[TaskDescription("判断指令是否一致.")]
[TaskCategory("Command")]
public class GKCompareCommand : Conditional 
{
    [SerializeField]
    private CommandType _type;

	public override TaskStatus OnUpdate()
	{
        if (_type == (CommandType)Owner.GetVariable("Command").GetValue())
            return TaskStatus.Success;

        return TaskStatus.Failure;
	}
}
