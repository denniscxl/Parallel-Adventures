using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GKBase;
using GKUI;

public class UIInformation : SingletonUIBase<UIInformation>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Text TitleText;
        public Text CreateTimeText;
        public Text KillCountText;
        public Text DeathCountText;
        public Text CoinCostText;
        public Text DiamondCostText;
        public Text FightingCountText;
        public Text SkillUpgradeCountText;
        public Text ConsumeCostText;
        public Text ThrowCountText;
        public Text WinCountText;
        public Text DefeatedCountText;
        public Button BackBtn;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
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
    }

    private void Init()
    {
        var data = AchievementController.Instance().GetDataBase();

        m_ctl.TitleText.text = DataController.Instance().GetLocalization(AchievementController.Instance().Title, LocalizationSubType.Title);
        m_ctl.CreateTimeText.text = GK.GetDateTime(data.GetAttribute((int)EObjectAttr.CreateTime).longValue).ToString("yyyy.MM.dd");
        m_ctl.KillCountText.text = data.GetAttribute((int)EObjectAttr.AchiKillCount).ValInt.ToString();
        m_ctl.DeathCountText.text = data.GetAttribute((int)EObjectAttr.AchiDeathCount).ValInt.ToString();;
        m_ctl.CoinCostText.text = data.GetAttribute((int)EObjectAttr.AchiCoinCost).ValInt.ToString();
        m_ctl.DiamondCostText.text = data.GetAttribute((int)EObjectAttr.AchiDiamondCost).ValInt.ToString();
        m_ctl.FightingCountText.text = data.GetAttribute((int)EObjectAttr.AchiFightingCount).ValInt.ToString();
        m_ctl.SkillUpgradeCountText.text = data.GetAttribute((int)EObjectAttr.AchiSkillUpgrade).ValInt.ToString();
        m_ctl.ConsumeCostText.text = data.GetAttribute((int)EObjectAttr.AchiConsumeCost).ValInt.ToString();
        m_ctl.ThrowCountText.text = data.GetAttribute((int)EObjectAttr.AchiThrowCount).ValInt.ToString();
        m_ctl.WinCountText.text = data.GetAttribute((int)EObjectAttr.AchiWinCount).ValInt.ToString();
        m_ctl.DefeatedCountText.text = data.GetAttribute((int)EObjectAttr.AchiDefeatedCount).ValInt.ToString();
    }

    private void OnBack(GameObject go)
    {
        Close();
    }
    #endregion
}
