using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GKBase
{
    public class GKSVN
    {

        public static void RevertFile(string path)
        {
            Debug.Log(string.Format("RevertFile Begin. path: {0}", path));

            var psi = new System.Diagnostics.ProcessStartInfo("svn", "revert " + path + " -R --non-interactive");
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;

            using (var process = System.Diagnostics.Process.Start(psi))
            {
                process.WaitForExit();
            }

            Debug.Log(string.Format("RevertFile End. path: {0}", path));
        }

    }

}
