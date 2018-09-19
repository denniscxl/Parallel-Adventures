using UnityEngine;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GKBase
{
    public static class GKReflection
    {

        static public T PtrToStructure<T>(System.IntPtr p)
        {
            return (T)Marshal.PtrToStructure(p, typeof(T));
        }

        public static System.Type FindTypeByName(string typeName)
        {
            var type = System.Type.GetType(typeName);
            if (type != null) return type;

            foreach (var a in System.AppDomain.CurrentDomain.GetAssemblies())
            {
                type = a.GetType(typeName);
                if (type != null) return type;
            }
            return null;
        }

        static public FieldInfo GetFieldFromPath(object obj, string fieldPath)
        {
            var type = obj.GetType();
            var fieldNames = fieldPath.Split('.');

            FieldInfo fi = null;

            foreach (var fn in fieldNames)
            {
                if (fi == null)
                {
                    fi = type.GetField(fn);
                }
                else
                {
                    fi = fi.FieldType.GetField(fn);
                }
                if (fi == null) return null;
            }

            return fi;
        }

        static public object GetFieldValueFromPath(object obj, string fieldPath)
        {
            var type = obj.GetType();
            var fieldNames = fieldPath.Split('.');

            FieldInfo fi = null;

            foreach (var fn in fieldNames)
            {
                if (fi == null)
                {
                    fi = type.GetField(fn);
                }
                else
                {
                    fi = fi.FieldType.GetField(fn);
                }
                if (fi == null) return null;

                obj = fi.GetValue(obj);
                if (obj == null) return null;
            }

            return obj;
        }

        static public bool SetFieldValueFromPath(object obj, string fieldPath, object val)
        {
            var type = obj.GetType();
            var fieldNames = fieldPath.Split('.');

            FieldInfo fi = null;

            for (int i = 0; i < fieldNames.Length; i++)
            {
                if (fi == null)
                {
                    fi = type.GetField(fieldNames[i]);
                }
                else
                {
                    fi = fi.FieldType.GetField(fieldNames[i]);
                }
                if (fi == null) return false;

                if (i == fieldNames.Length - 1)
                {
                    fi.SetValue(obj, val);
                    return true;
                }
                else
                {
                    obj = fi.GetValue(obj);
                    if (obj == null) return false;
                }
            }

            return false;
        }

    }
}