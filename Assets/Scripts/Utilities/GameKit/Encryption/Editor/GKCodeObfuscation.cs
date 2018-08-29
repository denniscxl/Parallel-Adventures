using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using GKFile;

namespace GKEncryption
{
    public class GKCodeObfuscation
    {

        #region PublicMethod
        [MenuItem("GK/Obfuscation/Scoure code obfuscation")]
        static private void MenuItem_ScoureCodeObfuscation()
        {
            if (EditorUtility.DisplayDialog("Obfuscation", "Do you want Scoure code obfuscation?", "Of course", "No, Thanks"))
            {
                Debug.Log("Scoure code obfuscation.");
                List<string> files = new List<string>();
                // Custom path.
                files.Add(string.Format("{0}/Scripts/TestScripts", Application.dataPath));
                ScoureCodeObfusation(files.ToArray());
            }
            else
            {
                Debug.Log("Cancel.");
            }
        }
        #endregion

        #region PublicMethod
        /*
         * Depending on your writing habits, 
         * You need to develop various parsing methods that only implement simple parsing functions.
         * */
        static public void ScoureCodeObfusation(string[] paths)
        {
            // One by one get directory.
            for (int i = 0; i < paths.Length; i++)
            {

                Debug.Log(string.Format("ScoureCodeObfusation file path: {0}", paths[i]));

                if (string.IsNullOrEmpty(paths[i]))
                    continue;

                List<string> list = new List<string>();
                string appendLine = "\tprivate void ObfuscationMethod_" + UnityEngine.Random.Range(0, 1000).ToString() + "(){} // For code obfuscation.";

                // Get files.
                var fs = GKFileUtil.GetFilesInDirectory(paths[i]);
                foreach (var f in fs)
                {

                    if (!GKFileUtil.FilterInvalidFiles(f))
                        continue;

                    bool bAppend = false;
                    bool bFinished = false;
                    list.Clear();
                    string strLine = "";

                    StreamReader sr = new StreamReader(f);

                    while (null != (strLine = sr.ReadLine()))
                    {

                        list.Add(strLine);

                        if (!bFinished)
                        {
                            // The new line contains '{', and meets the additional conditions.
                            if (bAppend && strLine.Contains("{"))
                            {
                                list.Add(appendLine);
                                bAppend = false;
                                bFinished = true;
                            }
                            else
                            {
                                // At the beginning of the line is not to judge or / / / *, *.
                                if (string.IsNullOrEmpty(strLine) || strLine.Trim().StartsWith("//")
                                    || strLine.Trim().StartsWith("/*") || strLine.Trim().StartsWith("*"))
                                {
                                    continue;
                                }

                                //	Check the read result and if it don't contains '{' lineNum++.
                                if (strLine.Contains("public class"))
                                {
                                    if (strLine.Contains("{"))
                                    {
                                        list.Add(appendLine);
                                        bAppend = false;
                                        bFinished = true;
                                    }
                                    else
                                    {
                                        bAppend = true;
                                    }
                                }

                                //...
                            }
                        }
                    }
                    sr.Close();

                    StreamWriter sw = new StreamWriter(f, false);
                    foreach (var l in list)
                    {
                        sw.WriteLine(l);
                    }
                    sw.Close();
                }
            }
        }
        #endregion
    }
}