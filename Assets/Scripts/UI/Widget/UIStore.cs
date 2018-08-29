using UnityEngine.UI;
using UnityEngine;
using GKBase;
using GKUI;

public class UIStore : SingletonUIBase<UIStore>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public GameObject DetailRoot;
        public GameObject DetailContentRoot;
        public UIStoreItemSample UIStoreItemSample;
        public Button ExitBtn;
        public GameObject ClassRoot;
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
    public void ChangeState(bool Selected)
    {
        m_ctl.ClassRoot.SetActive(Selected);
        m_ctl.DetailRoot.SetActive(!Selected);
    }

    public void InitItems(UIStoreClassType type)
    {
        GK.DestroyAllChildren(m_ctl.DetailContentRoot);
        for (int i = 1; i < 7; i++)
        {
            var go = GameObject.Instantiate(m_ctl.UIStoreItemSample.gameObject);
            if (null != go)
            {
                go.SetActive(true);
                GK.SetParent(go, m_ctl.DetailContentRoot, false);
                GK.GetOrAddComponent<UIStoreItemSample>(go).SetData(type, i);
            }
        }
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
        GKUIEventTriggerListener.Get(m_ctl.ExitBtn.gameObject).onClick = OnExit;
        GKUIEventTriggerListener.Get(m_ctl.BackBtn.gameObject).onClick = OnBack;
    }

    private void Init()
    {
        ChangeState(true);
    }

    private void OnExit(GameObject go)
    {
        Close();
    }

    private void OnBack(GameObject go)
    {
        ChangeState(true);
    }
    #endregion
}
