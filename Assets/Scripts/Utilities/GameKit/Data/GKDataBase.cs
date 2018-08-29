using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKMemory;
using System.IO;

namespace GKData
{
    // 需要等到对象实例化之后再进行数据操作及事件处理.
    public class GKDataBase
    {
        #region PublicField

        #endregion

        #region PrivateField
        protected Dictionary<int, GKCommonValue> _attrDict = null;
        protected Dictionary<int, GKCommonListValue> _attrListDict = null;
        protected static List<GKCommonValue> _changedAttrList = new List<GKCommonValue>();
        protected static List<GKCommonListValue> _changedAttrListList = new List<GKCommonListValue>();
        #endregion

        #region PublicMethod
        public GKDataBase()
        {
            InitAttrDict();
        }

        public Dictionary<int, GKCommonValue> GetAttrDict()
        {
            return _attrDict;
        }

        public Dictionary<int, GKCommonListValue> GetAttrListDict()
        {
            return _attrListDict;
        }

        // 回收所有属性对象到对象池中.
        virtual public void RecycleAllAttribute()
        {
            Dictionary<int, GKCommonValue>.Enumerator it = _attrDict.GetEnumerator();
            while(it.MoveNext())
            {
                GKCommonValue attr = it.Current.Value;
                if(null != attr)
                {
                    attr.Clear();
                    GKCommonValue.commonValuePool.Recycle(attr);
                }
            }
            _attrDict.Clear();

            Dictionary<int, GKCommonListValue>.Enumerator it_ = _attrListDict.GetEnumerator();
            while (it.MoveNext())
            {
                GKCommonListValue attr = it_.Current.Value;
                if (null != attr)
                {
                    attr.Clear();
                    GKCommonListValue.commonValuePool.Recycle(attr);
                }
            }
            _attrListDict.Clear();
        }

        // 清空所有属性值, 绑定事件不清楚. (用途类似游戏中断线重连等)
        virtual public void ClearAllAttributeValue()
        {
            Dictionary<int, GKCommonValue>.Enumerator it = _attrDict.GetEnumerator();
            while(it.MoveNext())
            {
                GKCommonValue attr = it.Current.Value;
                if(null != attr)
                {
                    attr.ClearValueWithOutEvent();
                }
            }

            Dictionary<int, GKCommonListValue>.Enumerator it_ = _attrListDict.GetEnumerator();
            while (it_.MoveNext())
            {
                GKCommonListValue attr = it_.Current.Value;
                if (null != attr)
                {
                    attr.ClearValueWithOutEvent();
                }
            }
        }

        virtual public GKCommonValue GetOrCreateAttribute(int idx)
        {
            GKCommonValue attr = null;
            if(!_attrDict.TryGetValue(idx, out attr) || null == attr)
            {
                attr = GKCommonValue.commonValuePool.Spawn(true);
                // 如果取出来的对象存在监听, 意味着对象仍旧存在外部代理或同一个对象呗多次放回内存池中.
                // 为了避免意外情况发生, 抛弃此对象使用. 重新获取.
                while(attr.HasEvent())
                {
                    Debug.LogWarning(string.Format("GetOrCreateAttribute target still has event. target: {0}", attr.GetEventTarget()));
                    attr = GKCommonValue.commonValuePool.Spawn(true);
                }
                attr.index = idx;
                _attrDict[idx] = attr;
            }
            return attr;
        }

        virtual public GKCommonListValue GetOrCreateAttributeList(int idx)
        {
            GKCommonListValue attr = null;
            if (!_attrListDict.TryGetValue(idx, out attr) || null == attr)
            {
                attr = GKCommonListValue.commonValuePool.Spawn(true);
                // 如果取出来的对象存在监听, 意味着对象仍旧存在外部代理或同一个对象呗多次放回内存池中.
                // 为了避免意外情况发生, 抛弃此对象使用. 重新获取.
                while (attr.HasEvent())
                {
                    Debug.LogWarning(string.Format("GetOrCreateAttribute target still has event. target: {0}", attr.GetEventTarget()));
                    attr = GKCommonListValue.commonValuePool.Spawn(true);
                }
                attr.index = idx;
                _attrListDict[idx] = attr;
            }
            return attr;
        }


