using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using GKBase;
using GKEncryption;
using GKFile;

public class GTAssetBundleBuilderWindow : EditorWindow
{

    #region PublicField
    public class Depend
    {
        public string channel;
        public bool show;

        public Depend(string c)
        {
            channel = c;
            show = true;
        }

        public void Draw()
        {

            if (!show)
                return;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Button(channel);
            Color c = GUI.color;
            GUI.color = Color.red;
            GUILayoutOption[] option = { GUILayout.Width(20), GUILayout.Height(20) };
            if (GUILayout.Button("X", option))
            {
                for (int i = dependenceList.Count - 1; i >= 0; i--)
                {
                    if (string.Equals(dependenceList[i].channel, channel))
                    {
                        show = false;
                        break;
                    }
                }
            }
            GUI.color = c;
            EditorGUILayout.EndHorizontal();
            GKEditor.DrawInspectorSeperator();
        }

        static public void AddDependence(string channel)
        {

            // Non benchmark channel.
            if (string.Equals(channel.ToLower(), "dev"))
            {
                return;
            }

            foreach (var d in dependenceList)
            {
                // If current is in use, and the channel is the same.
                if (d.show && string.Equals(channel.ToLower(), d.channel.ToLower()))
                {
                    return;
                }
            }

            // Add new element. 
            // To ensure the addition sequence, the used channel object is released at Reset.
            Depend depend = new Depend(channel);
            dependenceList.Add(depend);

        }

        static private Depend GetEmptyDepend()
        {
            foreach (var d in dependenceList)
            {
                if (!d.show)
                {
                    return d;
                }
            }
            return null;
        }
    }
    #endregion

    #region PrivateField
    private int selected = 0;
    private int dependSele = 0;
    private Dictionary<string, AssetBundleInfo> assetInfoList = new Dictionary<string, AssetBundleInfo>();     // Write to cache, and output to file. Increase packing speed.
    private Dictionary<string, AssetBundleInfo> localInfoList = new Dictionary<string, AssetBundleInfo>();     // Local info list.
    private Dictionary<string, AssetBundleInfo> removeAssetList = new Dictionary<string, AssetBundleInfo>();   // Remove info list.
    private Dictionary<string, AssetBundleInfo> bundleInfoList = new Dictionary<string, AssetBundleInfo>();
    private Dictionary<string, AssetBundleInfo> removeBundleDict = new Dictionary<string, AssetBundleInfo>();
    private Dictionary<string, List<string>> bdd = new Dictionary<string, List<string>>();
    private bool autoGenPackage = false;
    static private List<Depend> dependenceList = new List<Depend>();
    #endregion

    #region PublicMethod
    public static void MenuItem_Window()
    {
        var w = EditorWindow.GetWindow<GTAssetBundleBuilderWindow>("Asset Bundle");
        w.autoRepaintOnSceneChange = true;
        // Window size is not allowed.
        w.minSize = new Vector2(800, 500);
        w.maxSize = new Vector2(800, 500);
        w.Show();
    }
    #endregion

    #region PrivateMethod
    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        {
            GUILayout.Label("Select the channel.");
            string[] channels = GK.EnumNames<RegionDefine.Channel>();
            selected = EditorGUILayout.Popup(selected, channels);
            RegionDefine.Channel[] channelEnum = GK.EnumValues<RegionDefine.Channel>();
            RegionDefine.Channel curRegion = RegionDefine.GetRegionType(channelEnum[selected]);
            RegionDefine.currentChannel = curRegion;    // Set region for output path.
            string region = curRegion.ToString();
            GUILayout.Label(string.Format("Region name: [ {0} ]", region));
            EditorGUILayout.BeginHorizontal();
            dependSele = EditorGUILayout.Popup(dependSele, channels);
            if (GUILayout.Button("Add dependence channel"))
            {
                Depend.AddDependence(RegionDefine.GetRegionType(channelEnum[dependSele]).ToString());
            }
            EditorGUILayout.EndHorizontal();
            GKEditor.DrawInspectorSeperator();
            // Draw depend items.
            foreach (var d in dependenceList)
            {
                d.Draw();
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Last version");
            RegionDefine.currentData.lastVersion = GUILayout.TextField(RegionDefine.currentData.lastVersion);
            GUILayout.Label("Current version");
            RegionDefine.currentData.version = GUILayout.TextField(RegionDefine.currentData.version);
            EditorGUILayout.EndHorizontal();
            autoGenPackage = GUILayout.Toggle(autoGenPackage, "Automatic generation of packages.");
            if (GUILayout.Button("Reset"))
            {
                selected = 0;
                autoGenPackage = true;
                dependenceList.Clear();
            }
            if (GUILayout.Button("Build"))
            {
                BuildExternalResources();
            }
        }
        EditorGUILayout.EndVertical();
    }

