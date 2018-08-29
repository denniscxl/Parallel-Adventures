using UnityEngine;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using GKRole;

[TaskDescription("判断是否在视野范围内.")]
[TaskCategory("Unit")]
public class GKWithinSight : Conditional
{
    // How wide of an angle the object can see
    public float fieldOfViewAngle;
    // The tag of the targets
    public string targetTag;
    // Set the target variable when a target has been found so the subsequent tasks know which object is the target
    public SharedTransform target;

    // A cache of all of the possible targets
    private Transform[] possibleTargets;

	private float cycleTime = 3;
	private float lastTime = 0;

    public override void OnAwake()
    {
		Init ();
    }

    public override TaskStatus OnUpdate()
    {
		if (Time.realtimeSinceStartup - lastTime > cycleTime) 
		{
			lastTime = Time.realtimeSinceStartup;
			Init ();
		}

        // Return success if a target is within sight
        for (int i = 0; i < possibleTargets.Length; ++i)
        {
            if (withinSight(possibleTargets[i], fieldOfViewAngle))
            {
                // Set the target so other tasks will know which transform is within sight
                target.Value = possibleTargets[i];
				Owner.SetVariableValue ("target", target.Value);

                return TaskStatus.Success;
            }
        }
        return TaskStatus.Failure;
    }

    // Returns true if targetTransform is within sight of current transform
    public bool withinSight(Transform targetTransform, float fieldOfViewAngle)
    {
        Vector3 direction = targetTransform.position - transform.position;
        // An object is within sight if the angle is less than field of view
        return Vector3.Angle(direction, transform.forward) < fieldOfViewAngle; 
    }

	private void Init()
	{
		// Cache all of the transforms that have a tag of targetTag
		var targets = GameObject.FindGameObjectsWithTag(targetTag);
		possibleTargets = new Transform[targets.Length];
		for (int i = 0; i < targets.Length; ++i)
		{
			possibleTargets[i] = targets[i].transform;
		}
	}
}