using UnityEngine;
using UnityEngine.UI;
using GKBase;
using GKUI;

public class UITutorial : SingletonUIBase<UITutorial>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Image border;
        public Image finger;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    #endregion

    #region PublicMethod
    public void SetStyle(Vector3 pos, Vector2 size, bool finger = true)
    {
        m_ctl.border.rectTransform.position = pos;
        m_ctl.border.rectTransform.sizeDelta = size;
        m_ctl.finger.gameObject.SetActive(finger);
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
        GKUIEventTriggerListener.Get(m_ctl.border.gameObject).onClick = OnClick;
    }

    private void Init()
    {
       
    }

    private void OnDestroy()
    {
       
    }

    private void OnClick(GameObject go)
    {
        Debug.Log("OnClick");
    }
    #endregion
}

