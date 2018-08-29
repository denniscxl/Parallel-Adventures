using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using GKFile;

public class GKBuilder
{

    #region PublicField
    #endregion

    #region PrivateField
    #endregion

    #region PublicMethod
    static public void Building(RegionDefine.Channel channel, bool bLarger, bool bObfuscation, bool bDebug = false)
    {
        // Not execute.
        //		GKEditor.DifferentialResourcesCoverage (channel);

        if (bLarger)
        {
            // Rename only in non large package mode. 
            RenameRegionResources(channel, false);
        }

        if (bObfuscation)
        {
            // Obfuscation does not work at the moment because version recovery is not possible.
            //			string path = string.Format ("{0}/Scripts", Application.dataPath);
            //			GKCodeObfuscation.ScoureCodeObfusation (path);
        }

        string targetPathName = string.Format("{0}_{1}", PlayerSettings.productName, PlayerSettings.bundleVersion);

        PlayerSettings.productName = "GameKit";
        PlayerSettings.applicationIdentifier = "";
        PlayerSettings.bundleVersion = RegionDefine.currentData.version;
#if UNITY_IOS
        //		targetPathName	= "";
        //PlayerSettings.shortBundleVersion = "";
        PlayerSettings.targetIOSGraphics = TargetIOSGraphics.OpenGLES_2_0;
        PlayerSettings.SetPropertyInt("ScriptingBackend", (int)ScriptingImplementation.IL2CPP, BuildTarget.iOS);
        //PlayerSettings.SetPropertyInt("Architecture", (int)iPhoneArchitecture.Universal, BuildTarget.iOS);
        PlayerSettings.iOS.targetOSVersion = iOSTargetOSVersion.iOS_6_0;
#elif UNITY_ANDROID
		targetPathName += ".apk";
		PlayerSettings.Android.bundleVersionCode = 1;
		//Set keystore and its password
		PlayerSettings.Android.keystoreName = "";
		PlayerSettings.Android.keystorePass = "";
		PlayerSettings.Android.keyaliasName = "";
		PlayerSettings.Android.keyaliasPass = "";
		Debug.Log(string.Format("keystoreName:{0},keystorePass:{1},keyaliasName:{2},keyaliasPass:{3}", PlayerSettings.Android.keystoreName, PlayerSettings.Android.keystorePass, PlayerSettings.Android.keyaliasName, PlayerSettings.Android.keyaliasPass));
		// Split Application Binary
		PlayerSettings.Android.useAPKExpansionFiles = "";
#endif

        // Collect all the scenes in build and setup build options.
        List<string> levels = new List<string>();
        for (int i = 0, end = EditorBuildSettings.scenes.Length; i < end; i++)
        {
            if (EditorBuildSettings.scenes[i].enabled)
            {
                levels.Add(EditorBuildSettings.scenes[i].path);
            }
        }

        BuildOptions buildOptions = bDebug
            ? BuildOptions.SymlinkLibraries | BuildOptions.Development | BuildOptions.ConnectWithProfiler | BuildOptions.AllowDebugging
            : BuildOptions.None;

        // Here we go!
#if UNITY_2018_1_6
        var report = BuildPipeline.BuildPlayer(levels.ToArray(), targetPathName, EditorUserBuildSettings.activeBuildTarget, buildOptions);
        //if (err.Length == 0 && (File.Exists(targetPathName) || Directory.Exists(targetPathName)))
        //{
            Debug.Log("Unity build success.（" + targetPathName + "）");
        //}
        //else
        //{
        //    Debug.LogError(string.Format("Unity build faliure！（{0}). err: {1}", targetPathName, err));
        //}
#else
        string err = BuildPipeline.BuildPlayer(levels.ToArray(), targetPathName, EditorUserBuildSettings.activeBuildTarget, buildOptions);
        if (err.Length == 0 && (File.Exists(targetPathName) || Directory.Exists(targetPathName))) {
            Debug.Log("Unity build success.（" + targetPathName + "）");
        } else {
            Debug.LogError(string.Format("Unity build faliure！（{0}). err: {1}", targetPathName, err));
        }
#endif


		if (bLarger) {
			// Restore.
			RenameRegionResources (channel, true);
		}
	}


#endregion

#region PrivateMethod
	[MenuItem("GK/Build Game")]
	static private void MenuItem_BuildGame()
	{
		if (EditorUtility.DisplayDialog ("Build Game", "Do you want build game?", "Of course", "No, Thanks")) {
			Debug.Log ("Build game.");
			GKBuilderWindow.MenuItem_Window ();
		} else {
			Debug.Log ("Cancel.");
		}
	}

    /**
     * Rename the current region resource folder name, depending on the package type.
     * */
    static private void RenameRegionResources(RegionDefine.Channel channel, bool bChangeToRes)
    {
        AssetDatabase.StartAssetEditing();
        var c = RegionDefine.GetRegionType(channel);
        string srcFolderName = bChangeToRes ? "TempRes" : "Resources";
        string destFolderName = bChangeToRes ? "Resources" : "TempRes";
        string resPath = string.Format("Assets/ExternalResources/{0}/{1}/", c.ToString(), srcFolderName);

        string result = AssetDatabase.RenameAsset(resPath, destFolderName);
        if (!string.IsNullOrEmpty(result))
        {
            Debug.LogError(string.Format("RenameRegionResources result failure: {0}", result));
        }
        AssetDatabase.StopAssetEditing();
        AssetDatabase.Refresh();
    }

    /**
     * Differential resource coverage function.
     * */
    static private void DifferentialResourcesCoverage(RegionDefine.Channel channel)
    {
        AssetDatabase.StartAssetEditing();
        Debug.Log("DifferentialResourcesCoverage Start.");

        if (RegionDefine.Channel.Dev == channel)
            return;

        // Initializes the destination directory, and base directory path.
        string dirPath = string.Format("{0}/ExternalResource/{1}/", Application.dataPath, channel.ToString());
        string[] files = GKFileUtil.GetFilesInDirectory(dirPath);
        foreach (var f in files)
        {
            //Debug.Log("DifferentialResourceCoverage f: " + f);
            string p = GKFileUtil.GetAssetPath(f);
            if (!string.IsNullOrEmpty(p) && GKFileUtil.FilterInvalidFiles(p, true))
            {
                //Debug.Log("DifferentialResourceCoverage p: " + p);
                // Replace the target area name for Dev.
                string devPath = p.Replace(channel.ToString(), "Dev");
                // If Deferred or Essential resources, Add Resources/.
                if (p.Contains("Deferred/"))
                {
                    int index = p.IndexOf("Deferred/");
                    devPath = p.Insert(index, "Resources/");
                }
                if (p.Contains("Essential/"))
                {
                    int index = p.IndexOf("Essential/");
                    devPath = p.Insert(index, "Resources/");
                }

                // Copy
                if (!AssetDatabase.CopyAsset(p, devPath))
                {
                    Debug.Log("Couldn't copy the " + devPath);
                }
            }
        }

        Debug.Log("DifferentialResourceCoverage Finished.");
        AssetDatabase.StopAssetEditing();
        // Refresh.
        AssetDatabase.Refresh();
    }
#endregion

}
