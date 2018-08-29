using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using GKBase;


public class NetController : GKSingleton<NetController>
{
    #region Data
    public delegate void OnMultFinished(DownloadData data, AssetBundle bundle);
    public delegate void OnCustom(string msg);
    static private Queue<DownloadData> downloadQueue = new Queue<DownloadData>();
    #endregion

    #region PublicField
    static public bool bLocalIPResult = false;  // Did you successfully get the local IP?
    static public int curUseDownloadCount = 0;  // Current number of download handles.
    static public MonoBehaviour gameInstance = null;
    #endregion

    #region PrivateField
    private WWW localIPHandel = null;
    static private int threadingLimitCount = 8;
    static private List<MultDownload> downloadList = new List<MultDownload>();
    #endregion

    #region PublicMethod
    static public void EnQueue(DownloadData data)
    {
        data.finished = false;
        // Dictionary Checked.
        string n = Path.GetFileNameWithoutExtension(data.url);
        var dict = AssetBundleController.Instance().GetDictionaryByPath(data.url);
        if (null != dict)
        {
            if (!dict.ContainsKey(n))
            {   // Error.
                Debug.LogError(string.Format("EnQueue failure. You want to download targer But dictionary don't contains key: {0}", n));
                return;
            }
            else if (1 == dict[n].isDownload)
            {   // Load from cache.
                //Debug.Log (string.Format("EnQueue isDownload is true, Load from cache. key: {0}", n));
                if (null != data.MultiCallback)
                {
                    string path = string.Format("{0}{1}{2}.assetbundle", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath(), data.url);
                    AssetBundle ab = AssetBundleController.Instance().GetAssetBundleFromCache(path);
                    if (null != ab)
                    {
                        data.MultiCallback(data, ab);
                        return;
                    }
                }
            }
        }

        // Downloading...
        downloadQueue.Enqueue(data);
        Next();
    }

    static public void Next()
    {
        if (curUseDownloadCount < threadingLimitCount)
            DownLoadNextData();
    }

    /// <summary>
    /// Get the queue count.
    /// </summary>
    /// <returns>The queue count.</returns>
    static public int GetQueueCount()
    {
        int count = downloadQueue.Count;

        //		Debug.Log (string.Format("GetQueueCount, downloadQueue count: {0}", count));

        foreach (var h in downloadList)
        {
            if (1 == h.state)
                count++;
        }

        return count;
    }

    // Gets current download speed.
    public int GetDownloadSpeed()
    {
        int speed = 0;
        foreach (var h in downloadList)
        {
            speed += h.GetCycleDownloadCount();
        }
        return speed;
    }

    public void Init(MonoBehaviour g)
    {
        gameInstance = g;
        InitMultithreading();
    }

    public void GetLocalIP()
    {
        bLocalIPResult = false;
        //		Game.instance.StartCoroutine (GetLocalIP("http://www.3322.org/dyndns/getip"));
    }

    static public MultDownload GetIdleDownloadHandle()
    {
        foreach (var h in downloadList)
        {
            if (0 == h.state)
                return h;
        }
        return null;
    }
    #endregion

    #region PrivateMethod
    static private void DownLoadNextData()
    {
        if (0 == downloadQueue.Count)
            return;

        var next = downloadQueue.Dequeue();
        // Get from cache. 
        string path = string.Format("{0}{1}{2}", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath(), next.url);

        // Get from internet.
        curUseDownloadCount++;

        var h = GetIdleDownloadHandle();
        string url = string.Format("{0}{1}{2}", AssetBundleDefine.assetBundlePath, AssetBundleDefine.GetExternalResourcesFullPath(), next.url);
        string destDir = string.Format("{0}/", Path.GetDirectoryName(path));
        Debug.Log(string.Format("DownLoadNextData url: {0} | destDir: {1}", url, destDir));
        h.Download(url, destDir, next);
        gameInstance.StartCoroutine(MulithreadingCompleted(h));
    }

    private IEnumerator GetLocalIP(string url)
    {
        localIPHandel = new WWW(url);
        yield return localIPHandel;
        bLocalIPResult = true;
    }

    private void InitMultithreading()
    {
        threadingLimitCount = SystemInfo.processorCount;
        Debug.Log(string.Format("processorCount: {0}", threadingLimitCount));

        for (int i = 0; i < threadingLimitCount; i++)
        {
            MultDownload download = new MultDownload();
            download.Close();
            downloadList.Add(download);
        }
        curUseDownloadCount = 0;
    }

    static private IEnumerator MulithreadingCompleted(MultDownload d)
    {
        // If data is null, break.
        if (null == d)
            yield break;

        // Waitting for download finished.
        while (1 == d.state)
        {
            //Debug.Log(string.Format("state: {0}, name: {1}", d.state, d.data.name));
            yield return null;
        }

        //Debug.Log(string.Format("state: {0}, url: {1}, name: {2}", d.state, d.data.url, d.data.name));
        if (null != d.data)
        {
            //Debug.Log (string.Format ("MulithreadingCompleted, name: {0}", d.data.url));

            d.data.finished = true;
            // Load assetbundle from cache, But need main thread.
            if (null != d.data.MultiCallback)
            {
                string path = string.Format("{0}{1}{2}", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath(), d.data.url);

                AssetBundle ab = AssetBundleController.Instance().GetAssetBundleFromCache(path);
                d.data.MultiCallback(d.data, ab);
            }

            // Call custom callback.
            if (null != d.data.customCallback)
            {
                d.data.customCallback(d.data.url);
            }
        }

        d.Close();

        Next();
    }
    #endregion

}

public class DownloadData
{
    public string url;
    public NetController.OnMultFinished MultiCallback;
    public string name;
    public object target;
    public AssetBundleDefine.AssestbundleType type;
    public Hash128 hashCode;
    public int version;
    public bool finished;   //	Download state.
    public NetController.OnCustom customCallback;

    public DownloadData(string _url, NetController.OnMultFinished multiFun, string _name, object _target = null,
        AssetBundleDefine.AssestbundleType _type = AssetBundleDefine.AssestbundleType.GameObject,
        NetController.OnCustom callback = null, string _hashCode = "", int ver = 0)
    {
        url = _url;
        MultiCallback = multiFun;
        name = _name;
        target = _target;
        type = _type;
        if (!string.IsNullOrEmpty(_hashCode))
        {
            Hash128 code = Hash128.Parse(_hashCode);
            hashCode = code;
        }
        version = ver;
        customCallback = callback;
        finished = false;
    }
}