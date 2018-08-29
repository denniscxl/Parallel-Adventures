using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using GKBase;
using GKUI;

public class UILoading : SingletonUIBase<UILoading>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Text LoadingContent;
        public Scrollbar LoadingBar;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private LoadingType _type = LoadingType.Lobby;
    #endregion

    #region PublicMethod
    // 设置加载进度.
    public void SetLoadingProgress(float progress, string content)
    {
        m_ctl.LoadingBar.size = progress;
        m_ctl.LoadingContent.text = content;
    }

    public void Next(LoadingType type)
    {
        _type = type;
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
        
    }

    private void Init()
    {
        MyGame.Instance.StartCoroutine(Loading());
        UIController.instance.ShowHUD(false,true);
    }

    public virtual IEnumerator Loading()
    {
        float progress = 0f;
        yield return new WaitForSeconds(0.1f);
        progress += 0.1f;
        SetLoadingProgress(progress, "Loading .");
        yield return new WaitForSeconds(0.1f);
        progress += 0.1f;
        SetLoadingProgress(progress, "Loading ..");
        yield return new WaitForSeconds(0.1f);
        progress += 0.1f;
        SetLoadingProgress(progress, "Loading ...");
        yield return new WaitForSeconds(0.1f);
        progress += 0.1f;
        SetLoadingProgress(progress, "Loading ... .");
        yield return new WaitForSeconds(0.1f);
        progress += 0.1f;
        SetLoadingProgress(progress, "Loading ... ..");
        yield return new WaitForSeconds(0.1f);
        progress = 1f;
        SetLoadingProgress(progress, "Loading ... ...");

        LoadingScene();
        Close();
    }

    private void LoadingScene()
    {
        switch(_type)
        {
            case LoadingType.Lobby:
                SceneManager.LoadSceneAsync(2);
                UILobby.Open();
                break;
            case LoadingType.Fight:
                SceneManager.LoadSceneAsync(1);
                UIController.instance.ShowHUD(true, true);
                break;
        }
    }
    #endregion
}

public enum LoadingType
{
    Lobby = 0,
    Fight,
}

