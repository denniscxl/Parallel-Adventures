using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using GKBase;
using GKMap;
using GKUI;

public class UIFoodHUD : UIBase
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Button TitleBtn;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private GKFood _food;
    private Camera _mainCamera;
    private Camera _uiCamera;
    private Transform _myTransform;
    private float _offestY = 30f;
    // 避免不必要的GC.
    private Vector3 _tmpV3 = Vector3.zero;
    #endregion

    #region PublicMethod
    public void SetData(GKFood food)
    {
        _food = food;
    }

    // 设置阵营图标.
    public void SetCampIcon(int camp)
    {
        
    }

    // 设置城镇名称.
    public void SetVillageName(string name)
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
        
    }

    // 更新hud位置.
    private void SyncPos()
    {
        if (null == _uiCamera)
            return;

        _tmpV3 = _mainCamera.WorldToScreenPoint(_food.myTransform.position);    
        _tmpV3.y = _tmpV3.y + _offestY + (4 - CameraController.Instance().GetZoomlevel()) * 15; 
        _tmpV3.z = 0; 
        _myTransform.position = _tmpV3; 

    }
    #endregion
}
