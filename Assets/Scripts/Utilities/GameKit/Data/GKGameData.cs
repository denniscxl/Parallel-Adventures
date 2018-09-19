using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using GKBase;

namespace GKData
{
    public class GKGameData : ScriptableObject
    {
        public void ResetDataArray<ENUM_TYPE, DATA_TYPE>(ref DATA_TYPE[] dst) where DATA_TYPE : new()
        {
            var dt = typeof(DATA_TYPE);
            var values = GK.EnumValues<ENUM_TYPE>();
            var N = values.Length;
            var newArr = new DATA_TYPE[N];

            for (int i = 0; i < N; i++)
            {
                var o = GK.SafeGetElement(dst, i);
                if (o == null) o = new DATA_TYPE();
                newArr[i] = o;
                var t = values[i];
                dt.GetField("type").SetValue(o, t);
            }
            dst = newArr;
        }

        public void ResetDataArray<DATA_TYPE>(int length, ref DATA_TYPE[] dst) where DATA_TYPE : new()
        {
            var dt = typeof(DATA_TYPE);
            var N = length;
            var newArr = new DATA_TYPE[N];

            for (int i = 0; i < N; i++)
            {
                var o = GK.SafeGetElement(dst, i);
                if (o == null) o = new DATA_TYPE();
                newArr[i] = o;
                dt.GetField("id").SetValue(o, i);
            }
            dst = newArr;
        }

        public delegate void OnValueChange<T>(T t);
    }
}
