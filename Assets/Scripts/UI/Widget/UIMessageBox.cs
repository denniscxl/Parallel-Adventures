using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GKBase;
using GKUI;

public class UIMessageBox : SingletonUIBase<UIMessageBox>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public GameObject Root;
        public Button   ConfirmBtn;
        public Text     BtnText;
        public Text     Title;
        public Text     Content;
		public Button   OKBtn;
		public Text     OKText;
		public Button   CancelBtn;
		public Text     CancelText;

        public Button ResourceMegRoot;
        public Image ResIcon;
        public RawImage ResRawIcon;
        public Text ResTitle;
        public Text ResContent;
    }
    #endregion

    #region PublicField
    public delegate void OnConfirmBtnClick();
    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private OnConfirmBtnClick OnConfirmFun;
	private OnConfirmBtnClick OnOKFun;
	private OnConfirmBtnClick OnCancelFun;
    #endregion

    #region PublicMethod
    public void Show(string content, string btnText = "Confirm", OnConfirmBtnClick fun = null, string title = "Tips")
    {
        m_ctl.Root.SetActive(true);
        m_ctl.ResourceMegRoot.gameObject.SetActive(false);

		m_ctl.ConfirmBtn.gameObject.SetActive (true);
		m_ctl.OKBtn.gameObject.SetActive (false);
		m_ctl.CancelBtn.gameObject.SetActive (false);

        m_ctl.Title.text = title;
        m_ctl.Content.text = content;
        m_ctl.BtnText.text = btnText;
        OnConfirmFun = fun;
    }

	public void ShowSelectMsgBox(string content, string okText = "OK", string cancelText = "Cancel", 
		OnConfirmBtnClick ok = null, OnConfirmBtnClick cancel = null, string title = "Tips")
	{
        m_ctl.Root.SetActive(true);
        m_ctl.ResourceMegRoot.gameObject.SetActive(false);

		m_ctl.ConfirmBtn.gameObject.SetActive (false);
		m_ctl.OKBtn.gameObject.SetActive (true);
		m_ctl.CancelBtn.gameObject.SetActive (true);

		m_ctl.Title.text = title;
		m_ctl.Content.text = content;
		m_ctl.OKText.text = okText;
		m_ctl.CancelText.text = cancelText;
		OnOKFun = ok;
		OnCancelFun = cancel;
	}

    // type 0 card, 1 equipment, 2 consume. 
    public void ShowResource(string content, int type, int id, string title = "Tips")
    {
        m_ctl.Root.SetActive(false);
        m_ctl.ResourceMegRoot.gameObject.SetActive(true);

        m_ctl.ResTitle.text = title;
        m_ctl.ResContent.text = content;

        m_ctl.ResIcon.gameObject.SetActive(false);
        m_ctl.ResRawIcon.gameObject.SetActive(false);

        OnConfirmFun = null;

        switch(type)
        {
            case 0:
                m_ctl.ResRawIcon.gameObject.SetActive(true);
                m_ctl.ResRawIcon.texture = ConfigController.Instance().GetCardIconTexture(id);
                break;
            case 1:
                m_ctl.ResIcon.gameObject.SetActive(true);
                m_ctl.ResIcon.sprite = ConfigController.Instance().GetEquipmentSprite(id);
                break;
            case 2:
                m_ctl.ResIcon.gameObject.SetActive(true);
                m_ctl.ResIcon.sprite = ConfigController.Instance().GetConsumeSprite(id);
                break;
        }
    }
    #endregion

    #region PrivateMethod
    private void Awake()
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
        GKUIEventTriggerListener.Get(m_ctl.ConfirmBtn.gameObject).onClick = OnConfirmClick;
		GKUIEventTriggerListener.Get(m_ctl.OKBtn.gameObject).onClick = OnOKClick;
		GKUIEventTriggerListener.Get(m_ctl.CancelBtn.gameObject).onClick = OnCancelClick;
        GKUIEventTriggerListener.Get(m_ctl.ResourceMegRoot.gameObject).onClick = OnConfirmClick;
    }

    private void Init()
    {
        OnConfirmFun = null;
    }

    private void OnConfirmClick(GameObject go)
    {
        if (null != OnConfirmFun)
        {
            OnConfirmFun();
        }
        Close();
        ShowNextMessage();
    }

	private void OnOKClick(GameObject go)
	{
		if (null != OnOKFun)
		{
			OnOKFun ();
		}
        Close();
        ShowNextMessage();
	}

	private void OnCancelClick(GameObject go)
	{
		if (null != OnCancelFun)
		{
			OnCancelFun ();
		}
        Close();
        ShowNextMessage();
	}
    #endregion

    #region Message
    public class MessageData
    {
        public MessageData(int type, string title, string content, string okText,
                           UIMessageBox.OnConfirmBtnClick ok, string cancelText,
                           UIMessageBox.OnConfirmBtnClick cancel,
                           int resType = 0, int resID = 0)
        {
            // 本地化处理.
            if (string.Equals("Tips", title))
            {
                title = DataController.Instance().GetLocalization(91);
            }
            if (string.Equals("OK", okText))
            {
                okText = DataController.Instance().GetLocalization(92);
            }
            if (string.Equals("Cancel", cancelText))
            {
                cancelText = DataController.Instance().GetLocalization(93);
            }
            _type = type;
            _title = title;
            _content = content;
            _okText = okText;
            _ok = ok;
            _cancelText = cancelText;
            _cancel = cancel;
            _resType = resType;
            _resID = resID;
        }

        // 0, normal, 1, selected. 2, resource.
        public int _type = 0;
        public string _title = "";
        public string _content = "";
        public string _okText = "";
        public OnConfirmBtnClick _ok;
        public string _cancelText = "";
        public OnConfirmBtnClick _cancel;

        public int _resType = 0;
        public int _resID = 0;
    }
    private static Queue<MessageData> _MessageQueue = new Queue<MessageData>();
    private static bool _uiMessageShowing = false;
    public static void ShowUIMessage(string content, string btnText = "OK",
                              OnConfirmBtnClick fun = null, string title = "Tips")
    {
        MessageData data = new MessageData(0, title, content, btnText, fun, "", null);
        _MessageQueue.Enqueue(data);
        if (!_uiMessageShowing)
        {
            ShowNextMessage();
        }
    }

    public static void ShowUISelectMessage(string content, string okText = "OK", string cancelText = "Cancel",
                                    OnConfirmBtnClick ok = null, OnConfirmBtnClick cancel = null, string title = "Tips")
    {

        MessageData data = new MessageData(1, title, content, okText, ok, cancelText, cancel);
        _MessageQueue.Enqueue(data);
        if (!_uiMessageShowing)
        {
            ShowNextMessage();
        }
    }

    public static void ShowUIResMessage(string content, int type, int id, string title = "Tips")
    {

        MessageData data = new MessageData(2, title, content, "", null, "", null, type, id);
        _MessageQueue.Enqueue(data);
        if (!_uiMessageShowing)
        {
            ShowNextMessage();
        }
    }

    public static void ShowNextMessage()
    {
        _uiMessageShowing = false;
        if (_MessageQueue.Count <= 0)
            return;

        _uiMessageShowing = true;
        var data = _MessageQueue.Dequeue();
        var msg = UIMessageBox.Open();
        switch (data._type)
        {
            case 0:
                msg.Show(data._content, data._okText, data._ok, data._title);
                break;
            case 1:
                msg.ShowSelectMsgBox(data._content, data._okText, data._cancelText, data._ok, data._cancel, data._title);
                break;
            case 2:
                msg.ShowResource(data._content, data._resType, data._resID);
                break;
        }
    }
    #endregion
}
