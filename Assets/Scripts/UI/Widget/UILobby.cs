using UnityEngine.UI;
using UnityEngine;
using GKBase;
using GKUI;

public class UILobby : SingletonUIBase<UILobby>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Button FormationBtn;
        public Button SkillBtn;
        public Button EquipmentBtn;
        public Button InventoryBtn;
        public Button LotteryBtn;
        public Button AchievementBtn;
        public Button StoreBtn;
        public Button InformationBtn;
        public Button AthleticsBtn;
        public Button StoryBtn;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private UITitle _uiTitle;
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
        GKUIEventTriggerListener.Get(m_ctl.FormationBtn.gameObject).onClick = OnFormation;
        GKUIEventTriggerListener.Get(m_ctl.SkillBtn.gameObject).onClick = OnSkill;
        GKUIEventTriggerListener.Get(m_ctl.EquipmentBtn.gameObject).onClick = OnEquipment;
        GKUIEventTriggerListener.Get(m_ctl.InventoryBtn.gameObject).onClick = OnInventory;
        GKUIEventTriggerListener.Get(m_ctl.LotteryBtn.gameObject).onClick = OnLottery;
        GKUIEventTriggerListener.Get(m_ctl.AchievementBtn.gameObject).onClick = OnAchievement;
        GKUIEventTriggerListener.Get(m_ctl.StoreBtn.gameObject).onClick = OnStore;
        GKUIEventTriggerListener.Get(m_ctl.InformationBtn.gameObject).onClick = OnInformation;
        GKUIEventTriggerListener.Get(m_ctl.AthleticsBtn.gameObject).onClick = OnAthletics;
        GKUIEventTriggerListener.Get(m_ctl.StoryBtn.gameObject).onClick = OnStory;

    }

    private void Init()
    {
        _uiTitle = UITitle.Open();
        _uiTitle.SetState(false);
    }

    private void OnFormation(GameObject go)
    {
        UIFormation.Open();
    }

    private void OnSkill(GameObject go)
    {
        UISkill.Open();
    }

    private void OnEquipment(GameObject go)
    {
        UIEquipment.Open();
    }

    private void OnInventory(GameObject go)
    {
        UIInventory.Open().SetMode(InventoryOperationMode.Normal);
    }

    private void OnLottery(GameObject go)
    {
        UILottery.Open();
    }

    private void OnAchievement(GameObject go)
    {
        UIAchievement.Open();
    }

    private void OnStore(GameObject go)
    {
        UIStore.Open();
    }

    private void OnInformation(GameObject go)
    {
        UIInformation.Open();
    }

    private void OnStory(GameObject go)
    {
        StartGame(LoadingType.Fight);
    }

    private void OnAthletics(GameObject go)
    {
        StartGame(LoadingType.Fight);
    }

    private void StartGame(LoadingType type)
    {
        if (0 == PlayerController.Instance().GetFightingCount())
        {
            UIMessageBox.ShowUISelectMessage(DataController.Instance().GetLocalization(126),
                                                      DataController.Instance().GetLocalization(86),
                                                      DataController.Instance().GetLocalization(87),
                                                      () =>
                                                      {
                                                          UIFormation.Open();
                                                      });
            return;
        }

        // 临时设置玩家阵营.
        PlayerController.Instance().Camp = CampType.Blue;
        UILoading.Open().Next(type);
        Close();
    }
    #endregion
}
