using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using GKBase;
using GKUI;

public class UILogin : SingletonUIBase<UILogin>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public GameObject LoadingRoot;
        public GameObject LoginRoot;
        public Button StartBtn;
        public Text LoadingContent;
        public Scrollbar LoadingBar;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    #endregion

    #region PublicMethod
    // 判断是否初始化完毕.
    public bool InitLoginFinished()
    {
        return (null != m_ctl);
    }

    // 设置加载进度.
    public void SetLoadingProgress(float progress, string content)
    {
        m_ctl.LoadingBar.size = progress;
        m_ctl.LoadingContent.text = content;
    }

    // 加载完成.
    public void LoadingFinished()
    {
        m_ctl.LoadingRoot.SetActive(false);
        m_ctl.LoginRoot.SetActive(true);
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
        GKUIEventTriggerListener.Get(m_ctl.StartBtn.gameObject).onClick = OnStart;
    }

    private void Init()
    {
        m_ctl.LoadingRoot.SetActive(true);
        m_ctl.LoginRoot.SetActive(false);
        m_ctl.LoadingContent.text = "";
    }

    private void OnStart(GameObject go)
    {
        Close();
        UILobby.Open();
    }
   
    #endregion
}
