using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RegionDataBase {

	#region Data
	private RegionDefine.Channel _channelType { get; set;}
	public RegionDefine.Channel channelType 
	{ 
		get
		{
			return _channelType;
		}
		protected set 
		{
			_channelType = value;
			RegionDefine.currentChannel = value;
		}
	}

    public string encryptKey = "Chef";

    public string lastVersion { get; set; }
    private string _version = "";
    public string version
    {
        set
        {
            _version = value;
            AssetBundleDefine.assetBundlePath = string.Format("{0}{1}/", resourceRootUrl, _version);
        }
        get
        {
            return _version;
        }  
    }

    private string _resourceRootUrl = "http://chef.webpatch.sdg-china.com/Test/AssestBundles/";
    public string resourceRootUrl 
	{
		set
		{
            _resourceRootUrl = value;
            AssetBundleDefine.assetBundlePath = string.Format("{0}/{1}/", value, version);
            AssetBundleController.Instance().SetResourceRoot(_resourceRootUrl);
		}
		get
		{
            if (string.IsNullOrEmpty(_resourceRootUrl))
                _resourceRootUrl = string.Format("{0}/../AssestBundles/", Application.dataPath);
            return _resourceRootUrl;
		}
	}

	public string frontEndServerUrl { get; set;}
	public string OBBUrl { get; set;}
	public string announcementUrl { get; set;}
	public string maintenanceUrl { get; set;}
	public string logServerUrl { get; set;}
	public string versionUrl { get; set;}
	public string installationDownloadUrl{ get; set;}
	public string localIP { get; set;}
	public bool isCommandLineVisible = false;
	#endregion

	#region PublicMethod
	public RegionDataBase(RegionDefine.Channel channel)
	{
		channelType = channel;

#if UNITY_EDITOR
		isCommandLineVisible = true;
#else
		isCommandLineVisible = false;
#endif
	}

	public void UpdateFrontEndServerInfo(string data)
	{
		Debug.Log (string.Format ("UpdateFrontEndServerInfo:{0}", data));
		string[] strInfoArray = data.Split (new string[] { "||" }, System.StringSplitOptions.None);

		// Update resources root url & version url.
		if(strInfoArray.Length > 0)
		{
			Debug.Log (string.Format ("resourceRootUrl:{0}", strInfoArray[0]));
			resourceRootUrl = strInfoArray [0];
			versionUrl = string.Format ("{0}/Ini/V.txt");
			announcementUrl = string.Format ("{0}/Ini/ann.txt");	// Default Announcement url.
		}
		// Update announcement url.
		if(strInfoArray.Length > 1)
		{
			Debug.Log (string.Format ("announcementUrl:{0}", strInfoArray[1]));
			if(!string.IsNullOrEmpty(strInfoArray[1]))
				announcementUrl = strInfoArray [1];
		}
		// Update maintenanceUrl url.
		if(strInfoArray.Length > 2)
		{
			Debug.Log (string.Format ("maintenanceUrl:{0}", strInfoArray[2]));
			if(!string.IsNullOrEmpty(strInfoArray[2]))
				maintenanceUrl = strInfoArray [2];
		}
		// Update logServerUrl url.
		if(strInfoArray.Length > 3)
		{
			Debug.Log (string.Format ("logServerUrl:{0}", strInfoArray[3]));
			if(!string.IsNullOrEmpty(strInfoArray[3]))
				logServerUrl = strInfoArray [3];
		}
	}
	#endregion


}
