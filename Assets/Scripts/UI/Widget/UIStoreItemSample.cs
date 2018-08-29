using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using UnityEngine.SceneManagement;
using GKBase;
using GKMap;
using GKUI;

public class UIStoreItemSample : UIBase
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Image Icon;
        public Text Pay;
        public Text Earnings;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private UIStoreClassType _type;
    private int _level = 0;
    private int _pay = 0;
    private int _earnings = 0;
    #endregion

    #region PublicMethod
    // 初始化商品信息.
    // 目前为模拟信息. 之后通过配置表读取.
    public void SetData(UIStoreClassType type, int level)
    {
        _type = type;
        _level = level;
        ConfigController.Instance().GetStoreItemData(_type, level, out _pay, out _earnings);
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
        string spriteName = ConfigController.Instance().GetSpriteName(StoreClassToAttrType(_type, _level));
        GameObject gameObject = null;
        m_ctl.Icon.sprite = ConfigController.Instance().GetUISprite(spriteName);
        m_ctl.Pay.text = _pay.ToString();
        m_ctl.Earnings.text = _earnings.ToString();
    }

    private void OnClick(GameObject go)
    {
        switch(_type)
        {
            case UIStoreClassType.Conin:
                PlayerController.Instance().Coin += _earnings;
                break;
            case UIStoreClassType.Diamond:
                PlayerController.Instance().Diamond += _earnings;
                break;
            case UIStoreClassType.Item:
                UIMessageBox.ShowUIMessage("Coming soon.");
                break;
        }
    }

    // 类型转换.
    private EObjectAttr StoreClassToAttrType(UIStoreClassType type, int level)
    {
        switch(type)
        {
            case UIStoreClassType.Conin:
                return EObjectAttr.Coins;
            case UIStoreClassType.Diamond:
                return EObjectAttr.Diamond;
            case UIStoreClassType.Item:
                break;
        }
        return EObjectAttr.Coins;
    }
    #endregion
}