using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading;
using GKBase;
using GKCompress;
using GKFile;

public class AssetBundleController : GKSingleton<AssetBundleController>
{
    #region Data
    public class CompressList
    {
        public List<CompressInfo> names = new List<CompressInfo>();
    }

    public class UnCompressIniDelegate : GKZipUtility.UnzipCallback
    {
        public override void OnFinished(bool _result)
        {
            if (_result)
            {
                // Ini Success.
                Debug.Log(string.Format("CompressIni Success."));
                bInitCompressListFinished = true;
            }
        }
    }

    public class UnCompressDelegate : GKZipUtility.UnzipCallback
    {
        int count = 0;
        int curIdx = 0;

        public void Reset()
        {
            count = 0;
            curIdx = 0;
        }
        public void SetCount(int c) { count = c; }

        public override void OnFinished(bool _result)
        {
            if (_result)
            {
                curIdx++;
                Debug.Log(string.Format("UnCompressDelegate curIdx: {0} | count： {1}", curIdx, count));
                if (curIdx >= count)
                {
                    // Success.
                    Debug.Log(string.Format("Compress Success."));
                    bInitCompressFinished = true;
                }
            }
        }
    }


    #endregion

    #region PublicField
    // Info list: Before download the assetBundle, If not yet, get from the network.
    public Dictionary<string, AssetBundleInfo> essentialDictionary = new Dictionary<string, AssetBundleInfo>();
    public Dictionary<string, AssetBundleInfo> deferredDictionary = new Dictionary<string, AssetBundleInfo>();
    public Dictionary<string, AssetBundleInfo> dynamicDictionary = new Dictionary<string, AssetBundleInfo>();
    // Compress information cache data. 
    static public Dictionary<int, CompressList> compressDictionary = new Dictionary<int, CompressList>();
    static public bool bInitCompressListFinished = false;
    static public bool bInitCompressFinished = false;
    static public bool bLoadVersionFinished = false;
    static public bool bInitListFinished = false;
    static public bool bInitEssentialFinished = false;

    // Version changed delegate.
    public delegate void VersionChanged(string version);
    public VersionChanged OnVersionChanged = null;
    #endregion

    #region PrivateField
    private AssetBundleManifest manifest = null;
    private List<AssetBundle> DependenceList = new List<AssetBundle>();
    private const int iniListLimit = 3; // Essential & Deferred & Dynamic.
    private int initListCount = 0;  // init download & campare success count.
    static private ReaderWriterLockSlim fileLock = new ReaderWriterLockSlim(); // file sync lock.
                                                                               // Avoid duplicate writes and stream locks.idx 0: updatingFlag, 1: needUpdateFlag.
    static private bool[,] updatingListInfoFlag = new bool[(int)AssetBundleDefine.AssetBundleDownloadType.Count, 2];
    private UnCompressIniDelegate ucIniDelegate = new UnCompressIniDelegate();  // Init files compress delegate.
    private UnCompressDelegate ucDelegate = new UnCompressDelegate(); // files compress delegate.
    private Dictionary<string, Dictionary<string, Sprite>> spriteDic = new Dictionary<string, Dictionary<string, Sprite>>();    // sprites.
    private string _resourceRoot = "";
    #endregion

    #region PublicMethod
    public void Init()
    {
        // Reset init list count.
        initListCount = 0;
        bLoadVersionFinished = false;
        bInitListFinished = false;
        bInitEssentialFinished = false;
        spriteDic.Clear();
    }

    public void SetResourceRoot(string path)
    {
        _resourceRoot = path;
    }

