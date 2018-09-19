using System.Collections.Generic;
using GKMemory;
using System;

namespace GKData
{
    /// <summary>
    /// Common data structure.
    /// </summary>
    public class GKCommonListValue
    {
        #region PublicField
        // Attribute purpose index. 
        public int index = 0;
        // Attribute type.
        public AttributeType type = AttributeType.Type_NoSet;
        public static GKObjectPool<GKCommonListValue> commonValuePool = GKMemoryController.Instance().GetOrCreateObjectPool<GKCommonListValue>(-1, true);
        // 属性变更回调.
        public delegate void OnAttributChanged(object obj, GKCommonListValue attr);
        public event OnAttributChanged OnAttrbutChangedEvent = null;
        #endregion

        #region PrivateField
        // Current value.
        protected List<int> intValue = null;
        protected List<long> longValue = null;
        protected List<float> floatValue = null;
        protected List<string> stringValue = null;
        protected List<byte[]> bufferValue = null;
        #endregion

        #region PublicMethod
        // Get Current valuw.
        public List<int> ValInt
        {
            get
            {
                if(null == intValue)
                    intValue = new List<int>();
                return intValue;
            }
        }
        public List<long> ValLong
        {
            get
            {
                if (null == longValue)
                    longValue = new List<long>();
                return longValue;
            }
        }
        public List<float> ValFloat
        {
            get
            {
                if (null == floatValue)
                    floatValue = new List<float>();
                return floatValue;
            }
        }
        public List<string> ValString
        {
            get
            {
                if (null == stringValue)
                    stringValue = new List<string>();
                return stringValue;
            }
        }
        public List<byte[]> ValBuffer
        {
            get
            {
                if (null == bufferValue)
                    bufferValue = new List<byte[]>();
                return bufferValue;
            }
        }
        // 以下代码部分代码重复性高, 因为代用频率高. 为了性能， 牺牲部分美观.
        public void SetValue(List<int> newValue)
        {
            type = AttributeType.Type_Int32;
            intValue = newValue;
        }
        public void AddValue(int newValue)
        {
            type = AttributeType.Type_Int32;
            if (null == longValue)
                longValue = new List<long>();
            longValue.Add(newValue);
        }
        public void RemoveValue(int newValue)
        {
            type = AttributeType.Type_Int32;
            if (null == longValue || !longValue.Contains(newValue))
                return;
            longValue.Remove(newValue);
        }

        public void SetValue(List<long> newValue)
        {
            type = AttributeType.Type_Int64;
            longValue = newValue;
        }
        public void AddValue(long newValue)
        {
            type = AttributeType.Type_Int64;
            if (null == longValue)
                longValue = new List<long>();
            longValue.Add(newValue);
        }
        public void RemoveValue(long newValue)
        {
            type = AttributeType.Type_Int64;
            if (null == longValue || !longValue.Contains(newValue))
                return;
            longValue.Remove(newValue);
        }


        public void SetValue(List<float> newValue)
        {
            type = AttributeType.Type_Float;
            floatValue = newValue;
        }
        public void AddValue(float newValue)
        {
            type = AttributeType.Type_Float;
            if (null == floatValue)
                floatValue = new List<float>();
            floatValue.Add(newValue);
        }
        public void RemoveValue(float newValue)
        {
            type = AttributeType.Type_Float;
            if (null == floatValue || !floatValue.Contains(newValue))
                return;
            floatValue.Remove(newValue);
        }


        public void SetValue(List<string> newValue)
        {
            type = AttributeType.Type_String;
            stringValue = newValue;
        }
        public void AddValue(string newValue)
        {
            type = AttributeType.Type_String;
            if (null == stringValue)
                stringValue = new List<string>();
            stringValue.Add(newValue);
        }
        public void RemoveValue(string newValue)
        {
            type = AttributeType.Type_String;
            if (null == stringValue || !stringValue.Contains(newValue))
                return;
            stringValue.Remove(newValue);
        }


        public void SetValue(List<byte[]> newValue)
        {
            type = AttributeType.Type_Blob;
            bufferValue = newValue;
        }
        public void AddValue(byte[] newValue)
        {
            type = AttributeType.Type_Blob;
            if (null == bufferValue)
                bufferValue = new List<byte[]>();
            bufferValue.Add(newValue);
        }
        public void RemoveValue(byte[] newValue)
        {
            type = AttributeType.Type_Blob;
            if (null == bufferValue || !bufferValue.Contains(newValue))
                return;
            bufferValue.Remove(newValue);
        }


        public void Clear()
        {
            ClearValueWithOutEvent();
            OnAttrbutChangedEvent = null;
            type = AttributeType.Type_NoSet;
        }
        // 只清除数据, 不清除事件. 调用频繁, 故使用新函数而不拓展参数.
        public void ClearValueWithOutEvent()
        {
            longValue = null;
            floatValue = null;
            stringValue = null;
            bufferValue = null;
        }
        public void CopyVale(GKCommonListValue src, bool bDoEvent = false)
        {
            if(null != src)
            {
                longValue = src.longValue;
                floatValue = src.floatValue;
                stringValue = src.stringValue;
                bufferValue = src.bufferValue;
                type = src.type;
            }
        }
        public void DoEvent(object obj)
        {
            if(null != OnAttrbutChangedEvent)
            {
                try{
                    OnAttrbutChangedEvent(obj, this);
                }
                catch(System.Exception)
                {
                    
                }
            }
        }
        public bool HasEvent()
        {
            return (null != OnAttrbutChangedEvent);
        }
        public void ClearEvent()
        {
            OnAttrbutChangedEvent = null;
        }
        public string GetEventTarget()
        {
            if(null != OnAttrbutChangedEvent)
            {
                Delegate[] eventList = OnAttrbutChangedEvent.GetInvocationList();
                if(null != eventList)
                {
                    string result = string.Empty;
                    for (int i = 0, iCount = eventList.Length; i < iCount; i++)
                    {
                        Delegate oneEvent = eventList[i];
                        result += oneEvent.Target + ":" + oneEvent.Method + "\r\n";
                    }
                    return result;
                }
            }
            return string.Empty;
        }

        #endregion

        #region PrivateMethod

        #endregion
    }

    public enum AttributeType
    {
        Type_NoSet      = -2,
        Type_Invalid    = -1,
        Type_Blob       = 0,
        Type_Int8,
        Type_Int16,
        Type_Int32,
        Type_Int64,
        Type_Float,
        Type_Double,
        Type_String
    }
}
