using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKBase;

namespace GKData
{
    // 针对数据GKDataBase设计的序列化类.
    public class GKSerialize : GKSingleton<GKSerialize>
    {
        public string SerializeObject(GKDataBase data)
        {
            string content = "";
            foreach (var c in data.GetAttrDict().Values)
            {
                content += string.Format("{0}###{1}###{2}@@@", (int)c.type, GetCurValueToString(c), c.index);
            }
            content += "$$$";
            foreach (var c in data.GetAttrListDict().Values)
            {
                content += string.Format("{0}###{1}###{2}@@@", (int)c.type, GetCurValueToString(c), c.index);
            }
            content += "***";
            return content;
        }

        public GKDataBase DeserializeObject(string pSerizedString)
        {
            if (string.IsNullOrEmpty(pSerizedString))
                return null;

            GKDataBase dataBase = new GKDataBase();

            // 划分元素.
            string[] elements = pSerizedString.Split(new string[] { "***" }, System.StringSplitOptions.None);
            string[] types = null;
            string[] commons = null;
            string[] commonKV = null;
            string[] lists = null;
            string[] listKV = null;
            foreach (var element in elements)
            {
                if (string.IsNullOrEmpty(element))
                    continue;

                // 划分GKCommonValue & GKCommonListValue.
                types = element.Split(new string[] { "$$$" }, System.StringSplitOptions.None);

                // GKCommonValue 数据处理.
                if (!string.IsNullOrEmpty(types[0]))
                {
                    // 划分GKCommonValue数据.
                    commons = types[0].Split(new string[] { "@@@" }, System.StringSplitOptions.None);
                    foreach (var common in commons)
                    {
                        // 划分GKCommonValue Key/Value/Index.
                        commonKV = common.Split(new string[] { "###" }, System.StringSplitOptions.None);
                        if (string.IsNullOrEmpty(commonKV[0]) || string.IsNullOrEmpty(commonKV[1]) || string.IsNullOrEmpty(commonKV[2]))
                            continue;

                        SetValue(commonKV[0], commonKV[1], commonKV[2], ref dataBase);
                    }
                }
                // GKCommonListValue 数据处理.
                if (!string.IsNullOrEmpty(types[1]))
                {
                    // 划分GKCommonListValue数据.
                    lists = types[1].Split(new string[] { "@@@" }, System.StringSplitOptions.None);
                    foreach (var list in lists)
                    {
                        // 划分GKCommonListValue Key/Value/Index.
                        listKV = list.Split(new string[] { "###" }, System.StringSplitOptions.None);
                        if (string.IsNullOrEmpty(listKV[0]) || string.IsNullOrEmpty(listKV[1]) || string.IsNullOrEmpty(listKV[2]))
                            continue;

                        // 处理并生成List.
                        SetListValue(listKV[0], listKV[1], listKV[2], ref dataBase);
                    }
                }
            }

            return dataBase;
        }

        private string GetCurValueToString(GKCommonValue data)
        {
            switch (data.type)
            {
                case AttributeType.Type_Int8:
                case AttributeType.Type_Int16:
                case AttributeType.Type_Int32:
                    return data.ValInt.ToString();
                case AttributeType.Type_Int64:
                    return data.ValLong.ToString();
                case AttributeType.Type_Float:
                case AttributeType.Type_Double:
                    return data.floatValue.ToString();
                case AttributeType.Type_String:
                    return data.stringValue;
                case AttributeType.Type_Blob:
                    return data.bufferValue.ToString();
            }
            return "";
        }

        private void SetValue(string type, string value, string idx, ref GKDataBase data)
        {
            int t = int.Parse(type);
            switch (t)
            {
                case (int)AttributeType.Type_Int8:
                case (int)AttributeType.Type_Int16:
                case (int)AttributeType.Type_Int32:
                    {
                        data.GetOrCreateAttribute(int.Parse(idx)).SetValue(int.Parse(value));
                    }
                    break;
                case (int)AttributeType.Type_Int64:
                    {
                        data.GetOrCreateAttribute(int.Parse(idx)).SetValue(long.Parse(value));
                    }
                    break;
                case (int)AttributeType.Type_Float:
                case (int)AttributeType.Type_Double:
                    {
                        data.GetOrCreateAttribute(int.Parse(idx)).SetValue(float.Parse(value));
                    }
                    break;
                case (int)AttributeType.Type_String:
                    {
                        data.GetOrCreateAttribute(int.Parse(idx)).SetValue(value);
                    }
                    break;
                case (int)AttributeType.Type_Blob:
                    {
                        data.GetOrCreateAttribute(int.Parse(idx)).SetValue(System.Text.Encoding.Default.GetBytes(value));
                    }
                    break;
            }
        }

