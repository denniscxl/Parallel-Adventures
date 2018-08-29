using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace GKBase
{
    static public class GKRandom
    {
        public static Quaternion rotateY
        {
            get
            {
                var v = Random.insideUnitCircle;
                return Quaternion.LookRotation(new Vector3(v.x, 0, v.y));
            }
        }

        public static T From<T>(IList<T> list)
        {
            if (list == null || list.Count == 0) return default(T);
            var a = Random.Range(0, list.Count);
            return list[a];
        }

        public static double doubleValue
        {
            get
            {
                var r = new System.Random();
                return r.NextDouble();
            }
        }
    }
}
