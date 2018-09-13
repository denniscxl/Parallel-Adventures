namespace GKToy
{
    [System.Serializable]
    public class GKToySharedString : GKToyShardVariable<string>
    {
        public static implicit operator GKToySharedString(string value) { return new GKToySharedString { mValue = value }; }

        public override void SetValue(object value)
        {
            if(Value != (string)value)
            {
                ValueChanged();
                Value = (string)value;
            }
        }
    }
}