    public void DownloadObject(string bundle, string name, NetController.OnCustom callback = null)
    {
        DownloadAssestbundle<object>(bundle, name, null, AssetBundleDefine.AssestbundleType.Object, callback);
    }
    public void DownloadTexture(string bundle, string name, ref RawImage target, NetController.OnCustom callback = null)
    {
        DownloadAssestbundle<RawImage>(bundle, name, target, AssetBundleDefine.AssestbundleType.Texture, callback);
    }
    public void DownloadGameObject(string bundle, string name, GameObject target, NetController.OnCustom callback = null)
    {
        DownloadAssestbundle<GameObject>(bundle, name, target, AssetBundleDefine.AssestbundleType.GameObject, callback);
    }
    public void DownloadSprite(string bundle, string name, ref Image target, NetController.OnCustom callback = null)
    {
        string bundleName = Path.GetFileNameWithoutExtension(bundle);
        if (spriteDic.ContainsKey(bundleName))
        {
            target.sprite = spriteDic[bundleName][name];
            if (null != callback)
            {
                callback(bundle);
            }
        }
        else
        {
            DownloadAssestbundle<Image>("Essential/" + bundle, name, target, AssetBundleDefine.AssestbundleType.Sprite, callback);
        }
    }

    public void LoadVersion()
    {
        NetController.curUseDownloadCount++;
        var h = NetController.GetIdleDownloadHandle();
        string url = string.Format("{0}Version.txt", _resourceRoot);
        string destDir = GKFileUtil.GetPreviousFolderPath(AssetBundleDefine.assetBundleCachePath);
        //Debug.Log(string.Format("DownLoadNextData url: {0} | destDir: {1}", url, destDir));
        GKFileUtil.DeleteFile(destDir + "Version.txt");
        h.Download(url, destDir, null);
        NetController.gameInstance.StartCoroutine(SettingVersion(h, destDir));
    }

    /**
     * Load local list info. 
     * Step: essential -> deferred -> dynamic.
     * 1) Check the local list.
     * 2) Download list.
     * 3) Compare the list.
     * */
    public void LoadLocalListInfo()
    {
        string[] names = GK.EnumNames<AssetBundleDefine.AssetBundleDownloadType>();

        // Remove Ini & Count.
        for (int i = 1; i < names.Length - 1; i++)
        {
            // Init path.
            string path = AssetBundleDefine.GetCacheListInfoPathByFilePath(names[i]);
            if (string.IsNullOrEmpty(path))
            {
                // If type equals Compress, Don't print log.
                if (!string.Equals(names[i], "Compress"))
                    Debug.LogError(string.Format("LoadLocalListInfo failure. name: {0}, path: {1}", names[i], path));
                continue;
            }
            string backUpFilePath = string.Format("{0}/{1}_backUp{2}", Path.GetDirectoryName(path), Path.GetFileNameWithoutExtension(path), Path.GetExtension(path));
            // Check the file exists.
            if (!File.Exists(path))
            {
                Debug.Log(string.Format("Get list information from cache cant't find file. name: {0}, path: {1}", names[i], path));
                if (File.Exists(backUpFilePath))
                {
                    //Debug.LogError("Rollback config file.");
                    // Roll back. Just support object.
                    File.Copy(backUpFilePath, path);
                }
                else
                {
                    continue;
                }
            }

            // Select container.
            var dictionary = essentialDictionary;
            switch (names[i])
            {
                case "Essential":
                    dictionary = essentialDictionary;
                    break;
                case "Deferred":
                    dictionary = deferredDictionary;
                    break;
                case "Dynamic":
                    dictionary = dynamicDictionary;
                    break;
                default:
                    Debug.LogError(string.Format("Can not load list type. type: {0}", names[i]));
                    dictionary = null;
                    break;
            }

            if (null == dictionary)
                continue;

            dictionary.Clear();

            StreamReader sr = new StreamReader(path);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                // Init dictionary data.
                if (string.IsNullOrEmpty(line))
                    continue;
                AssetBundleInfo info = new AssetBundleInfo(line);
                string[] elements = line.Split('|');

                dictionary[elements[0]] = info;
            }
            sr.Close();
            sr.Dispose();

            // Remove list. If there is one, it triggers breakpoint resume.
            //GKFileUtil.DeleteFile (path);
        }

