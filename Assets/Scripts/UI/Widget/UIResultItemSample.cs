using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using GKBase;
using GKUI;

public class UIResultItemSample : UIBase
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Text NameText;
        public Text KillText;
        public Text PointsText;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private int _name;
    private int _killCcount;
    private int _points;
    #endregion

    #region PublicMethod
    public void SetData(int name, int killCount, int points)
    {
        var unitData = DataController.Data.GetUnitData(name);
        if (null == unitData)
            return;
        _name = unitData.name;
        _killCcount = killCount;
        _points = points;
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

    }

    private void Init()
    {
        m_ctl.NameText.text = DataController.Instance().GetLocalization(_name, LocalizationSubType.Unit);
        m_ctl.KillText.text = string.Format("{0}: {1}", DataController.Instance().GetLocalization(26), _killCcount);
        m_ctl.PointsText.text = string.Format("{0}: {1}", DataController.Instance().GetLocalization(96), _points);
    }

    public void OnClick(GameObject go)
    {
 
    }
    #endregion
}
