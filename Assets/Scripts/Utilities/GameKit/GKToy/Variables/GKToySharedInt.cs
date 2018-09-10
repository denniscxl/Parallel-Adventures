namespace GKToy
{
    [System.Serializable]
    public class GKToySharedInt : GKToyShardVariable<int>
    {
        public static implicit operator GKToySharedInt(int value) { return new GKToySharedInt { mValue = value }; }

        public override void SetValue(object value)
        {
            if (Value != (int)value)
            {
                ValueChanged();
                Value = (int)value;
            }
        }
    }
}