using UnityEngine;

namespace GKToy
{
    [System.Serializable]
    public class GKToySharedLong : GKToyShardVariable<long>
    {
        public static implicit operator GKToySharedLong(long value) { return new GKToySharedLong { mValue = value }; }

        public override void SetValue(object value)
        {
            if (Value != (long)value)
            {
                ValueChanged();
                Value = (long)value;
            }
        }
    }
}