using UnityEngine.UI;
using UnityEngine;
using GKBase;
using GKMap;
using GKData;
using GKUI;

public class UIFormationCardSample : UIBase
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public RawImage Icon;
        public Text PowerText;
        public Text HpText;
        public Text MpText;
        public Text NameText;
        public Text MoodText;
        public Text LevelText;
        public Scrollbar LvScrollBar;
        public Text SkillLvText;
        public Scrollbar SkillLvScrollBar;
        public Text StrText;
        public Text AgiText;
        public Text IntText;
        public Text SpeedText;
        public Text KillCountText;
        public Toggle FightingToggle;

        public GameObject MoveType;
        public Image MoveSample;

        public Button SkillBtn;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private Card _card;
    #endregion

    #region PublicMethod
    public void Release()
    {
        //Debug.Log("UIFormationCardSample Release");
        _card.dataBase.GetAttributeList((int)EObjectAttr.Unit_Skills).OnAttrbutChangedEvent -= OnSkillsChanged;
        _card.dataBase.GetAttributeList((int)EObjectAttr.Unit_Equipments).OnAttrbutChangedEvent -= OnEquipmentsChanged;
    }

    public void SetData(Card card)
    {
        _card = card;
    }

    public void OnSkill(GameObject go)
    {
        UISkill.Open().SetData(_card.dataBase.GetAttribute((int)EObjectAttr.ID).ValInt);
    }

    public void OnEquipment(GameObject go)
    {
        UIEquipment.Open().SetData(_card.dataBase.GetAttribute((int)EObjectAttr.ID).ValInt);
    }

    public void OnFighting(GameObject go)
    {
        if (null == _card)
            return;

        PlayerController.Instance().SetCardFighting(_card.dataBase.GetAttribute((int)EObjectAttr.ID).ValInt, m_ctl.FightingToggle.isOn);
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
        //Debug.Log("UIFormationCardSample InitListener");
        _card.dataBase.GetAttributeList((int)EObjectAttr.Unit_Skills).OnAttrbutChangedEvent += OnSkillsChanged;
        _card.dataBase.GetAttributeList((int)EObjectAttr.Unit_Equipments).OnAttrbutChangedEvent += OnEquipmentsChanged;
    }

    private void Init()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (null == _card)
        {
            Debug.LogWarning(string.Format("Init formation card faile. Card is null."));
            return;
        }
        m_ctl.PowerText.text = _card.dataBase.GetAttribute((int)EObjectAttr.Power).ValInt.ToString();
        m_ctl.Icon.texture = ConfigController.Instance().GetCardIconTexture(_card.dataBase.GetAttribute((int)EObjectAttr.ID).ValInt);
        m_ctl.HpText.text = _card.dataBase.GetAttribute((int)EObjectAttr.MaxHp).ValInt.ToString();
        m_ctl.MpText.text = _card.dataBase.GetAttribute((int)EObjectAttr.MaxMp).ValInt.ToString();
        string tName = DataController.Instance().GetLocalization(_card.dataBase.GetAttribute((int)EObjectAttr.Name).ValInt, LocalizationSubType.Unit);
        m_ctl.NameText.text = tName;
        m_ctl.MoodText.text = _card.dataBase.GetAttribute((int)EObjectAttr.Mood).ValInt.ToString();
        m_ctl.LevelText.text = _card.dataBase.GetAttribute((int)EObjectAttr.Level).ValInt.ToString();
        int curExp = _card.dataBase.GetAttribute((int)EObjectAttr.Exp).ValInt;
        int maxExp = _card.dataBase.GetAttribute((int)EObjectAttr.MaxExp).ValInt;
        m_ctl.LvScrollBar.size = (float)curExp / (float)maxExp;
        m_ctl.SkillLvText.text = _card.dataBase.GetAttribute((int)EObjectAttr.SkillLevel).ValInt.ToString();
        curExp = _card.dataBase.GetAttribute((int)EObjectAttr.SkillExp).ValInt;
        maxExp = _card.dataBase.GetAttribute((int)EObjectAttr.MaxSkillExp).ValInt;
        m_ctl.SkillLvScrollBar.size = (float)curExp / (float)maxExp;
        m_ctl.StrText.text = _card.dataBase.GetAttribute((int)EObjectAttr.Strength).ValInt.ToString();
        m_ctl.AgiText.text = _card.dataBase.GetAttribute((int)EObjectAttr.Agility).ValInt.ToString();
        m_ctl.IntText.text = _card.dataBase.GetAttribute((int)EObjectAttr.Intelligence).ValInt.ToString();
        m_ctl.SpeedText.text = _card.dataBase.GetAttribute((int)EObjectAttr.MoveSpeed).ValInt.ToString();
        curExp = _card.dataBase.GetAttribute((int)EObjectAttr.KillCount).ValInt;
        maxExp = _card.dataBase.GetAttribute((int)EObjectAttr.DeathCount).ValInt;
        m_ctl.KillCountText.text = string.Format("{0} / {1}", curExp, maxExp);
        m_ctl.FightingToggle.isOn = PlayerController.Instance().GetCardFightingState(_card.dataBase.GetAttribute((int)EObjectAttr.ID).ValInt);
        RefreshMoveTypeIcon();

    }

    // 刷新移动类型.
    private void RefreshMoveTypeIcon()
    {
        GK.DestroyAllChildren(m_ctl.MoveType);
        int layermask = _card.dataBase.GetAttribute((int)EObjectAttr.LayerMask).ValInt;
        if ((layermask & (int)MoveType.Road) == (int)MoveType.Road)
        {
            CloneMoveTypeIcon(0);
        }
        if ((layermask & (int)MoveType.Grass) == (int)MoveType.Grass)
        {
            CloneMoveTypeIcon(1);
        }
        if ((layermask & (int)MoveType.River) == (int)MoveType.River)
        {
            CloneMoveTypeIcon(2);
        }
    }

    // 克隆移动类型图标.
    private void CloneMoveTypeIcon(int id)
    {
        var moveIcon = GameObject.Instantiate(m_ctl.MoveSample) as Image;
        moveIcon.gameObject.SetActive(true);
        GK.SetParent(moveIcon.gameObject, m_ctl.MoveType, false);
        moveIcon.sprite = ConfigController.Instance().GetMoveTypeSprite(id);
    }

    private void OnSkillsChanged(object obj, GKCommonListValue attr)
    {
        //Debug.Log("UIFormationCardSample OnSkillsChanged");
        Refresh();
    }

    private void OnEquipmentsChanged(object obj, GKCommonListValue attr)
    {
        //Debug.Log("UIFormationCardSample OnEquipmentsChanged");
        Refresh();
    }
    #endregion
}
