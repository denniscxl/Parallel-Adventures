using UnityEngine;
using UnityEngine.UI;
using GKBase;
using GKUI;

public class UIFormation : SingletonUIBase<UIFormation>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Text FightingCountText;
        public UIFormationCardSample FormationCardSample;
        public GameObject ContentRoot;
        public Button BackBtn;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private UIFormationCardSample[] samples;
    #endregion

    #region PublicMethod

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
        GKUIEventTriggerListener.Get(m_ctl.BackBtn.gameObject).onClick = OnBack;
        PlayerController.Instance().OnFormationChangedEvent += OnFormationChanged;
        PlayerController.Instance().OnLanguageChangedEvent += OnLanguageChanged;
    }

    private void Init()
    {
        OnFormationChanged();
        GK.DestroyAllChildren(m_ctl.ContentRoot);
        int i = 0;
        samples = new UIFormationCardSample[PlayerController.Instance().GetPlayerCards().Count];
        foreach (var card in PlayerController.Instance().GetPlayerCards().Values)
        {
            var go = GameObject.Instantiate(m_ctl.FormationCardSample.gameObject);
            if (null != go)
            {
                go.SetActive(true);
                GK.SetParent(go, m_ctl.ContentRoot, false);
                samples[i] = GK.GetOrAddComponent<UIFormationCardSample>(go);
                samples[i].SetData(card);
                i++;
            }
        }
    }

    private void OnFormationChanged()
    {
        if (null == m_ctl || null == m_ctl.FightingCountText)
            return;
        
        m_ctl.FightingCountText.text = string.Format("{0} / {1}", PlayerController.Instance().GetFightingCount(), PlayerController.MAX_FIGHT_COUNT);
    }

    private void OnDestroy()
    {
        PlayerController.Instance().OnFormationChangedEvent -= OnFormationChanged;
        PlayerController.Instance().OnLanguageChangedEvent -= OnLanguageChanged;
    }

    // 刷新语言.
    private void OnLanguageChanged()
    {
        Init();
    }

    private void OnBack(GameObject go)
    {
        foreach(var s in samples)
        {
            if (null == s)
                continue;
            s.Release();
        }
        Close();
    }
    #endregion
}
