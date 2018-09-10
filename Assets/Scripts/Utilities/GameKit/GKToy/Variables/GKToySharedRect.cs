using UnityEngine;

namespace GKToy
{
    [System.Serializable]
    public class GKToySharedRect : GKToyShardVariable<Rect>
    {
        public static implicit operator GKToySharedRect(Rect value) { return new GKToySharedRect { mValue = value }; }

        public override void SetValue(object value)
        {
            if (Value != (Rect)value)
            {
                ValueChanged();
                Value = (Rect)value;
            }
        }
    }
}