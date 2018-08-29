using UnityEngine.UI;
using UnityEngine;
using GKBase;
using GKUI;

public class UIVillageHUD : UIBase
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Text VillageName;
        public Image CampIcon;
        public Button TitleBtn;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private GKVillage _village;
    private Camera _mainCamera;
    private Camera _uiCamera;
    private Transform _myTransform;
    private float _offestY = 30f;
    // 避免不必要的GC.
    private Vector3 _tmpV3 = Vector3.zero;
    #endregion

    #region PublicMethod
    public void SetData(GKVillage village)
    {
        _village = village;
    }

    // 设置阵营图标.
    public void SetCampIcon(int camp)
    {
        
    }

    // 设置城镇名称.
    public void SetVillageName(int name)
    {
        
    }
    #endregion

    #region PrivateMethod
    private void Start()
    {
        Serializable();
        InitListener();
        Init();
    }

    private void Serializable()
    {
        GK.FindControls(this.gameObject, ref m_ctl);
    }

    private void InitListener()
    {
        GKUIEventTriggerListener.Get(m_ctl.TitleBtn.gameObject).onClick = OnClick;
    }

    private void Init()
    {
        _uiCamera = UIController.instance.m_camera;
        _mainCamera = CameraController.Instance().GetMainCamera();
        _myTransform = transform;
    }

    private void Update()
    {
        SyncPos();
    }

    private void OnClick(GameObject go)
    {
        UICreateUnit.Open().SetData(_village.GetAttribute(EObjectAttr.TileID).ValInt);
    }

    // 更新hud位置.
    private void SyncPos()
    {
        if (null == _uiCamera)
            return;

        _tmpV3 = _mainCamera.WorldToScreenPoint(_village.myTransform.position);    
        _tmpV3.y = _tmpV3.y + _offestY + (4 - CameraController.Instance().GetZoomlevel()) * 15; 
        _tmpV3.z = 0; 
        _myTransform.position = _tmpV3;
    }
    #endregion
}