    /**
     *	Build external resource step:
     *	1. Filter invalid files.
     * 	2. Generate list, compare and eliminate non differential files. 
     * 	3. Create all asset bundles output Directory.
     * 	4. Delete & move asset bundle to target folder.
     */
    public void BuildExternalResources()
    {
        //Debug.Log (string.Format ("BuildExternalResources begin time: {0}", Time.realtimeSinceStartup));
        // Reset output folder.
        AssetBundleDefine.assetBundlePath = string.Format("{0}/../AssestBundles/{1}/", Application.dataPath, RegionDefine.currentData.version);
        CreateVersionFile();
        CopyVersionDirectory();
        ReloadLocalInfoList();
        bdd.Clear();
        // Init q list.
        Queue<string> q = new Queue<string>();

        // One by one Build.
        foreach (var t in GK.EnumValues<AssetBundleDefine.AssetBundleDownloadType>())
        {
            // Ini files no need build bundles.
            if (AssetBundleDefine.AssetBundleDownloadType.Ini == t || AssetBundleDefine.AssetBundleDownloadType.Count == t)
                continue;
            List<string> lInput = new List<string>();
            lInput.Clear();
            q.Clear();
            // Create directory.
            string tDirPath = string.Format("{0}{1}{2}/", AssetBundleDefine.assetBundlePath, AssetBundleDefine.GetExternalResourcesFullPath(), t.ToString());
            GKFileUtil.CreateDirectory(tDirPath);
            // Push root region.
            q.Enqueue(RegionDefine.Channel.Dev.ToString());
            // Push dependence region.
            foreach (var v in dependenceList)
            {
                if (v.show)
                    q.Enqueue(v.channel);
            }
            // Push current region.
            if (RegionDefine.currentChannel != RegionDefine.Channel.Dev && q.Contains(RegionDefine.currentChannel.ToString()))
            {
                Debug.LogError(
                    string.Format("BuildExternalResources failure. err code: Dependence contaions current region. region: {0}"
                                  , RegionDefine.currentChannel.ToString())
                );
                return;
            }
            if (RegionDefine.currentChannel != RegionDefine.Channel.Dev)
                q.Enqueue(RegionDefine.currentChannel.ToString());
            string[] files = GetAllAssetsByChannels(q, t.ToString()).Values.ToArray();
            // Filter invalid files.
            foreach (var f in files)
            {
                if (!GKFileUtil.FilterInvalidFiles(f))
                    continue;
                string relativePath = GKFileUtil.GetAssetPath(f);
                string guid = AssetDatabase.AssetPathToGUID(relativePath);
                // Compare the file's Attributes (new and LastWriteTime).  to determine whether to generate assetbundle.
                if (!localInfoList.ContainsKey(guid))
                {
                    lInput.Add(relativePath);
                }
                else if (localInfoList.ContainsKey(guid))
                {
                    FileInfo fi = new FileInfo(relativePath);
                    FileInfo fmetai = new FileInfo(string.Format("{0}.meta", relativePath));
                    if (0 < GK.CompareDateTime(fi.LastWriteTime, localInfoList[guid].time)
                        || 0 < GK.CompareDateTime(fmetai.LastWriteTime, localInfoList[guid].time))
                    {
                        lInput.Add(relativePath);
                    }
                }
                if (removeAssetList.Keys.Contains(guid))
                {
                    removeAssetList.Remove(guid);
                }
                string bn = GKFileEditor.GetBundleName(relativePath);
                if (removeBundleDict.Keys.Contains(bn))
                {
                    removeBundleDict.Remove(bn);
                }
            }
            // Create asset bundle output directory.
            string optputPath = string.Format("{0}{1}{2}/", AssetBundleDefine.assetBundlePath, AssetBundleDefine.GetExternalResourcesFullPath(), "Temp");
            GKFileUtil.CreateDirectory(optputPath);
            // Gets the Bundle name of the modified files.
            foreach (var path in lInput)
            {
                string tBundleName = GKFileEditor.GetBundleName(path);
                if (!bdd.ContainsKey(tBundleName))
                {
                    List<string> l = new List<string>();
                    bdd.Add(tBundleName, l);
                }
                bdd[tBundleName].Add(path);
            }
            // Adding the same bundle but not modifying files.
            foreach (var f in files)
            {
                if (!GKFileUtil.FilterInvalidFiles(f))
                    continue;
                string tBundleName = GKFileEditor.GetBundleName(f);
                if (ExistDict(bdd, tBundleName, f))
                    continue;
                if (bdd.ContainsKey(tBundleName))
                {
                    bdd[tBundleName].Add(GKFileUtil.GetAssetPath(f));
                }
            }
        }
        // Build asset bundles.
        AssetBundleBuild[] abbs = null;
        if (0 < bdd.Count)
            abbs = GKAssetBundleBuilder.BuildMapAssetBundles(bdd, EditorUserBuildSettings.activeBuildTarget);
        // Move new asset bundle to target folder.
        if (null != abbs && abbs.Length > 0)
        {
            Debug.Log(string.Format("builded assetbundle count: {0}", abbs.Length));
            foreach (var m in abbs)
            {
                string moveFrom = string.Format("{0}{1}{2}/{3}.{4}", AssetBundleDefine.assetBundlePath, AssetBundleDefine.GetExternalResourcesFullPath(),
                    "Temp", m.assetBundleName.ToLower(), m.assetBundleVariant);
                string moveTo = string.Format("{0}{1}{2}/{3}.{4}", AssetBundleDefine.assetBundlePath, AssetBundleDefine.GetExternalResourcesFullPath(),
                    AssetBundleDefine.GetAbDownloadTypeByPath(m.assetNames[0]).ToString(), m.assetBundleName.ToLower(), m.assetBundleVariant);
                string manifestMoveFrom = moveFrom + ".manifest";
                string manifestMoveTo = moveTo + ".manifest";
                try
                {
                    // Delete file.
                    if (File.Exists(moveTo))
                    {
                        FileUtil.DeleteFileOrDirectory(moveTo);
                    }
                    if (File.Exists(manifestMoveTo))
                    {
                        FileUtil.DeleteFileOrDirectory(manifestMoveTo);
                    }
                    // Move asset bundle.
                    FileUtil.MoveFileOrDirectory(moveFrom, moveTo);
                    // Move asset bundle manifest.
                    if (File.Exists(manifestMoveFrom))
                    {
                        FileUtil.DeleteFileOrDirectory(manifestMoveFrom);
                    }
                    //FileUtil.MoveFileOrDirectory (manifestMoveFrom, manifestMoveTo);
                }
                catch (System.Exception e)
                {
                    Debug.LogError(string.Format("BuildExternalResources Remove Directory or files failure. Exception: {0}", e));
                }
            }
        }
        // Add or update info list element.
        foreach (var info in bdd)
        {
            foreach (var p in info.Value)
            {
                ModifyAssetBundleInfo(p, ref assetInfoList);
            }
            if (info.Value.Count > 0)
            {
                string path = string.Format("{0}{1}/{2}.assetbundle", AssetBundleDefine.GetDiffAssetBundleRegionPath(true),
                    AssetBundleDefine.GetAbDownloadTypeByPath(info.Value[0]), info.Key);
                ModifyAssetBundleInfo(path, ref bundleInfoList, false);
            }
        }
        // Remove.
        foreach (string k in removeAssetList.Keys)
        {
            if (assetInfoList.ContainsKey(k))
            {
                assetInfoList.Remove(k);
            }
        }
        foreach (var rb in removeBundleDict)
        {
            if (bundleInfoList.ContainsKey(rb.Key))
            {
                bundleInfoList.Remove(rb.Key);
            }
            string p = string.Format("{0}{1}{2}/{3}.assetbundle", AssetBundleDefine.assetBundlePath,
                AssetBundleDefine.GetExternalResourcesFullPath(), rb.Value.type.ToString(), rb.Value.assetBundleName);
            GKFileUtil.DeleteFile(p);
            string mf = p + ".manifest";
            GKFileUtil.DeleteFile(mf);
        }
        // Update infolist.
        foreach (var t in GK.EnumValues<AssetBundleDefine.AssetBundleDownloadType>())
        {
            if (AssetBundleDefine.AssetBundleDownloadType.Ini == t || AssetBundleDefine.AssetBundleDownloadType.Count == t)
                continue;
            string infoPath = string.Format("{0}/Ini", AssetBundleDefine.GetDiffAssetBundleRegionPath(true));
            GKFileUtil.CreateDirectory(infoPath);
            string finalPath = string.Format("{0}/{1}Build.list", infoPath, t.ToString());
            OutputInfoToFile(finalPath, ref assetInfoList);
            finalPath = string.Format("{0}/{1}Info.list", infoPath, t.ToString());
            OutputInfoToFile(finalPath, ref bundleInfoList);
        }
        // Zip.
        if (autoGenPackage)
            GKAssetBundleBuilder.GenFullPackages();
    }

