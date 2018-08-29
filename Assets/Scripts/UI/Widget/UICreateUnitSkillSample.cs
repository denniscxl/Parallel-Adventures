using UnityEngine.UI;
using GKBase;
using GKUI;

public class UICreateUnitSkillSample : UIBase
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Image Icon;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private int _id;
    #endregion

    #region PublicMethod
    public void SyncData(int icon)
    {
        _id = icon;

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
        Refresh();
    }

    private void Refresh()
    {
        int key = DataController.Data.GetSkillData(_id).key;
        int resID = key / 100;
        m_ctl.Icon.sprite = ConfigController.Instance().GetSkillSprite(resID);
    }
    #endregion
}
