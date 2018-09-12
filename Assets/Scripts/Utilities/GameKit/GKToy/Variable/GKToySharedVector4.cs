using UnityEngine;

namespace GKToy
{
    [System.Serializable]
    public class GKToySharedVector4 : GKToyShardVariable<Vector4>
    {
        public static implicit operator GKToySharedVector4(Vector4 value) { return new GKToySharedVector4 { mValue = value }; }

        public override void SetValue(object value)
        {
            if (Value != (Vector4)value)
            {
                ValueChanged();
                Value = (Vector4)value;
            }
        }
    }
}