using UnityEngine;
using System.Collections;
using System.IO;
using GKBase;
using GKMap;
using GKController;
using GKFile;
using GKUI;

public class MyGame : SingletonGame<MyGame> {

	#region Data
	static protected UIController _UIController;
	static public UIController UIController
	{
		get
		{
			if (_UIController == null)
			{
				if (null != UIController.instance)
					_UIController = UIController.instance;
				else
					_UIController = GK.GetOrAddComponent<UIController>(GK.TryLoadGameObject("Prefabs/Manager/UIController"));
			}
			return _UIController;
		}
	}

    static protected bl_HUDText _HUDText;
    static public bl_HUDText HUDText
    {
        get
        {
            if (_HUDText == null)
            {
                _HUDText = GameObject.FindObjectOfType<bl_HUDText>();
            }
            return _HUDText;
        }
    }

    static public bool IsBattle { get { return _instance._bBattle; } set { _instance._bBattle = value; } }
	#endregion

	#region PublicField
    public Size MapSize {get{return _mapSize;}set{_mapSize = value;}}
	#endregion

	#region PrivateField
	private bool bInitVer = false;
	private bool bInitCompleted = false;
    // 当前是否战斗.
    private bool _bBattle = false;
    // 当前地图尺寸.
    private Size _mapSize = Size.Small;
    private UILogin _uiLogin = null;
	#endregion

	#region PublicMethod
	public override void Init()
	{
		base.Init ();

        NetController.Instance().Init(this);
		AssetBundleController.Instance ().Init ();
        AssetBundleController.Instance().OnVersionChanged += VersionChanged;
        GKMapManager.Instance().Init();
        CameraController.Instance().Init();

		InitRoot ();

        _uiLogin = UILogin.Open ();

        MyGame.Instance.StartCoroutine (Initialize ());
	}

	public override void InitRoot()
	{
		base.InitRoot ();
		GK.SetParent(UIController.gameObject, gameObject, false);
	}

	public void Quit()
	{
		Application.Quit ();
	}
    #endregion

    #region PrivateMethod
    private void OnDestroy()
    {
        AssetBundleController.Instance().OnVersionChanged -= VersionChanged;
    }

    /**
	 * Init source station link for testing.
	 * */
    private void InitEphemeralData()
	{
		AssetBundleDefine.assetBundlePath = "http://chef.webpatch.sdg-china.com/Test/AssestBundles/";
        RegionDefine.currentChannel = RegionDefine.Channel.Shanda;
	}

	// Startup process.
	IEnumerator Initialize()
	{
		bInitCompleted = false;

        while (!_uiLogin.InitLoginFinished())
            yield return null;

        // Init SDK.
        _uiLogin.SetLoadingProgress(0.1f, "Initialize SDK.");
        //while(!SDKInterface.Instance().sdkData.isInited)
        //yield return null;
        yield return new WaitForSeconds(0.1f);

        // Init SDK.
        _uiLogin.SetLoadingProgress(0.2f, "Downloading version information.");
        //AssetBundleController.Instance().LoadVersion();
        //while(!AssetBundleController.bLoadVersionFinished)
            //yield return null;
        yield return new WaitForSeconds(0.1f);

		// Init compress ini.
        _uiLogin.SetLoadingProgress(0.3f, "Downloading compress configs.");
		//AssetBundleController.Instance ().DownloadCompressIni ();
		//while(!AssetBundleController.bInitCompressListFinished)
			//yield return null;
        yield return new WaitForSeconds(0.1f);

		// Init compress.
        _uiLogin.SetLoadingProgress(0.4f, "Downloading compress packagers.");
		//AssetBundleController.Instance ().DownloadCompress ();
		//while(!AssetBundleController.bInitCompressFinished)
			//yield return null;
        yield return new WaitForSeconds(0.1f);

		// Init list.
        _uiLogin.SetLoadingProgress(0.5f, "Downloading resource list.");
		//AssetBundleController.Instance ().LoadLocalListInfo ();
		//while(!AssetBundleController.bInitListFinished)
			//yield return null;
        yield return new WaitForSeconds(0.1f);

		// Init essential.
        _uiLogin.SetLoadingProgress(0.6f, "Downloading resources.");
		//AssetBundleController.Instance ().DownloadEssential ();
		//while(!AssetBundleController.bInitEssentialFinished)
			//yield return null;
        yield return new WaitForSeconds(0.1f);

        // Check the client version.
        _uiLogin.SetLoadingProgress(0.8f, "Check version information.");
        //DownloadClientVersion ();
        //while(!false)
        //yield return null;
        yield return new WaitForSeconds(0.1f);

        //AssetBundleController.Instance().DownloadDeferred();

        _uiLogin.SetLoadingProgress(0.9f, "Load player data.");
        DataController.Instance().LoadData();
        // 成就初始化需在数据加载完毕后.
        AchievementController.Instance().Init();
        PlayerController.Instance().Init();

        //_uiLogin.SetTitle("Ini success.");
        yield return new WaitForSeconds(0.1f);
		bInitCompleted = true;
        _uiLogin.LoadingFinished();
	}

