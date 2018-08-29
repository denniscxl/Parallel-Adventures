using UnityEngine;

namespace BehaviorDesigner.Runtime.Tasks.Basic.UnityPlayerPrefs
{
    [TaskCategory("Basic/PlayerPrefs")]
    [TaskDescription("Deletes the specified key from the PlayerPrefs.")]
    public class DeleteKey : Action
    {
        [Tooltip("The key to delete")]
        public SharedString key;

        public override TaskStatus OnUpdate()
        {
            PlayerPrefs.DeleteKey(key.Value);

            return TaskStatus.Success;
        }

        public override void OnReset()
        {
            key = "";
        }
    }
}