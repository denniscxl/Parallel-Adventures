using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GKBase
{
    static public class GKString
    {
        static public string FromList(List<string> list, string seperator)
        {
            return FromArray(list.ToArray(), seperator);
        }

        static public string FromArray(string[] arr, string seperator)
        {
            if (arr == null) return "";
            var s = "";
            int i = 0;
            foreach (var a in arr)
            {
                if (i > 0) s += seperator;
                s += a;
                i++;
            }
            return s;
        }

        static public string GetFromPrefix(string str, string prefix, bool ignoreCase)
        {
            if (!str.StartsWith(prefix, ignoreCase, null))
                return string.Empty;
            return str.Substring(prefix.Length);
        }

        static public string GetFromSuffix(string str, string suffix, bool ignoreCase)
        {
            if (!str.EndsWith(suffix, ignoreCase, null))
                return string.Empty;
            return str.Substring(0, str.Length - suffix.Length);
        }

        static public string GetToken(string str, int index, params char[] separator)
        {
            if (str == null) return null;
            var t = str.Split(separator);
            if (t == null || t.Length < index) return null;
            return t[index];
        }

        static public string Reverse(string s)
        {
            if (s == null) return null;
            char[] array = s.ToCharArray();
            System.Array.Reverse(array);
            return new string(array);
        }

        static public string MoneyToString(double v) { return MoneyToString((long)v); }
        static public string MoneyToString(long v)
        {
            //return "$" + v.ToString("#,##");
            return "$" + v.ToString();
        }
    }
}