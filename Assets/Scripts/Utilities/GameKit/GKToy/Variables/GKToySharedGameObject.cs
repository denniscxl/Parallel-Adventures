using UnityEngine;

namespace GKToy
{
    [System.Serializable]
    public class GKToySharedGameObject : GKToyShardVariable<GameObject>
    {
        public static implicit operator GKToySharedGameObject(GameObject value) { return new GKToySharedGameObject { mValue = value }; }

        public override void SetValue(object value)
        {
            if (Value != (GameObject)value)
            {
                ValueChanged();
                Value = (GameObject)value;
            }
        }
    }
}