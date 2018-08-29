using UnityEngine.UI;
using UnityEngine;
using GKBase;
using GKData;
using GKUI;

public class UICreateUnit : SingletonUIBase<UICreateUnit>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public UICreateUnitSample UnitSample;
        public UICreateUnitSkillSample SkillSample;
        public GameObject UnitRoot;
        public Button BackBtn;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private int _tileID;
    private bool _bShowBackBtn = false;
    #endregion

    #region PublicMethod
    // bShowBack 开始游戏时, 选择第一名英雄不可返回.
    public void SetData(int tileID, bool bShowBack = false)
    {
        _tileID = tileID;
        _bShowBackBtn = bShowBack;
    }

    public void Refresh()
    {
        GK.DestroyAllChildren(m_ctl.UnitRoot);
        foreach (var card in LevelController.Instance().GetRemainderCards(PlayerController.Instance().Camp))
        {
            var go = GameObject.Instantiate(m_ctl.UnitSample.gameObject);
            var sample = GK.GetOrAddComponent<UICreateUnitSample>(go);
            GKDataBase c = PlayerController.Instance().GetCardDetaileFromFight(card);
            if (null != c)
            {
                go.SetActive(true);
                sample.SyncData(_tileID, c, m_ctl.SkillSample.gameObject);
                GK.SetParent(go, m_ctl.UnitRoot, false);
            }
        }
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
        GKUIEventTriggerListener.Get(m_ctl.BackBtn.gameObject).onClick = OnBack;
    }

    private void Init()
    {
        Refresh();
        m_ctl.BackBtn.gameObject.SetActive(_bShowBackBtn);
    }

    private void OnBack(GameObject go)
    {
        Close();
    }
    #endregion
}
