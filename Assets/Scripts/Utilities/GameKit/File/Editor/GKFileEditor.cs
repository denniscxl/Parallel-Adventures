using UnityEngine;
using UnityEditor;
using System.IO;

namespace GKFile
{
    public class GKFileEditor
    {

        // Get asset bundle name by meta.
        static public string GetBundleName(string assetPath, string defualtABName = "")
        {
            string fn = GKFileUtil.GetAssetPath(assetPath);
            if (string.IsNullOrEmpty(defualtABName))
                defualtABName = Path.GetFileNameWithoutExtension(assetPath);

            AssetImporter bundleImporter = AssetImporter.GetAtPath(fn);
            if (null == bundleImporter)
            {
                Debug.LogError(string.Format("GetBundleName bundleImporter is null. AssetPath: {0}", fn));
                return defualtABName;
            }
            string bn = bundleImporter.assetBundleName;
            // If bundle name is null. Name is set to file name.
            if (string.IsNullOrEmpty(bn))
            {
                bn = defualtABName;
            }
            return bn;
        }
    }
}