        DownloadListInfo();
    }

    // Download essential.
    public void DownloadEssential()
    {
        bool initEssentialSuccess = true;
        foreach (var e in essentialDictionary)
        {
            // If downloaded, return.
            if (1 == e.Value.isDownload)
                continue;

            initEssentialSuccess = false;

            //			Debug.Log (string.Format("DownloadEssential bundlePath: {0}, name: {1}", bundlePath, strArrays[strArrays.Length-1]));
            string bundle = string.Format("{0}/{1}.assetbundle", e.Value.type.ToString(), e.Key);
            AssetBundleController.Instance().DownloadObject(bundle, "", EssentialFinished);
        }

        if (initEssentialSuccess)
        {
            // Success.
            Debug.Log(string.Format("DownloadEssential success."));
            bInitEssentialFinished = true;
        }
    }

    // Download essential.
    public void DownloadDeferred()
    {
        foreach (var e in deferredDictionary)
        {
            // If downloaded, return.
            if (1 == e.Value.isDownload)
                continue;
            string bundle = string.Format("{0}/{1}.assetbundle", e.Value.type.ToString(), e.Key);
            AssetBundleController.Instance().DownloadObject(bundle, "", null);
        }
    }

    public AssetBundle GetAssetBundleFromCache(string path)
    {
        //		string p = string.Format("{0}{1}{2}", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath() , path);
        //		Debug.Log (string.Format ("GetAssetBundleFromCache file path: {0}", path));

        if (!File.Exists(path))
        {
            //Debug.Log (string.Format("GetAssetBundleFromCache cant't find file. Path: {0}", path));
            return null;
        }

        AssetBundle assetbundle = null;

        // If the suffix is AssetBundle.
        string[] suffix = path.Split('.');
        if (suffix[suffix.Length - 1].Equals("assetbundle"))
        {
            FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);

            if (0 == stream.Length)
            {
                Debug.Log("GetAssetBundleFromCache stream length is 0. Break it.");
                stream.Close();
                stream.Dispose();
                return null;
            }

            byte[] bytes = new byte[stream.Length];
            stream.Read(bytes, 0, (int)stream.Length);
            assetbundle = AssetBundle.LoadFromMemory(bytes);
            //		AssetBundle assetbundle = AssetBundle.LoadFromFile (path);
            stream.Close();
            stream.Dispose();
        }

        if (null == assetbundle)
        {
            //			Debug.Log (string.Format("GetAssetBundleFromCache cant't create assetbundle."));
            return null;
        }

        //		Debug.Log (string.Format("GetAssetBundleFromCache success. Bundle Name: {0}", assetbundle.name));
        return assetbundle;
    }

    /// <summary>
    /// Gets the dictionary by path.
    /// </summary>
    /// <returns>The dictionary by path.</returns>
    /// <param name="path">Path.</param>
    public Dictionary<string, AssetBundleInfo> GetDictionaryByPath(string path)
    {
        AssetBundleDefine.AssetBundleDownloadType t = AssetBundleDefine.GetAbDownloadTypeByPath(path);

        switch (t)
        {
            case AssetBundleDefine.AssetBundleDownloadType.Essential:
                return essentialDictionary;
            case AssetBundleDefine.AssetBundleDownloadType.Deferred:
                return deferredDictionary;
            case AssetBundleDefine.AssetBundleDownloadType.Dynamic:
                return dynamicDictionary;
        }

        return null;
    }

    // Init writer to file.
    public void OutputInfoToFile(string outputPath, Dictionary<string, AssetBundleInfo> dict)
    {
        //		foreach(var i in dict)
        //		{
        //			Debug.Log (string.Format("abInfoList element: {0}, download: {1}", i.Key, i.Value.isDownload));
        //		}

        try
        {
            // Init list type. Some containers maybe contain all list information.
            AssetBundleDefine.AssetBundleDownloadType type = AssetBundleDefine.AssetBundleDownloadType.Ini;
            string p = Path.GetFileNameWithoutExtension(outputPath);
            switch (p)
            {
                case "EssentialInfo":
                case "EssentialBuild":
                    type = AssetBundleDefine.AssetBundleDownloadType.Essential;
                    break;
                case "DeferredInfo":
                case "DeferredBuild":
                    type = AssetBundleDefine.AssetBundleDownloadType.Deferred;
                    break;
                case "DynamicInfo":
                case "DynamicBuild":
                    type = AssetBundleDefine.AssetBundleDownloadType.Dynamic;
                    break;
                default:
                    type = AssetBundleDefine.AssetBundleDownloadType.Ini;
                    break;
            }

            fileLock.EnterWriteLock();
            //			Debug.Log(string.Format("outputPath: {0}", outputPath));
            StreamWriter outputStream = new StreamWriter(outputPath);

            foreach (var info in dict)
            {
                string name = info.Key;
                var data = info.Value;

                if (data.type != type)
                {
                    //					Debug.Log(string.Format("OutputInfoToFile different types.data: {0} | type: {1}", data.type, type));
                    continue;
                }
                string outPutContent = string.Format("{0}|{1}|{2}|{3}|{4}|{5}|{6}|{7}|{8}|{9}",
                    name, (int)data.type, data.assetBundleName, data.version.ToString("X"), data.md5, data.size.ToString("X"), data.priority.ToString("X"),
                    data.time, data.srcMd5, data.isDownload.ToString("X"));
                outputStream.WriteLine(outPutContent);
            }
            outputStream.Flush();
            outputStream.Close();
        }
        catch
        {
            Debug.LogError("OutputInfoToFile have problem. Path: " + outputPath);
        }
        finally
        {
            fileLock.ExitWriteLock();
        }
    }

    static public void AddCompressInfo(CompressInfo info)
    {
        if (null == info)
            return;

        if (!compressDictionary.ContainsKey(info.version))
        {
            CompressList cl = new CompressList();
            compressDictionary.Add(info.version, cl);
        }
        compressDictionary[info.version].names.Add(info);
    }

    public void DownloadCompressIni()
    {
        string listPath = string.Format("{0}{1}/Essential/", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath());
        // If the list file is not empty, it represents the first run of the game.
        if (!Directory.Exists(listPath))
        {
            //			Debug.Log (string.Format("[DownloadCompressIni] listPath: {0}", listPath));
            AssetBundleController.Instance().DownloadObject("Compress/Ini.zip", "Ini", OnCompressIniFinished);
        }
        else
        {
            // Success.
            bInitCompressListFinished = true;
        }
    }

    // Determine whether full package compressed files need to be downloaded based on local resources.
    // If don't it, Download the corresponding zip files according to the resource version number.
    public void DownloadCompress()
    {
        string listPath = string.Format("{0}{1}/Essential/", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath());
        // If the list file is not empty, it represents the first run of the game.
        if (!Directory.Exists(listPath))
        {
            listPath = string.Format("{0}{1}/Ini/CompressInfo.list", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath());
            if (!File.Exists(listPath))
            {
                AssetBundleController.Instance().DownloadObject("Ini/CompressInfo.list", "CompressInfo", OnCompresslistFinished);
            }
            else
            {
                OnCompresslistFinished("");
            }
        }
        else
        {
            bInitCompressFinished = true;
        }
    }

    static public void LoadCompressListToDictionary(string path, int filterVersion = -1)
    {
        compressDictionary.Clear();

        // Initcompress list.
        if (File.Exists(path))
        {
            StreamReader sr = new StreamReader(path);
            String line;
            while ((line = sr.ReadLine()) != null)
            {
                CompressInfo info = new CompressInfo(line);
                if (filterVersion != info.version)
                    AddCompressInfo(info);
            }
            sr.Close();
        }
    }

    // Clear cache resources by folder or invalid assets.
    public void ClearCache(AssetBundleDefine.AssetBundleDownloadType type, bool bInvalidAssets = true)
    {
        if (AssetBundleDefine.AssetBundleDownloadType.Count == type)
            return;

        string path = string.Format("{0}{1}", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath());

        path = string.Format("{0}{1}/", path, type.ToString());

        Debug.Log(string.Format("ClearCache path: {0}", path));

        if (!bInvalidAssets)
            GKFileUtil.DeleteDirectory(path);
        else
        {
            // If clear Ini or compression. Delete the folder.
            if (AssetBundleDefine.AssetBundleDownloadType.Compress == type || AssetBundleDefine.AssetBundleDownloadType.Ini == type)
            {
                GKFileUtil.DeleteDirectory(path);
                return;
            }

            Dictionary<string, AssetBundleInfo> dict = null;
            switch (type)
            {
                case AssetBundleDefine.AssetBundleDownloadType.Essential:
                    dict = essentialDictionary;
                    break;
                case AssetBundleDefine.AssetBundleDownloadType.Deferred:
                    dict = deferredDictionary;
                    break;
                case AssetBundleDefine.AssetBundleDownloadType.Dynamic:
                    dict = dynamicDictionary;
                    break;
            }

            if (null == dict)
            {
                Debug.LogError(string.Format("ClearCache failure. type: {0}", type.ToString()));
                return;
            }

            string[] files = GKFileUtil.GetFilesInDirectory(path);
            foreach (var f in files)
            {
                //				Debug.Log (string.Format("ClearCache file path: {0}", f));
                string name = Path.GetFileNameWithoutExtension(f);
                if (!dict.ContainsKey(name))
                {
                    //					Debug.Log (string.Format("ClearCache DeleteFile path: {0}", f));
                    GKFileUtil.DeleteFile(f);
                }
            }
        }
    }
    #endregion

    #region PrivateMethod
    // Download Manifest Delegate.
    private void DownloadManifestFinished(WWW www, DownloadData data)
    {
        if (string.IsNullOrEmpty(www.error))
        {
            AssetBundle ab = www.assetBundle;
            manifest = (AssetBundleManifest)ab.LoadAsset("AssetBundleManifest");
            ab.Unload(false);
        }
        else
            Debug.Log(www.error);
    }

    // Download Dependencies Delegate.
    private void DownloadDependenceFinished(WWW www, DownloadData data)
    {
        if (string.IsNullOrEmpty(www.error))
        {
            DependenceList.Add(www.assetBundle);
        }
        else
            Debug.Log(www.error);
    }

    // Multithreading download target assetbundle delegate.
    private void MultithreadingDownloadFinished(DownloadData data, AssetBundle bundle)
    {
        ProcessBundle(data, bundle);

        // Refresh.
        // Dictionary Checked.
        string n = Path.GetFileNameWithoutExtension(data.url);
        var dict = AssetBundleController.Instance().GetDictionaryByPath(data.url);
        if (null != dict)
        {
            if (!dict.ContainsKey(n))
            {   // Error.
                Debug.LogError(string.Format("MultithreadingDownloadFinished failure. Modify List info, But can't find it. path: {0}", data.url));
                return;
            }
            else
            {
                dict[n].isDownload = 1;
                string cachePath = AssetBundleDefine.GetCacheListInfoPathByFilePath(data.url);
                if (string.IsNullOrEmpty(cachePath))
                {
                    Debug.LogError(string.Format("MultithreadingDownloadFinished failure. Can't gets cache list info path: {0}", data.url));
                    return;
                }
                UpdateListInfo(cachePath, dict);
            }
        }
    }

    // Refresh local file list (IO).
    private void UpdateListInfo(string path, Dictionary<string, AssetBundleInfo> dict)
    {
        var t = AssetBundleDefine.GetAbDownloadTypeByPath(path);

        // If the file is not written.
        if (!updatingListInfoFlag[(int)t, 0])
        {

            // Lock the file stream.
            updatingListInfoFlag[(int)t, 0] = true;
            OutputInfoToFile(path, dict);
            updatingListInfoFlag[(int)t, 0] = false;
            // Check the update flag.
            if (updatingListInfoFlag[(int)t, 1])
            {
                updatingListInfoFlag[(int)t, 1] = false;
                UpdateListInfo(path, dict);
            }

        }
        else
        {
            updatingListInfoFlag[(int)t, 1] = true;
        }
    }

    // Download target assetbundle delegate.
    private void DownloadTargetAssetBundleFinished(DownloadData data, WWW www = null)
    {
        if (string.IsNullOrEmpty(www.error))
        {
            AssetBundle ab = www.assetBundle;
            ProcessBundle(data, ab);
        }
        else
            Debug.Log(www.error);

        for (int i = DependenceList.Count - 1; i >= 0; i--)
            DependenceList[i].Unload(false);
        DependenceList.Clear();
    }

    private void ProcessBundle(DownloadData data, AssetBundle bundle)
    {
        if (null != bundle)
        {
            switch (data.type)
            {
                case AssetBundleDefine.AssestbundleType.GameObject:
                    GameObject[] goArray = bundle.LoadAllAssets<GameObject>();
                    if (goArray.Length > 0)
                        data.target = GameObject.Instantiate(goArray[0]);
                    break;
                case AssetBundleDefine.AssestbundleType.Texture:
                    Texture[] texArray = bundle.LoadAllAssets<Texture>();
                    if (texArray.Length > 0)
                        ((RawImage)data.target).texture = texArray[0] as Texture;
                    break;
                case AssetBundleDefine.AssestbundleType.Sprite:
                    if (null != data.target)
                    {
                        string bundleName = Path.GetFileNameWithoutExtension(data.url);
                        ((Image)data.target).sprite = LoadSprite(bundleName, data.name, bundle);
                    }
                    break;
                case AssetBundleDefine.AssestbundleType.Object:
                case AssetBundleDefine.AssestbundleType.Text:
                    break;
            }
            bundle.Unload(false);
            bundle = null;
        }
    }

    // Download Assestbundle.
    // Use MANIFEST 
    // Bundle is package name, Name is asset name;
    // Else
    // Bundle is asset path, Name is asset name;
    private void DownloadAssestbundle<T>(string bundle, string name, T target, AssetBundleDefine.AssestbundleType type, NetController.OnCustom custom = null)
    {
        // Download Target Assestbundle.
        DownloadData data = new DownloadData(bundle, MultithreadingDownloadFinished, name, target, type, custom);
        NetController.EnQueue(data);
    }

    // Download list.
    private void DownloadListInfo()
    {
        AssetBundleController.Instance().DownloadObject("Ini/EssentialInfo.list", "EssentialInfo", CompareListInfo);
        AssetBundleController.Instance().DownloadObject("Ini/DeferredInfo.list", "DeferredInfo", CompareListInfo);
        AssetBundleController.Instance().DownloadObject("Ini/DynamicInfo.list", "DynamicInfo", CompareListInfo);
    }

    // Compare the list.
    private void CompareListInfo(string msg)
    {
        string name = Path.GetFileNameWithoutExtension(msg);
        string path = string.Format("{0}{1}{2}", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath(), msg);

        // Check the file exists.
        if (!File.Exists(path))
        {
            Debug.Log(string.Format("Compare list information from cache cant't find file. type: {0}", msg));
            return;
        }

        var dictionary = GetDictionaryByPath(name);
        if (null == dictionary)
        {
            Debug.LogError(string.Format("CompareListInfo failure. Can't gets Dictionary path: {0}", name));
            return;
        }

        // Compare file;
        StreamReader sr = new StreamReader(path);
        String line;
        while ((line = sr.ReadLine()) != null)
        {
            // Init dictionary data.
            AssetBundleInfo info = new AssetBundleInfo(line);
            string[] elements = line.Split('|');
            //Debug.Log(line);
            if (!dictionary.ContainsKey(elements[0]))
            {
                //Debug.Log (string.Format ("CompareListInfo: !dictionary.ContainsKey, key: {0}", elements [0]));
                dictionary[elements[0]] = info;
            }
            else if (dictionary.ContainsKey(elements[0]) && (!dictionary[elements[0]].time.Equals(info.time)) || 0 == dictionary[elements[0]].isDownload)
            {
                //Debug.Log (string.Format ("CompareListInfo: dictionary.ContainsKey and time different., key: {0} | time: {1} - {2} | Download : {3}", 
                //elements [0], dictionary [elements [0]].time, info.time, dictionary [elements [0]].isDownload));

                info.isDownload = 0;
                dictionary[elements[0]] = info;

                // Remove exceed file.
                string p = string.Format("{0}{1}{2}.assetbundle", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath(),
                    string.Format("{0}/{1}", info.type.ToString(), elements[0]));
                GKFileUtil.DeleteFile(p);
            }
        }
        sr.Close();
        sr.Dispose();

        initListCount++;
        Debug.Log(string.Format("CompareListInfo {0}/{1} Msg: {2}.", initListCount, iniListLimit, msg));
        // Init list step success.
        if (iniListLimit == initListCount)
        {
            // Success
            Debug.Log(string.Format("Init list step success."));
            bInitListFinished = true;
        }
    }

    private IEnumerator SettingVersion(MultDownload handle, string msg)
    {
        if (null == handle)
            yield break;

        while (1 == handle.state)
            yield return null;

        msg = string.Format("{0}Version.txt", msg);
        string line = string.Empty;

        // Check the file exists.
        if (File.Exists(msg))
        {
            StreamReader sr = new StreamReader(msg);

            while ((line = sr.ReadLine()) != null)
            {
                if (!string.IsNullOrEmpty(line))
                {
                    if (null != OnVersionChanged)
                        OnVersionChanged(line);
                }
            }
            sr.Close();
            sr.Dispose();

            bLoadVersionFinished = true;
        }
        else
        {
            Debug.Log(string.Format("Setting version from cache cant't find file. type: {0}", msg));
        }

        Debug.Log(string.Format("Version: {0}", line));
        handle.Close();
        NetController.Next();
    }

    // Download essential finished.
    private void EssentialFinished(string msg)
    {
        int count = NetController.GetQueueCount();
        if (0 == count)
        {
            // Success
            Debug.Log(string.Format("DownloadEssential success."));
            bInitEssentialFinished = true;
        }
    }

    private Sprite LoadSprite(string bundleName, string spriteName, AssetBundle bundle)
    {
        if (!spriteDic.ContainsKey(bundleName))
        {
            Sprite[] result = bundle.LoadAllAssets<Sprite>();
            int length = result.Length;
            Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();
            for (int i = 0; i < length; i++)
            {
                Debug.Log(result[i].name);
                if (sprites.ContainsKey(result[i].name) == false)
                {
                    sprites.Add(result[i].name, result[i]);
                }
            }
            spriteDic.Add(bundleName, sprites);
            //Resources.UnloadUnusedAssets();
        }
        return spriteDic[bundleName][spriteName];
    }

    // Unzip the configuration list file.
    private void OnCompressIniFinished(string msg)
    {
        string inputFullPath = string.Format("{0}{1}{2}", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath(), msg);
        Debug.Log(string.Format("UnCompress inputFullPath: {0}", inputFullPath));

        string outputFullPath = string.Format("{0}{1}Ini/", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath());
        Debug.Log(string.Format("UnCompress outputFullPath: {0}", outputFullPath));

        if (!Directory.Exists(outputFullPath))
            GKFileUtil.CreateDirectory(outputFullPath);

        GKZipUtility.UnzipFile(inputFullPath, outputFullPath, null, ucIniDelegate, true);
    }

    private void OnCompresslistFinished(string msg)
    {
        ucDelegate.Reset();

        string compressFilePath = string.Format("{0}{1}Ini/CompressInfo.list", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath());
        LoadCompressListToDictionary(compressFilePath);

        int count = 0;
        // If first run game. Need to download full package compress.
        foreach (var c in compressDictionary)
        {
            if (0 == c.Key)
            {
                foreach (var n in compressDictionary[c.Key].names)
                {
                    compressFilePath = string.Format("Compress/{0}.zip", n.name);
                    Debug.Log(string.Format("Download compress file path: {0}", compressFilePath));
                    AssetBundleController.Instance().DownloadObject(compressFilePath, n.name, UnCompress);

                    count++;
                }
            }
        }

        // Set uncompress count;
        ucDelegate.SetCount(count);
    }

    // UnCompress to cache.
    private void UnCompress(string msg)
    {
        Debug.Log(string.Format("UnCompress msg: {0}", msg));
        string file = Path.GetFileNameWithoutExtension(msg);
        string dir = Path.GetDirectoryName(msg);
        string output = "";
        if (file.ToLower().Contains("ini"))
        {
            file = "Ini";
        }
        else if (file.ToLower().Contains("essential"))
        {
            file = "Essential";
        }
        else if (file.ToLower().Contains("deferred"))
        {
            file = "Deferred";
        }
        else
        {
            file = "Dynamic";
        }
        output = string.Format("{0}/../{1}/", dir, file);
        //		Debug.Log (string.Format("UnCompress output: {0}", output));

        string inputFullPath = string.Format("{0}{1}{2}", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath(), msg);
        //		Debug.Log (string.Format("UnCompress inputFullPath: {0}", inputFullPath));

        string outputFullPath = string.Format("{0}{1}{2}", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath(), output);
        //		Debug.Log (string.Format("UnCompress outputFullPath: {0}", outputFullPath));

        // Check the dirctory. If it does not exist, create it.
        if (!Directory.Exists(outputFullPath))
            GKFileUtil.CreateDirectory(outputFullPath);

        GKZipUtility.UnzipFile(inputFullPath, outputFullPath, null, ucDelegate, true);
    }
    #endregion
}

