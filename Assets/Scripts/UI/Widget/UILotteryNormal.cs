using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GKBase;
using GKUI;

public class UILotteryNormal : SingletonUIBase<UILotteryNormal>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public GameObject SelectPanel;
        public Button NineBtn;
        public Button FiveBtn;
        public Button OneBtn;
        public Button ExitBtn;
        public GameObject ResultPanel;
        public UILotteryCard LotteryCardSample;
        public Button DrawBtn;
        public Button TryAgainBtn;
        public Button BackBtn;
    }
    #endregion

    #region PublicField
    public void SetData(LotteryType type)
    {
        _lotteryModel = type;
    }
    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    [SerializeField]
    private GameObject[] _points;
    // 1 , 5, 9.
    private int _lotteryType = 1;
    // 缓存克隆卡片对象, 用来实现抽卡效果.
    private List<UILotteryCard> _cards = new List<UILotteryCard>();
    private LotteryType _lotteryModel = LotteryType.Coin;
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
        GKUIEventTriggerListener.Get(m_ctl.OneBtn.gameObject).onClick = OnOne;
        GKUIEventTriggerListener.Get(m_ctl.FiveBtn.gameObject).onClick = OnFive;
        GKUIEventTriggerListener.Get(m_ctl.NineBtn.gameObject).onClick = OnNine;
        GKUIEventTriggerListener.Get(m_ctl.ExitBtn.gameObject).onClick = OnExist;
        GKUIEventTriggerListener.Get(m_ctl.DrawBtn.gameObject).onClick = OnDraw;
        GKUIEventTriggerListener.Get(m_ctl.TryAgainBtn.gameObject).onClick = OnTryAgain;
        GKUIEventTriggerListener.Get(m_ctl.BackBtn.gameObject).onClick = OnBack;
    }

    private void Init()
    {

    }

    private void OnOne(GameObject go)
    {
        Pay(1, () =>
        {
            _lotteryType = 1;
            ChangeState(false);
        });
    }

    private void OnFive(GameObject go)
    {
        Pay(5, () =>
        {
            _lotteryType = 5;
            ChangeState(false);
        });
    }

    private void OnNine(GameObject go)
    {
        Pay(9, () =>
        {
            _lotteryType = 9;
            ChangeState(false); 
        });
    }

    private void OnExist(GameObject go)
    {
        Close();
    }

    private void OnDraw(GameObject go)
    {
        LotteryController.Instance().Get(_lotteryModel);
        foreach(var c in _cards)
        {
            c.Show(true);
        }
        UpdateResultPanelBtnsState(true);
    }

    private void OnTryAgain(GameObject go)
    {
        Pay(-1, () => 
        { 
            UpdateResultPanelBtnsState(false);
            InitDraws();
        });
    }

    private void Pay(int type, System.Action fun)
    {
        if (-1 == type)
            type = _lotteryType;

        var value = LotteryController.Instance().IsEnoughResourceToLottery(_lotteryModel, _lotteryType);
        if (-1 != value)
        {
            if (LotteryController.Instance().Pay(_lotteryModel, value))
            {
                fun();
            }
            else
            {
                Debug.LogWarning(string.Format("Pay faile. Cost value: {0}, level: {1}", value, _lotteryType));
            }
        }
        else
        {
            UIMessageBox.ShowUISelectMessage(DataController.Instance().GetLocalization(89),
                                                      DataController.Instance().GetLocalization(86), 
                                                      DataController.Instance().GetLocalization(87), () =>
            {
                UIStore.Open();
            }, null, "Tips");
        }
    }

    private void OnBack(GameObject go)
    {
        ChangeState(true);
    }

    private void InitDraws()
    {
        _cards.Clear();
        ClearPoints();
        UpdateResultPanelBtnsState(false);
       
        // 通过抽卡控制器计算并获取抽卡结果.
        var cards = LotteryController.Instance().CreateLotteryCards(_lotteryModel, _lotteryType);

        for (int i = 0; i < _lotteryType; i++)
        {
            // 克隆并初始化卡片.
            var go = GameObject.Instantiate(m_ctl.LotteryCardSample.gameObject);
            if (null != go && i < cards.Count)
            {
                go.SetActive(true);
                GK.SetParent(go, _points[i], false);
                var c = GK.GetOrAddComponent<UILotteryCard>(go);
                // 设置卡片图片.
                c.SetIcon(_lotteryModel, cards[i]);
                // 推入卡片缓存, 用来后续实现翻卡效果.
                _cards.Add(c);
            }
        }
    }

    private void ChangeState(bool bSelect)
    {
        m_ctl.SelectPanel.SetActive(bSelect);
        m_ctl.ResultPanel.SetActive(!bSelect);
        // 初始化抽卡列表.
        if(!bSelect)
        {
            InitDraws();
        }
    }

    private void ClearPoints()
    {
        foreach(var p in _points)
        {
            if (null == p)
                continue;
            GK.DestroyAllChildren(p);
        }
    }

    // 更新抽卡界面按钮状态. 
    // bDrawed: 是否抽卡完毕.
    private void UpdateResultPanelBtnsState(bool bDrawed)
    {
        m_ctl.DrawBtn.gameObject.SetActive(!bDrawed);
        m_ctl.BackBtn.gameObject.SetActive(bDrawed);
        m_ctl.TryAgainBtn.gameObject.SetActive(bDrawed);
    }
    #endregion
}
