using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System;
using GKBase;
using GKFile;

public class GKAdjustAssetBundleFolder : EditorWindow
{
    static private List<string> srcPath = new List<string>();
    static private int _progressCount = 0;
    static private int _curProgress = 0;
    static private int _stepCount = 4;
    static private int _step = 0;

    [MenuItem("GK/Build Asset Bundles/Adjust AssetBundle Folder", false, GKEditorConfiger.MenuItemPriorityA)]
    static void MenuItem_AdjustAssetBundleFolder()
    {
        CreateFolder();
        MoveAssets();
        SetAssetBundleName();
        MovePrefabs();
    }

    static void OnGUI()
    {
        if (_step < _stepCount && _curProgress < _progressCount)
        {
            EditorUtility.DisplayProgressBar("Progress Bar", string.Format("{0}/{1}", _curProgress, _progressCount), (float)(_curProgress / _progressCount));
        }
    }

    // Create folder.
    static private void CreateFolder()
    {
        _curProgress = 0;
        var files = GK.EnumNames<AssetBundleDefine.AssetBundleDownloadType>();
        _progressCount = files.Length - 2;
        for (int i = 1; i < files.Length - 2; i++)
        {
            GKFileUtil.CreateDirectory(AssetBundleDefine.GetDiffAssetBundleRegionPath(false) + files[i] + "/");
            _curProgress++;
        }
        AssetDatabase.Refresh();
        _step++;
    }

    // Move arts from asset bundle folder.
    static private void MoveAssets()
    {
        _curProgress = 0;
        srcPath.Clear();
        srcPath.Add(string.Format("{0}/UserInterface/Data/", Application.dataPath));
        foreach (var p in srcPath)
        {
            var assets = GKFileUtil.GetFilesInDirectory(p);
            _progressCount = assets.Length;
            foreach (var a in assets)
            {
                _curProgress++;
                if (!GKFileUtil.IsTexture(a))
                    continue;
                string assetPath = GKFileUtil.GetAssetPath(a);
                string subDirectory = GetSubDirectory(assetPath);
                if (string.IsNullOrEmpty(subDirectory))
                    continue;
                string targetDirectory = string.Format("{0}Essential/{1}/", AssetBundleDefine.GetDiffAssetBundleRegionPath(false), subDirectory);
                if (!Directory.Exists(targetDirectory))  // Create directory.
                    GKFileUtil.CreateDirectory(targetDirectory);

                // Remove Packing tag.
                TextureImporter ti = AssetImporter.GetAtPath(assetPath) as TextureImporter;
                ti.spritePackingTag = "";
                AssetDatabase.Refresh();

                string targetPath = string.Format("{0}{1}", targetDirectory, Path.GetFileName(assetPath));
                targetPath = GKFileUtil.GetAssetPath(targetPath);
                AssetDatabase.MoveAsset(assetPath, targetPath);
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        _step++;
    }

    // Custom function.
    static private string GetSubDirectory(string path)
    {
        string strDefault = "";
        string[] suffixArray = path.Split('/');
        if (suffixArray.Length > 4)
            return suffixArray[4];
        else
            return strDefault;
    }

    // Set assetbundle name by directory name.
    static private void SetAssetBundleName()
    {
        _curProgress = 0;
        var files = GKFileUtil.GetFilesInDirectory(string.Format("{0}Essential/", AssetBundleDefine.GetDiffAssetBundleRegionPath(false)));
        _progressCount = files.Length;
        foreach (var f in files)
        {
            _curProgress++;
            string ap = GKFileUtil.GetAssetPath(f);
            AssetImporter bundleImporter = AssetImporter.GetAtPath(ap);
            if (null == bundleImporter)
                continue;
            string directoryName = GKFileUtil.GetDirctoryName(f);
            bundleImporter.assetBundleName = directoryName;
            bundleImporter.assetBundleVariant = "assetbundle";
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        _step++;
    }

    // Prefabs move to resources.
    static private void MovePrefabs()
    {
        _curProgress = 0;
        srcPath.Clear();
        srcPath.Add(string.Format("{0}/UserInterface/Data/", Application.dataPath));
        foreach (var p in srcPath)
        {
            var assets = GKFileUtil.GetFilesInDirectory(p);
            _progressCount = assets.Length;
            foreach (var a in assets)
            {
                _curProgress++;
                if (!GKFileUtil.IsPrefab(a) || string.Equals(GKFileUtil.GetDirctoryName(a), "Resources"))
                    continue;
                string dir = string.Format("{0}/../Resources/", Path.GetDirectoryName(a));
                string targetPath = string.Format("{0}{1}", dir, Path.GetFileName(a));
                if (!Directory.Exists(dir))
                    GKFileUtil.CreateDirectory(dir);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                string srcPath = GKFileUtil.GetAssetPath(a);
                targetPath = GKFileUtil.GetAssetPath(targetPath);
                // Adjust directory path， AssetDataBase does not support.. /.
                targetPath = targetPath.Replace("Prefabs/../", "");
                targetPath = targetPath.Replace("Prefab/../", "");
                targetPath = targetPath.Replace("Effects/../", "");
                string ret = AssetDatabase.MoveAsset(srcPath, targetPath);
                if (!string.IsNullOrEmpty(ret))
                    Debug.LogError(string.Format("MovePrefabs ret: {0}. src: {1}, target: {2}", ret, srcPath, targetPath));
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        _step++;
    }
}