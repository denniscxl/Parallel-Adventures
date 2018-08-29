using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using UnityEngine.Networking;

public class RegionDefine {
	public enum Channel
	{
		Dev = 0,
		MainLand = 1,
        Shanda = 2,
	}
    static private Channel _currentChannel = Channel.Shanda;	// Current Region.
	static public Channel currentChannel {
		get
		{ 
			return _currentChannel;
		}
		set
		{
			if (_currentChannel != value) {
				_currentChannel = value;
				ReplaceRegionData ();
			}
		}
	}
    static public RegionDataBase _currentData = null;
	static public RegionDataBase currentData 
    {
        get
        {
            if(null == _currentData)
                ReplaceRegionData();
            return _currentData;
        }
        set
        {
            _currentData = value;
        }
    }
	// Get region number. Default return Dev;
	public static Channel GetRegionType(Channel channel)
	{
        return channel;
	}

	public static void ReplaceRegionData()
	{
		Channel t = GetRegionType (currentChannel);
		switch (t) 
		{
		case Channel.Dev:
			currentData = new RegionDev (currentChannel);
			break;
		case Channel.MainLand:
			currentData = new RegionMainland (currentChannel);
			break;
        case Channel.Shanda:
            currentData = new RegionShanda(currentChannel);
            break;
		default:
            currentData = new RegionDev (Channel.Dev);
			break;
		}
	}

	#if UNITY_EDITOR
	public static BuildTarget GetCurrentBuildTarget()
	{
		switch (Application.platform) 
		{
		case RuntimePlatform.Android:
			return BuildTarget.Android;
		case RuntimePlatform.IPhonePlayer:
		case RuntimePlatform.OSXEditor:
		case RuntimePlatform.OSXPlayer:
			return BuildTarget.iOS;
		default:
			return BuildTarget.StandaloneWindows;
		}
	}
	#endif
}
