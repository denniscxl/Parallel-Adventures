using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GKBase;
using GKUI;

public class UIInventory : SingletonUIBase<UIInventory>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Text CapacityText;
        public Button UpgradeBtn;
        public Text LvText;
        public Button BackBtn;
        public DynamicInfinityListRenderer DynamicInfinityList;
        public GameObject OperationPanel;
        public Text OperationNameText;
        public Text OperationJobText;
        public Text StrText;
        public Text StrValueText;
        public Text AgiText;
        public Text AgiValueText;
        public Text IntText;
        public Text IntValueText;
        public Text OperationDescriptionText;
        public Button UseBtn;
        public Button ThrowBtn;

        public ScrollRect ScrollViewRoot;
    }

    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private InventoryOperationMode _mode = InventoryOperationMode.Normal;
    // 上一个操作物品对象.
    private UIInventoryItemSample _lastOperationItem = null;
    [SerializeField]
    private GameObject[] _operationDemands;
    [SerializeField]
    private Image[] _operationDemandIcons;
    [SerializeField]
    private Text[] _operationDemandLvs;
    // 当前选择物品槽ID.
    private int _selectedSoltID = 0;
    // 当前选择角色ID.
    private int _unitID = -1;
    // 存储刷新标志位.
    private bool _bChanged = false;
    #endregion

    #region PublicMethod
    public void SetMode(InventoryOperationMode mode, int unitID = -1)
    {
        _mode = mode;
        _unitID = unitID;
    }

    // 设置当前选择物品槽ID.
    public void SetSelectSolt(int id)
    {
        _selectedSoltID = id;
    }

    public InventoryOperationMode GetOperationMode()
    {
        return _mode;
    }

    public override void OnClose()
    {
        base.OnClose();
        PlayerController.Instance().OnUpGradeInventoryEvent -= UpdateCacpcity;
    }

    // 显示操作面板.
    public void ShowOperationPanel(bool show, UIInventoryItemSample item = null)
    {
        m_ctl.OperationPanel.SetActive(show);
        // 如果上一个操作对象不为空.
        if (null != _lastOperationItem)
        {
            _lastOperationItem.operation = false;
        }

        if (show && null != item)
        {
            _lastOperationItem = item;
            item.operation = true;
            RefreshTips();
        }
    }

    public void OnUse()
    {
        ShowOperationPanel(false);

        Item item = PlayerController.Instance().GetInventorySolt(_selectedSoltID);

        if (ItemType.Equipment == item.type)
        {
            var data = DataController.Data.GetEquipmentData(item.id);
            if (null == data)
                return;

            int ret = PlayerController.Instance().ModifyCardEquipmentState(true, _unitID, data, _selectedSoltID);
            if (0 != ret)
            {
                string content = DataController.Instance().GetLocalization(ret, LocalizationSubType.ErrorCode);
                UIMessageBox.ShowUIMessage(content);
                return;
            }
        }
        else if (ItemType.Consume == item.type)
        {
            // 更新消耗品使用累积总数.
            AchievementController.Instance().UpdateAchievementCount(EObjectAttr.AchiConsumeCost, item.count);
            PlayerController.Instance().ReduceItem(_selectedSoltID, false, 1);
            PlayerController.Instance().SortInventory();
        }

        // 刷新背包界面.
        InitCacpcity();
        _bChanged = true;
    }

    public void OnThrow()
    {
        ShowOperationPanel(false);
        // 更新丢弃物品累积总数.
        var soltData = PlayerController.Instance().GetInventorySolt(_selectedSoltID);
        if(null != soltData)
        {
            AchievementController.Instance().UpdateAchievementCount(EObjectAttr.AchiThrowCount, soltData.count);
        }
        PlayerController.Instance().ReduceItem(_selectedSoltID, true);
        PlayerController.Instance().SortInventory();
        // 刷新背包界面.
        InitCacpcity();
        _bChanged = true;
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
        GKUIEventTriggerListener.Get(m_ctl.UpgradeBtn.gameObject).onClick = OnUpgrade;
        PlayerController.Instance().OnUpGradeInventoryEvent += UpdateCacpcity;
        PlayerController.Instance().OnLanguageChangedEvent += OnLanguageChanged;
    }

    private void Init()
    {
        m_ctl.DynamicInfinityList.InitRendererList(null, OnUpdateDataHandler);
        InitCacpcity();
        UpdateCacpcity();
    }

    // 初始化背包格子.
    private void InitCacpcity()
    {
        int count = PlayerController.Instance().GetInventoryCapacity();
        var data = PlayerController.Instance().GetInventory();
        List<Item> lst = new List<Item>();
        for (int i = 0; i < count; ++i)
        {
            Item item = null;
            if (data.ContainsKey(i))
            {
                item = data[i];
                item.solt = i;
            }
            else
            {
                item = new Item(i, 0, 0, 0);
            }
            lst.Add(item);
        }
        m_ctl.DynamicInfinityList.SetDataProvider(lst);
    }

    // 更新背包当前容量状态.
    private void UpdateCacpcity()
    {
        m_ctl.CapacityText.text = string.Format("{0} / {1}", PlayerController.Instance().GetInventory().Count, PlayerController.Instance().GetInventoryCapacity());
        m_ctl.LvText.text = string.Format("Lv {0}", PlayerController.Instance().InventoryLevel.ToString());
    }

    private void OnUpdateDataHandler(DynamicInfinityItem item)
    {
        item.GetComponent<UIInventoryItemSample>().Refresh();
    }

    // 升级背包.
    private void OnUpgrade(GameObject go)
    {
        // 获取升级价格.
        int val = DataController.Data.GetInventoryUpgradeData(PlayerController.Instance().InventoryLevel).diamond;
        //  确认界面.
        UIMessageBox.ShowUISelectMessage(string.Format(DataController.Instance().GetLocalization(88), val), 
                                                  DataController.Instance().GetLocalization(86), 
                                                  DataController.Instance().GetLocalization(87), () =>
        {
            int ret = PlayerController.Instance().UpgradeInventory();
            switch (ret)
            {
                case 0:
                    InitCacpcity();
                    UIMessageBox.ShowUIMessage(DataController.Instance().GetLocalization(41));
                    break;
                default:
                    string content = DataController.Instance().GetLocalization(ret, LocalizationSubType.ErrorCode);
                    UIMessageBox.ShowUIMessage(content);
                    break;
            }
        });
    }

    private void ShowEquipmentInfo(Item item)
    {
        var data = DataController.Data.GetEquipmentData(item.id);
        if (null == data)
            return;

        m_ctl.UseBtn.gameObject.SetActive(_mode != InventoryOperationMode.Normal);

        m_ctl.OperationNameText.text = DataController.Instance().GetLocalization(data.name, LocalizationSubType.Item);
        m_ctl.OperationJobText.text = Card.GetJobDescription(data.job);
        m_ctl.StrText.gameObject.SetActive(true);
        m_ctl.StrValueText.text = data.strength.ToString();
        m_ctl.AgiText.gameObject.SetActive(true);
        m_ctl.AgiValueText.text = data.agility.ToString();
        m_ctl.IntText.gameObject.SetActive(true);
        m_ctl.IntValueText.text = data.intelligence.ToString();

        for (int i = 0; i < 3; i++)
        {
            InitDemandSkill(i, data);
        }

        m_ctl.OperationDescriptionText.text = DataController.Instance().GetLocalization(data.description, LocalizationSubType.Item);
    }

    private void ShowConsume(Item item)
    {
        var data = DataController.Data.GetConsumeData(item.id);
        if (null == data)
            return;

        m_ctl.OperationJobText.gameObject.SetActive(false);
        m_ctl.UseBtn.gameObject.SetActive(true);
        m_ctl.StrText.gameObject.SetActive(false);
        m_ctl.AgiText.gameObject.SetActive(false);
        m_ctl.IntText.gameObject.SetActive(false);
        for (int i = 0; i < 3; i++)
        {
            _operationDemands[i].SetActive(false);
        }

        m_ctl.OperationNameText.text = DataController.Instance().GetLocalization(data.name, LocalizationSubType.Item);
        m_ctl.OperationDescriptionText.text = DataController.Instance().GetLocalization(data.description, LocalizationSubType.Item);
    }

    private void InitDemandSkill(int idx, GameData.EquipmentData data)
    {
        int demandID = 0;
        switch (idx)
        {
            case 0:
                demandID = data.skillEffectA;
                break;
            case 1:
                demandID = data.skillEffectB;
                break;
            case 2:
                demandID = data.skillEffectC;
                break;
        }

        if (-1 != demandID)
        {
            int key = DataController.Data.GetSkillData(demandID).key;
            int resID = key / 100;
            _operationDemands[idx].SetActive(true);
            _operationDemandIcons[idx].sprite = ConfigController.Instance().GetSkillSprite(resID);
            _operationDemandLvs[idx].text = string.Format("{0}", key % 100);
        }
        else
        {
            _operationDemands[idx].SetActive(false);
        }
    }

    private void OnDestroy()
    {
        PlayerController.Instance().OnUpGradeInventoryEvent -= UpdateCacpcity;
        PlayerController.Instance().OnLanguageChangedEvent -= OnLanguageChanged;
    }

    // 刷新语言.
    private void OnLanguageChanged()
    {
        UpdateCacpcity();
        RefreshTips();
    }

    // 更新Tips内容.
    private void RefreshTips()
    {
        if (m_ctl.OperationPanel.activeSelf && null != _lastOperationItem)
        {
            var data = _lastOperationItem.GetItemData();

            if (ItemType.Equipment == data.type)
            {
                ShowEquipmentInfo(data);
            }
            else if (ItemType.Consume == data.type)
            {
                ShowConsume(data);
            }
        }
    }

    private void OnBack(GameObject go)
    {
        if(_bChanged)
        {
            DataController.Instance().SaveInventory();
            // 如果模式为装备模式, 存储卡牌数据及刷新卡牌界面.
            if (InventoryOperationMode.Equip == _mode)
            {
                DataController.Instance().SaveCards();
                UIEquipment.instance.Refresh();
            }
            _bChanged = false;
        }
        Close();
    }
    #endregion
}

public enum InventoryOperationMode
{
    Normal = 0,
    Equip,
}
