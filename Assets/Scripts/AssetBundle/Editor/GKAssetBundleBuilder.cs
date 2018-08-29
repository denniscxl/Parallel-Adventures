using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using GKCompress;
using GKFile;

public class GKAssetBundleBuilder
{
    #region PublicField
    #endregion

    #region PrivateField
    #endregion

    #region PublicMethod
    public static AssetBundleBuild[] BuildMapAssetBundles(Dictionary<string, List<string>> bundleDict, BuildTarget target)
    {
        GKFileUtil.CreateDirectory(AssetBundleDefine.assetBundlePath);
        Dictionary<string, AssetBundleBuild> buildMap = new Dictionary<string, AssetBundleBuild>();
        foreach (var key in bundleDict.Keys)
        {
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = key;
            build.assetBundleVariant = "assetbundle";
            build.assetNames = bundleDict[key].ToArray();
            buildMap.Add(key, build);
        }
        string optputPath = string.Format("{0}{1}{2}/", AssetBundleDefine.assetBundlePath, AssetBundleDefine.GetExternalResourcesFullPath(), "Temp");
        //Debug.Log (string.Format("BuildMapAssetBundles output path: {0}| Target: {1}", optputPath, target.ToString()));	
        AssetBundleBuild[] bundles = new AssetBundleBuild[buildMap.Values.Count];
        int tIdx = 0;
        foreach (var b in buildMap.Values)
        {
            //Debug.Log(string.Format("bundle idx: {0}, path: {1}",tIdx, b.assetBundleName));
            bundles[tIdx] = b;
            tIdx++;
        }

        //Debug.Log (string.Format("Length: {0}, optputPath: {1}", bundles.Length, optputPath));
        BuildPipeline.BuildAssetBundles(optputPath, bundles, BuildAssetBundleOptions.None, target);
        //Debug.Log("Successed");
        return bundles;
    }

    static public void GenFullPackages()
    {
        ClearFullPackageCompressList();
        CompressAssetBundles("Essential/");
        ModifyDownloadFlagInList(1);
        CompressAssetBundles("Ini/");
        ModifyDownloadFlagInList(0);
    }
    #endregion

    #region PrivateMethod
    // Modify the list file download mark, use the compressed package to expand, avoid two changes.
    [MenuItem("GK/Modify download type in the list/Not downloaded")]
    static public void MenuItem_ModifyNotDownloadedInlist()
    {
        GKAssetBundleBuilder.ModifyDownloadFlagInList(0);
    }
    [MenuItem("GK/Modify download type in the list/Downloaded")]
    static public void MenuItem_ModifyDownloadedInlist()
    {
        GKAssetBundleBuilder.ModifyDownloadFlagInList(1);
    }

    [MenuItem("GK/Build Asset Bundles/StandaloneWindows")]
    static private void BuildStandaloneWindows()
    {
        if (EditorUtility.DisplayDialog("Build Asset Bundles", "Do you want build standalone windows?", "Of course", "No, Thanks"))
        {
            Debug.Log("Build standalone windows.");
            BuildAssetBundles(BuildTarget.StandaloneWindows64);
        }
        else
        {
            Debug.Log("Cancel.");
        }
    }

    [MenuItem("GK/Build Asset Bundles/IOS")]
    static private void BuildIOS()
    {
        if (EditorUtility.DisplayDialog("Build Asset Bundles", "Do you want build ios", "Of course", "No, Thanks"))
        {
            Debug.Log("Build ios.");
            BuildAssetBundles(BuildTarget.StandaloneOSXIntel64);
        }
        else
        {
            Debug.Log("Cancel.");
        }
    }

    [MenuItem("GK/Build Asset Bundles/Android")]
    static private void BuildAndroid()
    {
        if (EditorUtility.DisplayDialog("Build Asset Bundles", "Do you want build android?", "Of course", "No, Thanks"))
        {
            Debug.Log("Build android.");
            BuildAssetBundles(BuildTarget.Android);
        }
        else
        {
            Debug.Log("Cancel.");
        }
    }

    static private void BuildAssetBundles(BuildTarget target)
    {
        Debug.Log("assetBundlePath: " + AssetBundleDefine.assetBundlePath);
        GKFileUtil.CreateDirectory(AssetBundleDefine.assetBundlePath);
        BuildPipeline.BuildAssetBundles(AssetBundleDefine.assetBundlePath, BuildAssetBundleOptions.None, target);
    }

