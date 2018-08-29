using UnityEngine.UI;
using UnityEngine;
using GKBase;
using GKUI;

public class UIAchiItemSample : UIBase
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Text NameText;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private int _id;
    private GameData.AchievementData _data;
    private bool _bCompleted = false;
    #endregion

    #region PublicMethod
    public void SetData(int id)
    {
        _id = id;
    }

    public bool GetCompletedFlag()
    {
        return _bCompleted;
    }

    public void OnClick(GameObject go)
    {
        UIAchievement.instance.ShowDetaile(true, _data);
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
        _data = DataController.Data.GetAchievementData(_id);
        if (null == _data)
        {
            Debug.LogError(string.Format("UIAchiItemSample Init faile. Get data is null. id: {0}", _id));
            gameObject.SetActive(false);
            return;
        }
        m_ctl.NameText.text = DataController.Instance().GetLocalization(_data.id, LocalizationSubType.Achievement);
        // 判断是否达成, 达成后可以进行称号切换.
        _bCompleted = AchievementController.Instance().IsCompleted(_id);
        gameObject.GetComponent<Button>().interactable = _bCompleted;
    }


    #endregion
}
