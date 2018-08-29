using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using UnityEngine.Networking;

public class AssetBundleDefine
{
    //object type;
    public enum AssestbundleType
    {
        Object = 0,
        GameObject,
        Texture,
        Sprite,
        Text,
        Count
    }

    // Download file path type.
    public enum AssetBundleDownloadType
    {
        Ini = 0,
        Essential,
        Deferred,
        Dynamic,
        Compress,
        Count,
    }

    static private string _assetBundlePath = "";
    static public string assetBundlePath
    {
        set
        {
            _assetBundlePath = value;
        }
        get
        {
            if (string.IsNullOrEmpty(_assetBundlePath))
            {
                return string.Format("{0}/../AssestBundles/{1}/", Application.dataPath, RegionDefine.currentData.version);
            }
            else
            {
                return _assetBundlePath;
            }
        }
    }
    static public string assetBundleCachePath = string.Format("{0}/AssestBundles/", Application.temporaryCachePath);
    static public string externalResourcePath = "ExternalResources/";
    static public string assestbundleExtension = "";

    static public string version = "";

    // Get external resources & region path.
    public static string GetExternalResourcesFullPath()
    {
        return string.Format("{0}{1}/", externalResourcePath, RegionDefine.GetRegionType(RegionDefine.currentChannel));
    }

    // Get asset bundle region path.
    public static string GetDiffAssetBundleRegionPath(bool isOutputPath, bool isLastVersion = false)
    {
        string outRootPath = assetBundlePath;
        if (isOutputPath && isLastVersion)
            outRootPath = outRootPath.Replace(RegionDefine.currentData.version, RegionDefine.currentData.lastVersion);
        string rootPath = isOutputPath ? outRootPath : string.Format("{0}/", Application.dataPath);
        string subPath = GetExternalResourcesFullPath();
        return string.Format("{0}{1}", rootPath, subPath);
    }

    // Get different assetbundle type folder path.
    public static string GetDiffTypeFolderPath(AssetBundleDownloadType type, bool isOutputPath)
    {
        string rootPath = isOutputPath ? assetBundlePath : string.Format("{0}/", Application.dataPath);
        string subPath = GetExternalResourcesFullPath();
        return string.Format("{0}{1}{2}", rootPath, subPath, type.ToString());
    }

    // Inside to outside path.
    public static string ExchangeAssetBundlePath(string path, string prePath)
    {
        string[] array = path.Split(new string[] { "ExternalResources" }, System.StringSplitOptions.None);
        if (array.Length < 1)
            return "";

        string p = string.Format("{0}ExternalResources{1}", prePath, array[array.Length - 1]);
        return p;
    }

    /// <summary>
    /// Gets the asset bundle download type by path.
    /// </summary>
    /// <returns>AssetBundleDownloadType.</returns>
    /// <param name="p">P.</param>
    public static AssetBundleDownloadType GetAbDownloadTypeByPath(string p)
    {
        if (string.IsNullOrEmpty(p))
        {
            Debug.LogError(string.Format("GetAbDownloadTypeByPath Path is null or empty."));
            return AssetBundleDownloadType.Ini;
        }

        // Check ini.
        if (p.Contains("Ini/") || p.Contains(".list"))
            return AssetBundleDownloadType.Ini;

        // Check compress.
        if (p.Contains(".zip"))
            return AssetBundleDownloadType.Compress;

        if (p.ToLower().Contains("essential"))
        {
            return AssetBundleDownloadType.Essential;
        }
        else if (p.ToLower().Contains("deferred"))
        {
            return AssetBundleDownloadType.Deferred;
        }
        else if (p.ToLower().Contains("dynamic"))
        {
            return AssetBundleDownloadType.Dynamic;
        }
        return AssetBundleDownloadType.Ini;
    }

    /// <summary>
    /// Gets the cache list info path by file path.
    /// </summary>
    /// <returns>The cache list info path by file path.</returns>
    /// <param name="path">Path.</param>
    public static string GetCacheListInfoPathByFilePath(string path)
    {
        string fp = "EssentialInfo.list";

        AssetBundleDownloadType t = GetAbDownloadTypeByPath(path);

        switch (t)
        {
            case AssetBundleDownloadType.Essential:
                fp = "EssentialInfo.list";
                break;
            case AssetBundleDownloadType.Deferred:
                fp = "DeferredInfo.list";
                break;
            case AssetBundleDownloadType.Dynamic:
                fp = "DynamicInfo.list";
                break;
            default:
                return "";
        }

        return string.Format("{0}{1}Ini/{2}", assetBundleCachePath, GetExternalResourcesFullPath(), fp);
    }
}