    [MenuItem("GK/Build Asset Bundles/External Rescources")]
    static private void BuildExternalResources()
    {
        if (EditorUtility.DisplayDialog("Build Asset Bundles", "Do you want build external resources?", "Of course", "No, Thanks"))
        {
            Debug.Log("Build external resources.");
            GTAssetBundleBuilderWindow.MenuItem_Window();
        }
        else
        {
            Debug.Log("Cancel.");
        }
    }

    [MenuItem("GK/Generate bundle package/Dev")]
    static private void CompressAssetBundles()
    {
        if (EditorUtility.DisplayDialog("Generate bundle package", "Do you want generate bundle package?", "Of course", "No, Thanks"))
        {
            Debug.Log("Generate bundle package.");
            RegionDefine.currentChannel = RegionDefine.Channel.Dev;
            ClearFullPackageCompressList();
            CompressAssetBundles("Essential/");
            ModifyDownloadFlagInList(1);
            CompressAssetBundles("Ini/");
            ModifyDownloadFlagInList(0);
        }
        else
        {
            Debug.Log("Cancel.");
        }
    }

    // Compress asset bundles, Each compression pack is no more than 50M。
    static private void CompressAssetBundles(string path)
    {
        int compressLimit = 50 * 1024 * 1024; // 50M.

        string compressName = Path.GetDirectoryName(path);

        string compressDirectoryPath = string.Format("{0}Compress/{1}", AssetBundleDefine.GetDiffAssetBundleRegionPath(true), compressName);

        // Get bundles.
        string bundlesPath = string.Format("{0}/{1}", AssetBundleDefine.GetDiffAssetBundleRegionPath(true), path);

        //		Debug.Log ("bundlesPath: "+bundlesPath);
        string[] bundles = GKFileUtil.GetFilesInDirectory(bundlesPath);

        // Compressing.
        long totalLength = 0;
        int index = 0;  // Compression index.
        List<string> bundleName = new List<string>();
        bundleName.Clear();
        foreach (string b in bundles)
        {
            //			Debug.Log ("Bundle name: " + b);
            // Filter invalid files.
            if (!GKFileUtil.FilterInvalidFiles(b))
                continue;

            FileStream reader = new FileStream(b, FileMode.Open, FileAccess.Read);

            if (reader.Length > compressLimit)
            {
                Debug.LogError(string.Format("reader.Length > compressLimit, Skip it. File Name: {0}", b));
                continue;
            }

            //			Debug.Log (string.Format("len:{0}|Total:{1}|Limit:{2}", reader.Length, totalLength, compressLimit));
            if (reader.Length + totalLength < compressLimit)
            {
                //				Debug.Log ("Bundle add name: " + b );
                bundleName.Add(b);
                totalLength += reader.Length;
            }
            else
            {
                // Compressing the asset bundles.
                CompressingAssetbundleBundle(compressDirectoryPath, index, bundleName);

                // Reset variable;
                totalLength = 0;
                index++;
                bundleName.Clear();

                // Push asset bundle to buffer.
                bundleName.Add(b);
                totalLength += reader.Length;
            }

            reader.Close();
        }
        // Compressing the asset bundles.
        CompressingAssetbundleBundle(compressDirectoryPath, index, bundleName);
    }