        virtual public GKCommonValue GetAttribute(int idx)
        {
            return GetOrCreateAttribute(idx);
        }

        virtual public GKCommonListValue GetAttributeList(int idx)
        {
            return GetOrCreateAttributeList(idx);
        }

        // 设置属性. 此系列函数接口调用频繁, 为增加运行效率, 故牺牲代码美观, 重复部分相似代码.
        public GKCommonValue SetAttribute(int idx, int value, bool bDoEvent)
        {
            GKCommonValue attr = GetOrCreateAttribute((int)idx);
            if(null == attr)
            {
                return null;
            }
            attr.SetValue(value);
            if(bDoEvent)
            {
                attr.DoEvent(this);
            }
            return attr;
        }
        public GKCommonListValue SetAttributeList(int idx, List<int> value, bool bDoEvent)
        {
            GKCommonListValue attr = GetOrCreateAttributeList((int)idx);
            if (null == attr)
            {
                return null;
            }
            attr.SetValue(value);
            if (bDoEvent)
            {
                attr.DoEvent(this);
            }
            return attr;
        }
        public GKCommonListValue SetAttributeList(int idx, int value, bool bDoEvent, bool bRemove = false)
        {
            GKCommonListValue attr = GetOrCreateAttributeList((int)idx);
            if (null == attr)
            {
                return null;
            }
            if(!bRemove)
            {
                attr.AddValue(value);
            }
            else
            {
                attr.RemoveValue(value);
            }

            if (bDoEvent)
            {
                attr.DoEvent(this);
            }
            return attr;
        }

        public GKCommonValue SetAttribute(int idx, long value, bool bDoEvent)
        {
            GKCommonValue attr = GetOrCreateAttribute((int)idx);
            if (null == attr)
            {
                return null;
            }
            attr.SetValue(value);
            if (bDoEvent)
            {
                attr.DoEvent(this);
            }
            return attr;
        }
        public GKCommonListValue SetAttributeList(int idx, List<long> value, bool bDoEvent)
        {
            GKCommonListValue attr = GetOrCreateAttributeList((int)idx);
            if (null == attr)
            {
                return null;
            }
            attr.SetValue(value);
            if (bDoEvent)
            {
                attr.DoEvent(this);
            }
            return attr;
        }
        public GKCommonListValue SetAttributeList(int idx, long value, bool bDoEvent, bool bRemove = false)
        {
            GKCommonListValue attr = GetOrCreateAttributeList((int)idx);
            if (null == attr)
            {
                return null;
            }
            if (!bRemove)
            {
                attr.AddValue(value);
            }
            else
            {
                attr.RemoveValue(value);
            }

            if (bDoEvent)
            {
                attr.DoEvent(this);
            }
            return attr;
        }

        public GKCommonValue SetAttribute(int idx, float value, bool bDoEvent)
        {
            GKCommonValue attr = GetOrCreateAttribute((int)idx);
            if (null == attr)
            {
                return null;
            }
            attr.SetValue(value);
            if (bDoEvent)
            {
                attr.DoEvent(this);
            }
            return attr;
        }
        public GKCommonListValue SetAttributeList(int idx, List<float> value, bool bDoEvent)
        {
            GKCommonListValue attr = GetOrCreateAttributeList((int)idx);
            if (null == attr)
            {
                return null;
            }
            attr.SetValue(value);
            if (bDoEvent)
            {
                attr.DoEvent(this);
            }
            return attr;
        }
        public GKCommonListValue SetAttributeList(int idx, float value, bool bDoEvent, bool bRemove = false)
        {
            GKCommonListValue attr = GetOrCreateAttributeList((int)idx);
            if (null == attr)
            {
                return null;
            }
            if (!bRemove)
            {
                attr.AddValue(value);
            }
            else
            {
                attr.RemoveValue(value);
            }

            if (bDoEvent)
            {
                attr.DoEvent(this);
            }
            return attr;
        }

