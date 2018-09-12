using System;

namespace GKToy
{
    [System.Serializable]
    public class GKToySharedEnum : GKToyShardVariable<Enum>
    {
        public static implicit operator GKToySharedEnum(Enum value) { return new GKToySharedEnum { mValue = value }; }

        public override void SetValue(object value)
        {
            if (Value != (Enum)value)
            {
                ValueChanged();
                Value = (Enum)value;
            }
        }
    }
}