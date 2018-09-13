namespace GKToy
{
    [System.Serializable]
    public class GKToySharedBool : GKToyShardVariable<bool>
    {
        public static implicit operator GKToySharedBool(bool value) { return new GKToySharedBool { mValue = value }; }

        public override void SetValue(object value)
        {
            if(Value != (bool)value)
            {
                ValueChanged();
                Value = (bool)value;
            }
        }
    }
}