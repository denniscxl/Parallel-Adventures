using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using GKBase;
using GKData;
using GKUI;

public class UISkillSample : UIBase
{
    void HandleOnConfirmBtnClick()
    {
    }


    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Image Icon;
        public Image Cover;
        public Text NameText;
        public Text DescriptionText;
        public Text StrValueText;
        public Text AgiValueText;
        public Text IntValueText;
        public Text LvText;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    [SerializeField]
    private GameObject[] _demands;
    [SerializeField]
    private Image[] _demandIcons;
    [SerializeField]
    private Text[] _demandLvs;
    private int _skillID = 0;
    private int _cardID = 0;
    private int _index = 0;
    private GameData.SkillData _data;
    #endregion

    #region PublicMethod
    public void SetData(int skillID, int cardID, int idx)
    {
        _skillID = skillID;
        _cardID = cardID;
        _index = idx;
        _data = DataController.Data.GetSkillData(skillID);
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
        //GKUIEventTriggerListener.Get(gameObject).onClick = OnClick;
    }

    private void Init()
    {
        if(null == _data)
        {
            Debug.LogError(string.Format("Init skill sample faile. _data is null."));
            return;
        }

        int resID = _data.key / 100;
        int resLv = _data.key % 100;
        m_ctl.Icon.sprite = ConfigController.Instance().GetSkillSprite(resID);
        m_ctl.Cover.gameObject.SetActive(0 == resLv);
        m_ctl.NameText.text = DataController.Instance().GetLocalization(_data.name, LocalizationSubType.Skill);
        m_ctl.StrValueText.text = _data.strength.ToString();
        m_ctl.AgiValueText.text = _data.agility.ToString();
        m_ctl.IntValueText.text = _data.intelligence.ToString();
        m_ctl.LvText.text = string.Format("Lv {0}", resLv);
        m_ctl.DescriptionText.text = DataController.Instance().GetLocalization(_data.description, LocalizationSubType.Skill);

        for (int i = 0; i < 3; i++)
        {
            InitDemandSkill(i);
        }
    }

    private void InitDemandSkill(int idx)
    {
        int demandID = 0;
        switch(idx)
        {
            case 0:
                demandID = _data.demandA;
                break;
            case 1:
                demandID = _data.demandB;
                break;
            case 2:
                demandID = _data.demandC;
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

    public void OnLvUp(GameObject go)
    {
        UIMessageBox.ShowUISelectMessage(DataController.Instance().GetLocalization(125),
                                                  DataController.Instance().GetLocalization(86),
                                                  DataController.Instance().GetLocalization(87),
                                                 () =>
        {
            int ret = SkillController.Instance().CanLvUp(_data, _cardID);
            if (0 == ret)
            {
                SkillController.Instance().SkillUpLv(_cardID, _index);
            }
            else
            {
                UIMessageBox.ShowUIMessage(DataController.Instance().GetLocalization(ret, LocalizationSubType.ErrorCode));
            }
        });
    }
    #endregion
}
