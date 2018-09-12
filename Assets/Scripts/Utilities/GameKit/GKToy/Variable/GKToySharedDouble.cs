using UnityEngine;

namespace GKToy
{
    [System.Serializable]
    public class GKToySharedDouble : GKToyShardVariable<double>
    {
        public static implicit operator GKToySharedDouble(double value) { return new GKToySharedDouble { mValue = value }; }

        public override void SetValue(object value)
        {
            if (Value != (double)value)
            {
                ValueChanged();
                Value = (double)value;
            }
        }
    }
}