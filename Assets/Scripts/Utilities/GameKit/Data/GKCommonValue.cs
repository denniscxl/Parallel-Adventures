using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKMemory;
using System;

namespace GKData
{
    /// <summary>
    /// Common data structure.
    /// </summary>
    public class GKCommonValue
    {
        #region PublicField
        // Attribute purpose index. 
        public int index = 0;
        // Attribute type.
        public AttributeType type = AttributeType.Type_NoSet;
        public static GKObjectPool<GKCommonValue> commonValuePool = GKMemoryController.Instance().GetOrCreateObjectPool<GKCommonValue>(-1, true);
        // 属性变更回调.
        public delegate void OnAttributChanged(object obj, GKCommonValue attr);
        public event OnAttributChanged OnAttrbutChangedEvent = null;
        #endregion

        #region PrivateField
        // Current value.
        public long longValue = 0;
        public float floatValue = 0f;
        public string stringValue = null;
        public byte[] bufferValue = null;
        // last value.
        public GKCommonValue lastValue = null;
        #endregion

        #region PublicMethod
        // Get Current valuw.
        public int ValInt
        {
            get
            {
                return (int)longValue;
            }
        }
        public long ValLong
        {
            get
            {
                return longValue;
            }
        }
        public float ValFloat
        {
            get
            {
                return floatValue;
            }
        }
        public string ValString
        {
            get
            {
                return (null == stringValue) ? string.Empty : stringValue;
            }
        }
        public byte[] ValBuffer
        {
            get
            {
                return bufferValue;
            }
        }
        // Get last value.
        public int LastValInt
        {
            get
            {
                return (null == lastValue) ? 0 : lastValue.ValInt;
            }
        }
        public long LastValLong
        {
            get
            {
                return (null == lastValue) ? 0 : lastValue.ValLong;
            }
        }
        public float LastValFloat
        {
            get
            {
                return (null == lastValue) ? 0f : lastValue.ValFloat;
            }
        }
        public string LastValString
        {
            get
            {
                return (null == lastValue) ? string.Empty : lastValue.ValString;
            }
        }
        public byte[] LastValBuffer
        {
            get
            {
                return (null == lastValue) ? null : lastValue.ValBuffer;
            }
        }
        // 以下代码部分代码重复性高, 因为代用频率高. 为了性能， 牺牲部分美观.
        public void SetValue(int newValue)
        {
            type = AttributeType.Type_Int32;
            if(null != OnAttrbutChangedEvent)
            {
                if(null == lastValue)
                {
                    lastValue = commonValuePool.Spawn(true);
                }
                lastValue.longValue = longValue;
            }
            longValue = newValue;
        }
        public void SetValue(long newValue)
        {
            type = AttributeType.Type_Int64;
            if (null != OnAttrbutChangedEvent)
            {
                if (null == lastValue)
                {
                    lastValue = commonValuePool.Spawn(true);
                }
                lastValue.longValue = longValue;
            }
            longValue = newValue;
        }
        public void SetValue(float newValue)
        {
            type = AttributeType.Type_Float;
            if (null != OnAttrbutChangedEvent)
            {
                if (null == lastValue)
                {
                    lastValue = commonValuePool.Spawn(true);
                }
                lastValue.floatValue = floatValue;
            }
            floatValue = newValue;
        }
        public void SetValue(string newValue)
        {
            type = AttributeType.Type_String;
            if (null != OnAttrbutChangedEvent)
            {
                if (null == lastValue)
                {
                    lastValue = commonValuePool.Spawn(true);
                }
                lastValue.stringValue = stringValue;
            }
            stringValue = newValue;
        }
        public void SetValue(byte[] newValue)
        {
            type = AttributeType.Type_Blob;
            if (null != OnAttrbutChangedEvent)
            {
                if (null == lastValue)
                {
                    lastValue = commonValuePool.Spawn(true);
                }
                lastValue.bufferValue = bufferValue;
            }
            bufferValue = newValue;
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
            longValue = 0;
            floatValue = 0f;
            stringValue = null;
            bufferValue = null;
            if (null != lastValue)
            {
                lastValue.Clear();
                commonValuePool.Recycle(lastValue);
                lastValue = null;
            }
        }
        public void CopyVale(GKCommonValue src, bool bDoEvent = false)
        {
            if(null != OnAttrbutChangedEvent)
            {
                if(null == lastValue)
                {
                    lastValue = commonValuePool.Spawn(true);
                }
                lastValue.longValue = longValue;
                lastValue.floatValue = floatValue;
                lastValue.stringValue = stringValue;
                lastValue.bufferValue = bufferValue;
            }
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
}
