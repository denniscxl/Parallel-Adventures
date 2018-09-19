using System;
using System.Diagnostics;
using System.Collections.Generic;
using UnityEngine;

namespace GKPlatform
{
    public class GameCommand : MonoBehaviour
    {
        static public void GetParameters(ref bool bFlag)
        {
            string[] args = System.Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; ++i)
            {
                var arg = args[i];
                if (arg == "flag")
                {
                    bFlag = true;
                    break;
                }
            }
        }
    }
}