    private bool ExistDict(Dictionary<string, List<string>> dict, string key, string path)
    {
        if (!dict.Keys.Contains(key))
            return false;
        string rp = GKFileUtil.GetAssetPath(path);
        foreach (var p in dict[key])
        {
            if (p.Equals(rp))
                return true;
        }
        return false;
    }

    // Create version file to resource path;
    private void CreateVersionFile()
    {
        string optputPath = string.Format("{0}", Application.dataPath + "/../AssestBundles/");
        //		Debug.Log (string.Format("optputPath: {0}", optputPath));
        if (!Directory.Exists(optputPath))
        {
            Directory.CreateDirectory(optputPath);
        }
        optputPath += "Version.txt";

        using (StreamWriter sw = new StreamWriter(optputPath))
        {
            if (null != RegionDefine.currentData)
                sw.Write(RegionDefine.currentData.version);
            else
                Debug.LogError(string.Format("Please setting region version."));

            sw.Close();
        }
    }

    private void CopyVersionDirectory()
    {
        if (RegionDefine.currentData.version.Equals(RegionDefine.currentData.lastVersion))
            return;
        string target = string.Format("{0}{1}/", Application.dataPath + "/../AssestBundles/", RegionDefine.currentData.version);
        string src = string.Format("{0}{1}/", Application.dataPath + "/../AssestBundles/", RegionDefine.currentData.lastVersion);
        if (Directory.Exists(target))
            GKFileUtil.DeleteDirectory(target);
        if (Directory.Exists(src))
            GKFileUtil.CopyDirectory(src, target, true);
    }