	private void DownloadClientVersion()
	{
		// Remove local version.
		string verPath = string.Format ("{0}{1}Version.txt", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath());
		GKFileUtil.DeleteFile (verPath);

		AssetBundleController.Instance().DownloadObject("Version.txt", "Version", OnClientVerFinished);
	}

	/**
	 * The client version is compared with the latest version.
	 * The client master version number is below the latest version number, and the client needs to be updated.
	 * The client's minor version number is below the latest version number, and the user is selectively updated.
	 * The client version number is higher than the latest version number, and the version is unusual.
	**/
	private void OnClientVerFinished(string msg)
	{
		string p = string.Format("{0}{1}Version.txt", AssetBundleDefine.assetBundleCachePath, AssetBundleDefine.GetExternalResourcesFullPath());

		if (File.Exists (p)) {
			StreamReader sr = new StreamReader (p);
			string line;
			if ((line = sr.ReadLine ()) != null) {
				// Init version number.
				string[] v = line.Split ('.');
				int major = int.Parse (v [0]);
				int minor = int.Parse (v [1]);

				string[] localV = RegionDefine.currentData.version.Split ('.');
				int localMajor = int.Parse (localV [0]);
				int localMinor = int.Parse (localV [1]);

				// Compare version.
				if (localMajor > major || ((major == localMajor) && (localMinor > minor))) {
					// Version is unusual.
                    UIMessageBox.ShowUIMessage (string.Format ("Version is unusual. \n {0} \n {1}", line, RegionDefine.currentData.version));
				} else if (major > localMajor) {
					// Client needs to be updated.
                    UIMessageBox.ShowUIMessage (string.Format ("Client needs to be updated."), "Update", UpdateClientVersion, "Update");
				} else if (major == localMajor && minor > localMinor) {
					// User is selectively updated.
                    UIMessageBox.ShowUISelectMessage (string.Format ("The game has a slight update. \n You can ignore the continuation of the game."), 
						"Update", "Cancel", UpdateClientVersion, null, "Update");
					bInitVer = true;
				} else {
					bInitVer = true;
				}
			}
			sr.Close ();
		} else {
			Debug.LogError ("[OnClientVerFinished] Version.txt is missing.");
		}
	}

	private void UpdateClientVersion()
	{
		if (!string.IsNullOrEmpty (RegionDefine.currentData.installationDownloadUrl)) {
			Application.OpenURL (RegionDefine.currentData.installationDownloadUrl);
			Quit ();
		} else {
			Debug.LogError ("[UpdateClientVersion] installationDownloadUrl is NULL.");
		}
	}

    private void Update()
    {
        if(null != CameraController.Instance())
            CameraController.Instance().Update();

        if (null != DataController.Instance())
            DataController.Instance().Update();
    }

    private void VersionChanged(string ver)
    {
        RegionDefine.currentData.version = ver;
    }
    #endregion
}
