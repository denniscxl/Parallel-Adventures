using UnityEngine;
using GKBase;
using GKUI;

public class UIStoreClass : UIBase
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {

    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    [SerializeField]
    private UIStoreClassType _type = UIStoreClassType.Conin;
    private UIStore _uiStore;
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
        GKUIEventTriggerListener.Get(gameObject).onClick = OnClick;
    }

    private void Init()
    {
        _uiStore = UIStore.instance;
    }

    private void OnClick(GameObject go)
    {
        switch(_type)
        {
            case UIStoreClassType.Conin:
                _uiStore.InitItems(UIStoreClassType.Conin);
                _uiStore.ChangeState(false);
                break;
            case UIStoreClassType.Diamond:
                _uiStore.InitItems(UIStoreClassType.Diamond);
                _uiStore.ChangeState(false);
                break;
            case UIStoreClassType.Item:
                _uiStore.InitItems(UIStoreClassType.Item);
                _uiStore.ChangeState(false);
                break;
        }
    }
    #endregion
}

public enum UIStoreClassType
{
    Conin = 0,
    Diamond,
    Item

}