        public GKCommonValue SetAttribute(int idx, string value, bool bDoEvent)
        {
            GKCommonValue attr = GetOrCreateAttribute((int)idx);
            if (null == attr)
            {
                return null;
            }
            attr.SetValue(value);
            if (bDoEvent)
            {
                attr.DoEvent(this);
            }
            return attr;
        }
        public GKCommonListValue SetAttributeList(int idx, List<string> value, bool bDoEvent)
        {
            GKCommonListValue attr = GetOrCreateAttributeList((int)idx);
            if (null == attr)
            {
                return null;
            }
            attr.SetValue(value);
            if (bDoEvent)
            {
                attr.DoEvent(this);
            }
            return attr;
        }
        public GKCommonListValue SetAttributeList(int idx, string value, bool bDoEvent, bool bRemove = false)
        {
            GKCommonListValue attr = GetOrCreateAttributeList((int)idx);
            if (null == attr)
            {
                return null;
            }
            if (!bRemove)
            {
                attr.AddValue(value);
            }
            else
            {
                attr.RemoveValue(value);
            }

            if (bDoEvent)
            {
                attr.DoEvent(this);
            }
            return attr;
        }

        public GKCommonValue CopyAttribute(GKCommonValue src, GKCommonValue dest, bool bDoEvent)
        {
            dest.CopyVale(src);
            if(bDoEvent)
            {
                dest.DoEvent(this);
            }
            return dest;
        }
        public GKCommonListValue CopyAttribute(GKCommonListValue src, GKCommonListValue dest, bool bDoEvent)
        {
            dest.CopyVale(src);
            if (bDoEvent)
            {
                dest.DoEvent(this);
            }
            return dest;
        }
        public GKCommonValue CopyAttribute(int idx, GKCommonValue src, bool bDoEvent)
        {
            GKCommonValue attr = GetOrCreateAttribute((int)idx);
            if (null == attr)
                return null;
            return CopyAttribute(attr, src, bDoEvent);
        } 
        public GKCommonListValue CopyAttribute(int idx, GKCommonListValue src, bool bDoEvent)
        {
            GKCommonListValue attr = GetOrCreateAttributeList((int)idx);
            if (null == attr)
                return null;
            return CopyAttribute(attr, src, bDoEvent);
        } 

        // 网络传输过程使用字节流或Protobuff来实现. 目前暂订自定义结构体.
        // 为将来实现网络同步流出接口.
        // 实现网络传输需重构 ParseAttrbute 及 ReadAttribute.
        public void ParseAttrbute(List<AttributeInfo> list)
        {
            int count = list.Count;
            // 数据异常.
            if (count < 0 || count > 1000)
                return;
            for (int i = 0; i < count; i++)
            {
                ReadAttribute(list[i]);
            }
            DoAttrChangeEvent();
        }
        #endregion

        #region PrivateMethod
        virtual protected void InitAttrDict()
        {
            _attrDict = new Dictionary<int, GKCommonValue>();
            _attrListDict = new Dictionary<int, GKCommonListValue>();
        }

        protected void DoAttrChangeEvent()
        {
            for (int i = 0; i < _changedAttrList.Count; i++)
            {
                _changedAttrList[i].DoEvent(this);
            }
            _changedAttrList.Clear();
        }
        virtual protected void ReadAttribute(AttributeInfo data)
        {
            var attr = GetOrCreateAttribute(data.index);
            if(null != attr)
            {
                switch ((AttributeType)data.type)
                {
                    case AttributeType.Type_Int8:
                    case AttributeType.Type_Int16:
                    case AttributeType.Type_Int32:
                    case AttributeType.Type_Int64:
                        {
                            int tval = (int)data.value;
                            attr.SetValue(tval);
                        }
                        break;
                    case AttributeType.Type_Float:
                    case AttributeType.Type_Double:
                        {
                            float tval = (float)data.value;
                            attr.SetValue(tval);
                        }
                        break;
                    case AttributeType.Type_String:
                        {
                            string tval = (string)data.value;
                            attr.SetValue(tval);
                        }
                        break;
                    case AttributeType.Type_Blob:
                        {
                            byte[] tval = (byte[])data.value;
                            attr.SetValue(tval);
                        }
                        break;
                    default:
                        Debug.LogWarning(string.Format("Set unknow value type. type: {0}", data.type));
                        break;
                }
            }
            if(null != attr && attr.HasEvent())
            {
                _changedAttrList.Add(attr);
            }
        }
        #endregion
    }

    public class AttributeInfo
    {
        public int index;
        public int type;
        public object value;
    }

}