    // Modify assetbundleInfo list in RAM;
    private void ModifyAssetBundleInfo(string filePath, ref Dictionary<string, AssetBundleInfo> list, bool isAsset = true)
    {
        AssetBundleInfo info = new AssetBundleInfo();
        TimeSpan timeSpan = DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc));
        FileStream reader = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        string rp = GKFileUtil.GetAssetPath(filePath);
        info.type = AssetBundleDefine.GetAbDownloadTypeByPath(filePath);
        info.assetBundleName = isAsset ? GKFileEditor.GetBundleName(rp) : Path.GetFileNameWithoutExtension(filePath);
        info.version = Convert.ToInt32(timeSpan.TotalSeconds);
        info.md5 = GKMd5Sum.Calc(reader);
        info.size = reader.Length;
        info.priority = 0;
        info.time = string.Format("{0:G}", DateTime.Now);
        info.srcMd5 = "";
        info.isDownload = 0;
        string guid = isAsset ? AssetDatabase.AssetPathToGUID(rp) : info.assetBundleName;
        list[guid] = info;
        reader.Close();
    }

    // Write to file, Create list.
    private void OutputInfoToFile(string outputPath, ref Dictionary<string, AssetBundleInfo> dict)
    {
        AssetBundleController.Instance().OutputInfoToFile(outputPath, dict);
    }

    // Reload local list & info list.
    private void ReloadLocalInfoList()
    {
        localInfoList.Clear();
        assetInfoList.Clear();
        removeAssetList.Clear();
        bundleInfoList.Clear();
        removeBundleDict.Clear();
        foreach (var t in GK.EnumValues<AssetBundleDefine.AssetBundleDownloadType>())
        {
            if (AssetBundleDefine.AssetBundleDownloadType.Ini == t || AssetBundleDefine.AssetBundleDownloadType.Count == t)
                continue;
            string path = string.Format("{0}/Ini/{1}Build.list", AssetBundleDefine.GetDiffAssetBundleRegionPath(true, true), t.ToString());
            if (File.Exists(path))
            {
                FileStream reader = new FileStream(path, FileMode.Open, FileAccess.Read);
                // Stream convert to string.
                int len = (int)reader.Length;
                byte[] array = new byte[len];
                int count = reader.Read(array, 0, array.Length);
                string content = System.Text.Encoding.UTF8.GetString(array);
                // Get lines.
                string[] lines = content.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (string.IsNullOrEmpty(lines[i]))
                        continue;
                    AssetBundleInfo info = new AssetBundleInfo(lines[i]);
                    string[] elements = lines[i].Split('|');
                    localInfoList[elements[0]] = info;
                    assetInfoList[elements[0]] = info;
                    removeAssetList[elements[0]] = info;
                }
                reader.Close();
            }
            path = string.Format("{0}/Ini/{1}Info.list", AssetBundleDefine.GetDiffAssetBundleRegionPath(true, true), t.ToString());
            if (File.Exists(path))
            {
                FileStream reader = new FileStream(path, FileMode.Open, FileAccess.Read);
                // Stream convert to string.
                int len = (int)reader.Length;
                byte[] array = new byte[len];
                int count = reader.Read(array, 0, array.Length);
                string content = System.Text.Encoding.UTF8.GetString(array);
                // Get lines.
                string[] lines = content.Split('\n');
                for (int i = 0; i < lines.Length; i++)
                {
                    if (string.IsNullOrEmpty(lines[i]))
                        continue;
                    AssetBundleInfo info = new AssetBundleInfo(lines[i]);
                    string[] elements = lines[i].Split('|');
                    bundleInfoList[elements[0]] = info;
                    removeBundleDict[elements[0]] = info;
                }
                reader.Close();
            }
        }
    }

    /**
     * Get all assets based on channels.
     * */
    static private Dictionary<string, string> GetAllAssetsByChannels(Queue<string> channels, string subDirectory)
    {
        Dictionary<string, string> ret = new Dictionary<string, string>();
        Dictionary<string, string> temp = new Dictionary<string, string>();

        // If the number of channels is less than 2, Return current channel's assets.
        if (1 >= channels.Count)
        {
            GetAllAssetsByChannel(channels.Dequeue(), subDirectory, ret);
            Debug.Log(string.Format("GetAllAssetsByChannels channels only one. assets count: {0}", ret.Count));
            return ret;
        }

        while (channels.Count != 0)
        {
            GetAllAssetsByChannel(channels.Dequeue(), subDirectory, temp);
            CombinedChannelAssets(ret, temp);
        }

        return ret;
    }

    /**
     * Combined channel assets.
     * */
    static private void CombinedChannelAssets(Dictionary<string, string> ret, Dictionary<string, string> temp)
    {
        foreach (var f in temp)
        {
            ret[f.Key] = f.Value;
        }
    }

    /**
     * Get all assets based on channel.
     * */
    static private void GetAllAssetsByChannel(string channel, string subDirectory, Dictionary<string, string> l)
    {
        l.Clear();
        // The Resources directory is the toggle package size folder that exists only in Dev channels.
        string resource = (string.Equals(channel, "Dev")) ? "Transfer/" : "";
        string path = string.Format("{0}/ExternalResources/{1}/{2}{3}/", Application.dataPath, channel, resource, subDirectory);
        var files = GKFileUtil.GetFilesInDirectory(path);
        foreach (var f in files)
        {
            // If the file is legal, add it to the dictionary.
            if (GKFileUtil.FilterInvalidFiles(f))
            {
                string rp = GKFileUtil.GetAssetPath(f);
                string guid = AssetDatabase.AssetPathToGUID(rp);
                l[guid] = f;
            }
        }
    }
    #endregion
}