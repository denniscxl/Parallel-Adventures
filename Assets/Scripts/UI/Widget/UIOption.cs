using UnityEngine.UI;
using UnityEngine;
using GKBase;
using GKUI;

public class UIOption : SingletonUIBase<UIOption>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Slider SoundSlider;
        public Slider MusicSlider;
        public Toggle HighToggle;
        public Toggle MediumToggle;
        public Toggle LowToggle;
        public Button QuitBtn;
        public Button ExitBtn;
        public Button CreditsBtn;
        public Toggle EnglishToggle;
        public Toggle ChineseToggle;
        public Button ClearDataBtn;
        public Button BackBtn;
    }
    #endregion

    #region PublicField
    public void OnRendingQuailtyChanged(int lv)
    {
        RendingController.Instance().Quality = lv;
    }

    public void OnLanguageChanged(int lv)
    {
        PlayerController.Instance().Language = lv;
    }

    public void OnSoundChanged(float val)
    {
        AudioController.Instance().Sound = val;
    }

    public void OnMusicChanged(float val)
    {
        AudioController.Instance().Music = val;
    }
    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    #endregion

    #region PublicMethod
    public void OnEnable()
    {
        UpdateWidgetState();
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
        GKUIEventTriggerListener.Get(m_ctl.CreditsBtn.gameObject).onClick = OnCreditsData;
        GKUIEventTriggerListener.Get(m_ctl.ClearDataBtn.gameObject).onClick = OnClearData;
        GKUIEventTriggerListener.Get(m_ctl.BackBtn.gameObject).onClick = OnBack;
        GKUIEventTriggerListener.Get(m_ctl.QuitBtn.gameObject).onClick = OnQuit;
        GKUIEventTriggerListener.Get(m_ctl.ExitBtn.gameObject).onClick = OnExit;
    }

    private void Init()
    {
        keepInMemory = true;

        // 初始化音乐音效数据.
        m_ctl.SoundSlider.value = AudioController.Instance().Sound;
        m_ctl.MusicSlider.value = AudioController.Instance().Music;

        // 初始化渲染质量数据.
        switch(RendingController.Instance().Quality)
        {
            case 0:
                m_ctl.HighToggle.isOn = true;
                break;
            case 1:
                m_ctl.MediumToggle.isOn = true;
                break;
            case 2:
                m_ctl.LowToggle.isOn = true;
                break;
        }

        // 初始化语言数据.
        switch (PlayerController.Instance().Language)
        {
            case 0:
                m_ctl.EnglishToggle.isOn = true;
                break;
            case 1:
                m_ctl.ChineseToggle.isOn = true;
                break;
        }
        UpdateWidgetState();
    }

    // 初始化设置界面内容, 随游戏状态不同而不同. 
    // 首次打开时由于空间尚未初始化完毕, 需要在Init中执行. 之后再OnEnable中检测.
    private void UpdateWidgetState()
    {
        if (null == m_ctl || null == m_ctl.ExitBtn)
            return;
        // 离开游戏按钮仅在游戏中可见.
        m_ctl.ExitBtn.gameObject.SetActive(MyGame.IsBattle);
    }

    private void OnClearData(GameObject go)
    {
        DataController.Instance().ClearData();
    }

    private void OnCreditsData(GameObject go)
    {
        UICredits.Open();
    }

    private void OnExit(GameObject go)
    {
        UICreateUnit.Close();
        LevelController.Instance().EndGame();
        UIResult.Open().SetData(false);
        Close();
    }

    private void OnQuit(GameObject go)
    {
        Application.Quit();
        DataController.Instance().SaveData();
        Close();
    }

    private void OnBack(GameObject go)
    {
        Close();
    }
    #endregion
}
