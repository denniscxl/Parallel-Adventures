using UnityEngine;
using UnityEngine.UI;
using GKBase;
using GKUI;

public class UILottery : SingletonUIBase<UILottery>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
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

    }

    private void OnBack(GameObject go)
    {
        Close();
    }
    #endregion
}
