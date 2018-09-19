using UnityEngine;
using UnityEditor;
using System.IO;
using GKBase;

namespace GKCompress
{
    public class GKCompress : Editor
    {
        #region publicMethod
        #endregion

        #region PrivateMethod
        [MenuItem("GK/Compress/SelectedObjectCompress")]
        static void CompressFile()
        {
            string[] SelectedPath = GKEditor.GetAssetSelectionFilePath(true, null);
            string compressName = string.Format("{0}/Compress{1}.zip", Application.dataPath, UnityEngine.Random.Range(0, 999));
            GKZipUtility.Zip(SelectedPath, compressName);
            AssetDatabase.Refresh();

        }

        [MenuItem("GK/Compress/SelectedObjectUncompress")]
        static void DecompressFile()
        {
            string[] SelectedPath = GKEditor.GetAssetSelectionFilePath(true, null);
            foreach (string p in SelectedPath)
            {
                string fName = Path.GetFileNameWithoutExtension(p);
                string zipDir = string.Format("{0}/{1}/", Application.dataPath, fName);
                GKZipUtility.UnzipFile(p, zipDir);
            }
            AssetDatabase.Refresh();
        }
        #endregion
    }
}