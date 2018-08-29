using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using GKBase;
using GKUI;

public class UILotteryCard : UIBase
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public RawImage Icon_Raw;
        public Image Icon_Image;
        public Image Cover;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private int _id;
    private LotteryType _type;
    #endregion

    #region PublicMethod
    public void SetIcon(LotteryType type, int id)
    {
        _id = id;
        _type = type;
    }

    public void Show(bool bShow)
    {
        m_ctl.Cover.fillCenter = !bShow;
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
        GKUIEventTriggerListener.Get(gameObject).onClick = OnClick;
    }

    private void Init()
    {
        switch(_type)
        {
            case LotteryType.Coin:
            case LotteryType.Diamond:
                m_ctl.Icon_Raw.gameObject.SetActive(true);
                m_ctl.Icon_Image.gameObject.SetActive(false);
                m_ctl.Icon_Raw.texture = ConfigController.Instance().GetCardIconTexture(_id);
                break;
            case LotteryType.Equipment:
                m_ctl.Icon_Raw.gameObject.SetActive(false);
                m_ctl.Icon_Image.gameObject.SetActive(true);
                m_ctl.Icon_Image.sprite = ConfigController.Instance().GetEquipmentSprite(_id);
                break;
            case LotteryType.Consume:
                m_ctl.Icon_Raw.gameObject.SetActive(false);
                m_ctl.Icon_Image.gameObject.SetActive(true);
                m_ctl.Icon_Image.sprite = ConfigController.Instance().GetConsumeSprite(_id);
                break;
        }

    }

    private void OnClick(GameObject go)
    {

    }
    #endregion
}
