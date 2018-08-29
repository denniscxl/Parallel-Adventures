using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UI_Settings : ScriptableObject
{
    [System.Serializable]
    public class Test
    {
        public int a = 960;
        public float b = 2;
    }
    public  Test test = new Test();
}
