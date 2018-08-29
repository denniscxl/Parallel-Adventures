using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GKBase;
using GKUI;

public class UIAchievement : SingletonUIBase<UIAchievement>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Text PointsText;
        public Text CountText;
        public Text CurTitleText;
        public Text CurTitleDescText;
        public GameObject Content;
        public UIAchiItemSample UIAchiItemSample;
        public Toggle FilterToggle;
        public Text FilterText;
        public Button BackBtn;

        public GameObject DetialPanel;
        public Text DetailNameText;
        public Text DetailTitleText;
        public Text DetailTitleDescText;
        public Text DetailPointsText;
        public Text DetailDescText;
        public Button ChangedBtn;
        public Button DetailBackBtn;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private UIAchiItemSample[] _samples;
    // 当前选中成就数据.
    private GameData.AchievementData _curData = null;
    #endregion

    #region PublicMethod
    public void SetTitle(int id)
    {
        m_ctl.CurTitleText.text = DataController.Instance().GetLocalization(id, LocalizationSubType.Title);
        m_ctl.CurTitleDescText.text = DataController.Instance().GetLocalization(id, LocalizationSubType.TitleDesc);
    }

    public void OnFilter()
    {
        bool bAll = m_ctl.FilterToggle.isOn;
        bool bShow = false;
        foreach (var item in _samples)
        {
            if (null == item)
                continue;

            bShow = bAll ? true : item.GetCompletedFlag();
            item.gameObject.SetActive(bShow);
        }
        m_ctl.FilterText.text = DataController.Instance().GetLocalization(bAll? 100 : 99);
    }
    #endregion

    #region PrivateMethod
    private void Start()
    {
        Serializable();
        InitListener();
        Init();

        UITitle.Close();
    }

    private void Serializable()
    {
        
        GK.FindControls(this.gameObject, ref m_ctl);
    }

    private void InitListener()
    {
        GKUIEventTriggerListener.Get(m_ctl.BackBtn.gameObject).onClick = OnBack;
        GKUIEventTriggerListener.Get(m_ctl.ChangedBtn.gameObject).onClick = OnChanged;
        GKUIEventTriggerListener.Get(m_ctl.DetailBackBtn.gameObject).onClick = OnDetailBack;
        AchievementController.Instance().OnTitleChangedEvent += OnTitleChanged;
        PlayerController.Instance().OnLanguageChangedEvent += OnLanguageChanged;
    }

    private void Init()
    {
        GK.DestroyAllChildren(m_ctl.Content);
        _samples = new UIAchiItemSample[AchievementController.MAX_ACHIEVEMENT_COUNT];
        for (int i = 0; i < AchievementController.MAX_ACHIEVEMENT_COUNT; i++)
        {
            var go = GameObject.Instantiate(m_ctl.UIAchiItemSample.gameObject);
            if (null != go)
            {
                go.SetActive(true);
                GK.SetParent(go, m_ctl.Content, false);
                _samples[i] = GK.GetOrAddComponent<UIAchiItemSample>(go);
                _samples[i].SetData(i);
            }
        }

        int points = 0;
        var lst = AchievementController.Instance().GetAchievements();
        // 设置当前成就完成数.
        m_ctl.CountText.text = string.Format("{0} / {1}", lst.Count, AchievementController.MAX_ACHIEVEMENT_COUNT);
        foreach(int id in lst)
        {
            var data = DataController.Data.GetAchievementData(id);
            if (null == data)
                continue;
            points += data.points;
        }
        m_ctl.PointsText.text = string.Format("{0}: {1}", DataController.Instance().GetLocalization(98), points.ToString());
        SetTitle(AchievementController.Instance().Title);
        m_ctl.FilterText.text = DataController.Instance().GetLocalization(m_ctl.FilterToggle.isOn ? 100 : 99);
    }

    private void OnDestroy()
    {
        AchievementController.Instance().OnTitleChangedEvent -= OnTitleChanged;
        PlayerController.Instance().OnLanguageChangedEvent -= OnLanguageChanged;
    }

    private void OnTitleChanged()
    {
        SetTitle(AchievementController.Instance().Title);
    }

    // 显示成就信息.
    public void ShowDetaile(bool show, GameData.AchievementData data = null)
    {
        _curData = data;
        m_ctl.DetialPanel.gameObject.SetActive(show);

        if (show)
        {
            if (null == data)
            {
                Debug.LogError(string.Format("ShowDetaile faile. Get data is null. data is null."));
                m_ctl.DetialPanel.gameObject.SetActive(false);
                return;
            }
            m_ctl.DetailNameText.text = DataController.Instance().GetLocalization(data.id, LocalizationSubType.Achievement);
            m_ctl.DetailPointsText.text = string.Format("{0}: {1}", DataController.Instance().GetLocalization(98), data.points.ToString());

            // 判断成就室是有有称号奖励.
            m_ctl.DetailTitleText.gameObject.SetActive(-1 != data.id);
            m_ctl.DetailTitleDescText.gameObject.SetActive(-1 != data.id);
            if (-1 != data.id)
            {
                m_ctl.DetailTitleText.text = DataController.Instance().GetLocalization(data.title, LocalizationSubType.Title);
                m_ctl.DetailTitleDescText.text = DataController.Instance().GetLocalization(data.title, LocalizationSubType.TitleDesc);
            }
            m_ctl.DetailDescText.text = AchievementController.Instance().GetDescription((AchievementType)data.action, data.parameter);

            m_ctl.ChangedBtn.gameObject.SetActive(-1 != data.title);
        }
    }

    public void OnChanged(GameObject go)
    {
        if (-1 == _curData.title)
            return;
        AchievementController.Instance().Title = _curData.title;
        ShowDetaile(false);
    }

    public void OnDetailBack(GameObject go)
    {
        ShowDetaile(false);
    }

    private void OnBack(GameObject go)
    {
        UITitle.Open();
        Close();
    }

    // 刷新语言.
    private void OnLanguageChanged()
    {
        Init();
    }
    #endregion
}
