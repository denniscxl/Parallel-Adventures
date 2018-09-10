using UnityEngine;

namespace GKToy
{
    [System.Serializable]
    public class GKToySharedVector3 : GKToyShardVariable<Vector3>
    {
        public static implicit operator GKToySharedVector3(Vector3 value) { return new GKToySharedVector3 { mValue = value }; }

        public override void SetValue(object value)
        {
            if (Value != (Vector3)value)
            {
                ValueChanged();
                Value = (Vector3)value;
            }
        }
    }
}