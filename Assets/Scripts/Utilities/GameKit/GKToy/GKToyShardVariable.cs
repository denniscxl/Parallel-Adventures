using UnityEngine;

namespace GKToy
{
    public abstract class GKToyShardVariable<T> : GKToyVariable
    {
        //private Func<T> mGetter;
        //private Action<T> mSetter;

        [SerializeField]
        protected T mValue;
        public T Value
        {
            get;
            set;
        }

        public override void InitializePropertyMapping(GKToyBaseOverlord overlord)
        {
            
        }

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object value)
        {
            Value = (T)value;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}