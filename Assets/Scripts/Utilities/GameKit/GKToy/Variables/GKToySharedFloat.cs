namespace GKToy
{
    [System.Serializable]
    public class GKToySharedFloat : GKToyShardVariable<float>
    {
        public static implicit operator GKToySharedFloat(float value) { return new GKToySharedFloat { mValue = value }; }

        public override void SetValue(object value)
        {
            if (Value != (float)value)
            {
                ValueChanged();
                Value = (float)value;
            }
        }
    }
}