        private string GetCurValueToString(GKCommonListValue data)
        {
            string content = "";
            switch (data.type)
            {
                case AttributeType.Type_Int8:
                case AttributeType.Type_Int16:
                case AttributeType.Type_Int32:
                    foreach (var d in data.ValInt)
                    {
                        content += string.Format("{0}%%%", d);
                    }
                    break;
                case AttributeType.Type_Int64:
                    foreach (var d in data.ValLong)
                    {
                        content += string.Format("{0}%%%", d);
                    }
                    break;
                case AttributeType.Type_Float:
                case AttributeType.Type_Double:
                    foreach (var d in data.ValFloat)
                    {
                        content += string.Format("{0}%%%", d);
                    }
                    break;
                case AttributeType.Type_String:
                    foreach (var d in data.ValString)
                    {
                        content += string.Format("{0}%%%", d);
                    }
                    break;
                case AttributeType.Type_Blob:
                    foreach (var d in data.ValBuffer)
                    {
                        content += string.Format("{0}%%%", d.ToString());
                    }
                    break;
            }
            return content;
        }

        private void SetListValue(string type, string value, string idx, ref GKDataBase data)
        {
            int t = int.Parse(type);
            switch (t)
            {
                case (int)AttributeType.Type_Int8:
                case (int)AttributeType.Type_Int16:
                case (int)AttributeType.Type_Int32:
                    {
                        List<int> lst = new List<int>();

                        // 处理链表元素.
                        var array = value.Split(new string[] { "%%%" }, System.StringSplitOptions.None);
                        foreach (var str in array)
                        {
                            if (string.IsNullOrEmpty(str))
                                continue;

                            lst.Add(int.Parse(str));
                        }

                        data.GetOrCreateAttributeList(int.Parse(idx)).SetValue(lst);
                    }
                    break;
                case (int)AttributeType.Type_Int64:
                    {
                        List<long> lst = new List<long>();

                        // 处理链表元素.
                        var array = value.Split(new string[] { "%%%" }, System.StringSplitOptions.None);
                        foreach (var str in array)
                        {
                            if (string.IsNullOrEmpty(str))
                                continue;

                            lst.Add(long.Parse(str));
                        }

                        data.GetOrCreateAttributeList(int.Parse(idx)).SetValue(lst);
                    }
                    break;
                case (int)AttributeType.Type_Float:
                case (int)AttributeType.Type_Double:
                    {
                        List<float> lst = new List<float>();

                        // 处理链表元素.
                        var array = value.Split(new string[] { "%%%" }, System.StringSplitOptions.None);
                        foreach (var str in array)
                        {
                            if (string.IsNullOrEmpty(str))
                                continue;

                            lst.Add(float.Parse(str));
                        }

                        data.GetOrCreateAttributeList(int.Parse(idx)).SetValue(lst);
                    }
                    break;
                case (int)AttributeType.Type_String:
                    {
                        List<string> lst = new List<string>();

                        // 处理链表元素.
                        var array = value.Split(new string[] { "%%%" }, System.StringSplitOptions.None);
                        foreach (var str in array)
                        {
                            if (string.IsNullOrEmpty(str))
                                continue;

                            lst.Add(str);
                        }

                        data.GetOrCreateAttributeList(int.Parse(idx)).SetValue(lst);
                    }
                    break;
                case (int)AttributeType.Type_Blob:
                    {
                        List<byte[]> lst = new List<byte[]>();

                        // 处理链表元素.
                        var array = value.Split(new string[] { "%%%" }, System.StringSplitOptions.None);
                        foreach (var str in array)
                        {
                            if (string.IsNullOrEmpty(str))
                                continue;

                            lst.Add(System.Text.Encoding.Default.GetBytes(str));
                        }

                        data.GetOrCreateAttributeList(int.Parse(idx)).SetValue(lst);
                    }
                    break;
            }
        }
    }
}
