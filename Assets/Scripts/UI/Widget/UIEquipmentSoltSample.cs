using UnityEngine.UI;
using UnityEngine;
using GKBase;
using GKUI;

public class UIEquipmentSoltSample : UIBase
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Image Icon;
        public Text PartText;
        public GameObject DetailRoot;
        public Text NameText;
        public Text StrValueText;
        public Text AgiValueText;
        public Text IntValueText;
        public Text JobText;
    }
    #endregion

    #region PublicField
    [SerializeField]
    private GameObject[] _demands;
    [SerializeField]
    private Image[] _demandIcons;
    [SerializeField]
    private Text[] _demandLvs;
    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private EquipmentPart _part = EquipmentPart.Weapon;
    private int _id;
    private int _unitID;
    private GameData.EquipmentData _data;
    #endregion

    #region PublicMethod
    public void SetData(EquipmentPart part)
    {
        _part = part;
    }

    public void Refresh(int id, int unitID)
    {
        _id = id;
        _unitID = unitID;
    
        // 第一次刷新时对象由于还未生成. 故跳过. 并在Init()中刷新.
        if (null == m_ctl || null == m_ctl.Icon)
            return;

        m_ctl.Icon.gameObject.SetActive(-1 != _id);
        m_ctl.DetailRoot.SetActive(-1 != _id);
        if (-1 != _id)
        {
            _data = DataController.Data.GetEquipmentData(_id);

            if (null != _data)
            {
                m_ctl.Icon.sprite = ConfigController.Instance().GetEquipmentSprite(_id);
                m_ctl.NameText.text = DataController.Instance().GetLocalization(_data.name, LocalizationSubType.Item);
                m_ctl.StrValueText.text = _data.strength.ToString();
                m_ctl.AgiValueText.text = _data.agility.ToString();
                m_ctl.IntValueText.text = _data.intelligence.ToString();
                m_ctl.JobText.text = Card.GetJobDescription(_data.job);
            }
            for (int i = 0; i < 3; i++)
            {
                InitDemandSkill(i);
            }
        }
    }

    public void OnClick(GameObject go)
    {
        UIInventory.Open().SetMode(InventoryOperationMode.Equip, UIEquipment.instance.GetCurrentCardID());
    }

    public void OnUnloadBtn(GameObject go)
    {
        // 检测背包容量是否已满.
        if(PlayerController.Instance().GetInventory().Count >= PlayerController.Instance().GetInventoryCapacity())
        {
            UIMessageBox.ShowUISelectMessage(DataController.Instance().GetLocalization(85), 
                                                      DataController.Instance().GetLocalization(86), 
                                                      DataController.Instance().GetLocalization(87), () =>
            {
                UIInventory.Open().SetMode(InventoryOperationMode.Normal);
            });
            return;
        }

        PlayerController.Instance().ModifyCardEquipmentState(false, _unitID, _data, -1);
        _id = -1;
        // 刷新.
        Refresh(-1, _unitID);
        UIEquipment.instance.Changed();
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
        PlayerController.Instance().OnLanguageChangedEvent += OnLanguageChanged;
    }

    private void Init()
    {
        m_ctl.PartText.text = DataController.Instance().GetLocalization(75 + (int)_part);
        Refresh(_id, _unitID);
    }

    private void InitDemandSkill(int idx)
    {
        int demandID = 0;
        switch (idx)
        {
            case 0:
                demandID = _data.skillEffectA;
                break;
            case 1:
                demandID = _data.skillEffectB;
                break;
            case 2:
                demandID = _data.skillEffectC;
                break;
        }

        if (-1 != demandID)
        {
            int key = DataController.Data.GetSkillData(demandID).key;
            int resID = key / 100;
            _demandIcons[idx].sprite = ConfigController.Instance().GetSkillSprite(resID);
            _demandLvs[idx].text = string.Format("Lv {0}", key % 100);
        }
        else
        {
            _demands[idx].SetActive(false);
        }
    }

    private void OnDestroy()
    {
        PlayerController.Instance().OnLanguageChangedEvent -= OnLanguageChanged;
    }

    // 刷新语言.
    private void OnLanguageChanged()
    {
        Init();
    }

    #endregion
}