// Compress data.
public class CompressInfo
{
    public CompressInfo()
    { }

    public CompressInfo(string content)
    {
        string[] elements = content.Split('|');
        if (3 < elements.Length)
        {
            version = Convert.ToInt32(elements[0], 16);
            name = elements[1];
            size = (long)Convert.ToInt32(elements[2], 16);
            time = elements[3];
        }
    }

    public int version { get; set; }
    public string name { get; set; }
    public long size { get; set; }
    public string time { get; set; }
}

// Asset bundle data.
public class AssetBundleInfo
{
    public AssetBundleInfo()
    { }

    public AssetBundleInfo(string content)
    {
        if (content[content.Length - 1].Equals('\r'))
            content = content.Remove(content.Length - 1);
        string[] elements = content.Split('|');
        if (9 < elements.Length)
        {
            type = (AssetBundleDefine.AssetBundleDownloadType)Convert.ToInt32(elements[1], 16);
            assetBundleName = elements[2];
            version = Convert.ToInt32(elements[3], 16);
            md5 = elements[4];
            size = (long)Convert.ToInt32(elements[5], 16);
            priority = Convert.ToInt32(elements[6], 16);
            time = elements[7];
            srcMd5 = elements[8];
            isDownload = Convert.ToInt32(elements[9], 16);
        }
    }

    public AssetBundleDefine.AssetBundleDownloadType type;
    public string assetBundleName { get; set; }
    public int version { get; set; }
    public string md5 { get; set; }
    public long size { get; set; }
    public int priority { get; set; }
    public string time { get; set; }
    public string srcMd5 { get; set; }
    public int isDownload { get; set; }
}

public class AssetBundleInfoList
{
    public AssetBundleDefine.AssetBundleDownloadType type;
    public string path;
}