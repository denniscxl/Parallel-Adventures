using UnityEngine;
using UnityEditor;

namespace GKToy
{
    [System.Serializable]
    public abstract class GKToyShardVariable<T> : GKToyVariable
    {

        [SerializeField]
        protected T mValue;
        public T Value
        {
            get { return mValue; }
            set { mValue = value; }
        }

        public override object GetValue()
        {
            return Value;
        }

        public override void SetValue(object value)
        {
            Value = (T)value;
            ValueChanged();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}