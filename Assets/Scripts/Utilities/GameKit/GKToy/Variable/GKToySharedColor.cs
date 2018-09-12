using UnityEngine;

namespace GKToy
{
    [System.Serializable]
    public class GKToySharedColor : GKToyShardVariable<Color>
    {
        public static implicit operator GKToySharedColor(Color value) { return new GKToySharedColor { mValue = value }; }

        public override void SetValue(object value)
        {
            if (Value != (Color)value)
            {
                ValueChanged();
                Value = (Color)value;
            }
        }
    }
}