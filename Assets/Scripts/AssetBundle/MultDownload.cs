using UnityEngine;
using System.Collections;
using System.Net;
using System.IO;
using System.Threading;
using GKFile;

public class MultDownload
{

    #region PublicField
    public float progress { get; private set; }
    public int state = 0;                   // Thread state. 0: Idle, 1: Running, 2: Waitting.
    public DownloadData data;   // Download data.
    #endregion

    #region PrivateField
    private Thread thread;  // Threading handle.
    private int cycleDownloadCount = 0; // The number of bytes downloaded in the current cycle.
    #endregion

    #region PublicMethod
    /**
     * Download function.
     * parameter url - Internet url;
     * parameter fileDirctory - Local cache path;
     * parameter data - download data;
     * */
    public void Download(string url, string fileDirctory, DownloadData data)
    {
        state = 1;
        this.data = data;

        thread = new Thread(delegate ()
            {
            // Create Cache Directory.
            if (!Directory.Exists(fileDirctory))
                {
                //					Debug.Log(string.Format("Directory isn't Exists. Create it. path: {0}", fileDirctory));
                Directory.CreateDirectory(fileDirctory);
                }

            // Create cache file.
            string filePath = string.Format("{0}/{1}", fileDirctory.Substring(0, fileDirctory.Length - 1), url.Substring(url.LastIndexOf('/') + 1));
                string backUpFilePath = string.Format("{0}/{1}_backUp{2}", fileDirctory.Substring(0, fileDirctory.Length - 1), Path.GetFileNameWithoutExtension(url), Path.GetExtension(url));
            //Debug.Log(string.Format("Cache file path: {0} | {1}", filePath, backUpFilePath));
            if (File.Exists(filePath) && data.type == AssetBundleDefine.AssestbundleType.Object)
                {
                // Backup and next time roll back When the download fails.
                //Debug.LogError("Backup config file.");
                if (File.Exists(backUpFilePath))
                        GKFileUtil.DeleteFile(backUpFilePath);
                    File.Copy(filePath, backUpFilePath);
                    GKFileUtil.DeleteFile(filePath);
                }

                var info = File.Create(filePath);
                info.Close();
                info.Dispose();

                float totalLength = GetLength(url);
                if (-1 != totalLength)
                {
                // Load local file. Calc the size.
                FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write);
                    long fileLength = fileStream.Length;
                //Debug.Log(string.Format("fileLength:{0}, totalLength:{1}", fileLength, totalLength));

                // continue downloading...
                if (fileLength < totalLength)
                    {
                        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
                        request.AddRange((int)fileLength);
                        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                        fileStream.Seek(fileLength, SeekOrigin.Begin);
                        Stream httpStream = response.GetResponseStream();
                        byte[] buffer = new byte[1024];
                        int length = httpStream.Read(buffer, 0, buffer.Length);

                    //Debug.Log(string.Format("httpStream Read length: {0}", length));

                    while (length > 0)
                        {
                            if (1 != state)
                                break;

                        // Increase the number of bytes downloaded.
                        cycleDownloadCount += length;

                            fileStream.Write(buffer, 0, length);
                            fileLength += length;
                            progress = fileLength / totalLength * 100;
                            fileStream.Flush();
                            length = httpStream.Read(buffer, 0, buffer.Length);
                        }
                    // Close stream.
                    httpStream.Dispose();
                    }
                    else
                        progress = fileLength / totalLength * 100;

                // Need to release the stream, avoid block.
                fileStream.Close();
                    fileStream.Dispose();
                }
            //Debug.Log(string.Format("Download finished."));

            NetController.curUseDownloadCount--;
                Waitting();
            });
        thread.IsBackground = true;
        thread.Start();
    }

    public void Waitting()
    {
        state = 2;
    }

    public void Close()
    {
        if (null != thread)
            thread.Abort();
        state = 0;
    }

    // Gets the number of bytes downloaded in the current cycle.
    public int GetCycleDownloadCount()
    {
        int count = cycleDownloadCount;
        cycleDownloadCount = 0;
        //		Debug.Log (string.Format("cycleDownloadCount: {0}, count: {1}", cycleDownloadCount, count));
        return count;
    }
    #endregion

    #region PrivateMethod
    /**
     * Get downloading size;
     * 由于CDN策略(分块传输), 在包头中不显示具体长度, 故不使用对应包头信息. 采用UAB List中文件大小;
     * parameter fileUrl - Internet address;
     * */
    private long GetLength(string fileUrl)
    {
        HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(fileUrl);
        request.Method = "HEAD";
        HttpWebResponse res = null;
        try
        {
            res = (HttpWebResponse)request.GetResponse();
        }
        catch (WebException ex)
        {
            Debug.Log(string.Format("GetLength faile. url: {0}, err: {1}", fileUrl, ex.ToString()));
            return -1;
        }
        return res.ContentLength;
    }
    #endregion



}