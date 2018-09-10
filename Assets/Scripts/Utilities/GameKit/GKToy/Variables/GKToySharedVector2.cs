using UnityEngine;

namespace GKToy
{
    [System.Serializable]
    public class GKToySharedVector2 : GKToyShardVariable<Vector2>
    {
        public static implicit operator GKToySharedVector2(Vector2 value) { return new GKToySharedVector2 { mValue = value }; }

        public override void SetValue(object value)
        {
            if (Value != (Vector2)value)
            {
                ValueChanged();
                Value = (Vector2)value;
            }
        }
    }
}