    static private void CompressingAssetbundleBundle(string directory, int i, List<string> bundles, int version = 0)
    {
        //		Debug.Log (string.Format( "CompressingAssetbundleBundle directory: {0}", directory));

        // Create the directory.
        // Remove last char [/].
        string dir = GKFileUtil.RemoveLastSlash(directory);

        string[] fs = dir.Split('/');
        string fn = fs[fs.Length - 1];

        //		Debug.Log (string.Format( "CompressingAssetbundleBundle fn: {0}", fn));

        // If is Ini folder.
        if (!dir.Contains("Ini") && 0 == version)
        {
            if (0 == i)
                dir = string.Format("{0}_{1}", dir, version);
            else
                dir = string.Format("{0}_{1}_{2}", dir, i, version);
        }

        GKFileUtil.CreateDirectory(dir);

        // Delete old asset bundles.
        GKFileUtil.DeleteDirectory(dir);

        // Clone the asset bundle.
        foreach (string b in bundles)
        {
            int startFileIdx = b.IndexOf(fn) + fn.Length;
            string p = b.Substring(startFileIdx, b.Length - startFileIdx);
            string dest = string.Format("{0}{1}", dir, p);
            GKFileUtil.CreateDirectory(Path.GetDirectoryName(dest));
            //			Debug.Log ("Bundle Copy name: " + b + "| Dest: " + dest);
            System.IO.File.Copy(b, dest);
            //			Debug.Log ("Success.");
        }

        // Compressing.
        List<string> l = new List<string>();
        string zipName = string.Format("{0}Compress/{1}.zip", AssetBundleDefine.GetDiffAssetBundleRegionPath(true), GKFileUtil.GetDirctoryName(dir));
        l.Add(dir);
        GKZipUtility.Zip(l.ToArray(), zipName, 0);

        GKFileUtil.DeleteDirectory(dir);

        // Gen compress list.
        if (!dir.Contains("Ini") && 0 == version)
            GenCompressList(version, zipName);

    }

    // Remove full package lines.
    static private void ClearFullPackageCompressList()
    {
        string compressFilePath = string.Format("{0}Ini/CompressInfo.list", AssetBundleDefine.GetDiffAssetBundleRegionPath(true));

        // Filter version 0.
        AssetBundleController.LoadCompressListToDictionary(compressFilePath, 0);

        // Output to file.
        CompressListPushToFile(compressFilePath);
    }

    static private void GenCompressList(int version, string path)
    {
        string compressFilePath = string.Format("{0}Ini/CompressInfo.list", AssetBundleDefine.GetDiffAssetBundleRegionPath(true));
        AssetBundleController.LoadCompressListToDictionary(compressFilePath);

        // Init info.
        FileStream reader = new FileStream(path, FileMode.Open, FileAccess.Read);
        CompressInfo ci = new CompressInfo();
        ci.version = version;
        ci.name = Path.GetFileNameWithoutExtension(path);
        ci.size = reader.Length;
        ci.time = string.Format("{0:G}", DateTime.Now);
        reader.Close();

        // Push to Dictionary.
        AssetBundleController.AddCompressInfo(ci);

        // Output to file.
        CompressListPushToFile(compressFilePath);
    }

    static public void CompressListPushToFile(string path)
    {
        StreamWriter outputStream = new StreamWriter(path);
        foreach (var cd in AssetBundleController.compressDictionary.Values)
        {
            foreach (var n in cd.names)
            {
                string outPutContent = string.Format("{0}|{1}|{2}|{3}", n.version.ToString("X"), n.name, n.size.ToString("X"), n.time);
                outputStream.WriteLine(outPutContent);
            }
        }
        outputStream.Flush();
        outputStream.Close();
    }

    // Currently, Only need to process the essential list.
    static public void ModifyDownloadFlagInList(int isDownload)
    {
        Dictionary<string, AssetBundleInfo> dict = new Dictionary<string, AssetBundleInfo>();

        //		foreach (var t in GK.EnumValues<AssetBundleDefine.AssetBundleDownloadType>()) {
        //			if (AssetBundleDefine.AssetBundleDownloadType.Ini == t || AssetBundleDefine.AssetBundleDownloadType.Count == t)
        //				continue;

        AssetBundleDefine.AssetBundleDownloadType t = AssetBundleDefine.AssetBundleDownloadType.Essential;

        dict.Clear();

        string infoPath = string.Format("{0}/Ini", AssetBundleDefine.GetDiffAssetBundleRegionPath(true));
        GKFileUtil.CreateDirectory(infoPath);
        infoPath = string.Format("{0}/{1}Info.list", infoPath, t.ToString());

        StreamReader sr = new StreamReader(infoPath);
        String line;
        while ((line = sr.ReadLine()) != null)
        {
            AssetBundleInfo info = new AssetBundleInfo(line);
            string[] elements = line.Split('|');
            info.isDownload = isDownload;
            dict[elements[0]] = info;
        }
        sr.Close();

        AssetBundleController.Instance().OutputInfoToFile(infoPath, dict);
        //		}
    }
    #endregion
}