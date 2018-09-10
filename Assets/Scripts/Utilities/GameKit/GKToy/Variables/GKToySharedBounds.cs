using UnityEngine;

namespace GKToy
{
    [System.Serializable]
    public class GKToySharedBounds : GKToyShardVariable<Bounds>
    {
        public static implicit operator GKToySharedBounds(Bounds value) { return new GKToySharedBounds { mValue = value }; }

        public override void SetValue(object value)
        {
            if (Value != (Bounds)value)
            {
                ValueChanged();
                Value = (Bounds)value;
            }
        }
    }
}