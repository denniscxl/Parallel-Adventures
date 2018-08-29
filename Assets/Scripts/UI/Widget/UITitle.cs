using UnityEngine.UI;
using UnityEngine;
using GKBase;
using GKData;
using GKUI;

public class UITitle : SingletonUIBase<UITitle>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public Image Coin;
        public Image Diamond;
        public Image Belief;
        public Image Food;
        public Text CoinValue;
        public Text DiamondValue;
        public Text BeliefValue;
        public Text FoodValue;
        public Button OpitionBtn;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private bool _bFighting = false;
    #endregion

    #region PublicMethod
    public void SetState(bool bFight)
    {

        _bFighting = bFight;
        if (null == m_ctl)
            return;

        m_ctl.Coin.gameObject.SetActive(!bFight);
        m_ctl.Diamond.gameObject.SetActive(!bFight);
        m_ctl.Belief.gameObject.SetActive(bFight);
        m_ctl.Food.gameObject.SetActive(bFight);

        if (bFight)
        {
            ListenerLevelData();
            SetBelief(LevelController.Instance().GetBelief(PlayerController.Instance().Camp));
            SetFood(LevelController.Instance().GetFood(PlayerController.Instance().Camp));
        } 
    }
    public void SetBelief(int val)
    {
        m_ctl.BeliefValue.text = val.ToString();
    }

    public void SetFood(int val)
    {
        m_ctl.FoodValue.text = val.ToString();
    }

    public void ListenerLevelData()
    {
        var levelData = LevelController.Instance().GetDataBase(PlayerController.Instance().Camp);
        if (null == levelData)
            return;
        levelData.GetAttribute((int)EObjectAttr.Belief).OnAttrbutChangedEvent += OnBeliefChanged;
        levelData.GetAttribute((int)EObjectAttr.Food).OnAttrbutChangedEvent += OnFoodChanged;
    }

    public void ReleaseLevelData()
    {
        if (null == LevelController.Instance())
            return;
        var levelData = LevelController.Instance().GetDataBase(PlayerController.Instance().Camp);
        if (null == levelData)
            return;
        levelData.GetAttribute((int)EObjectAttr.Belief).OnAttrbutChangedEvent -= OnBeliefChanged;
        levelData.GetAttribute((int)EObjectAttr.Food).OnAttrbutChangedEvent -= OnFoodChanged;
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
        GKUIEventTriggerListener.Get(m_ctl.OpitionBtn.gameObject).onClick = OnOptition;
        var playerData = PlayerController.Instance().GetDataBase();
        playerData.GetAttribute((int)EObjectAttr.Coins).OnAttrbutChangedEvent += OnCoinChanged;
        playerData.GetAttribute((int)EObjectAttr.Diamond).OnAttrbutChangedEvent += OnDiamondChanged;
    }

    private void Init()
    {
        SetState(_bFighting);
        m_ctl.CoinValue.text = PlayerController.Instance().Coin.ToString();
        m_ctl.DiamondValue.text = PlayerController.Instance().Diamond.ToString();
    }

    private void OnDestroy()
    {
        var playerData = PlayerController.Instance().GetDataBase();
        playerData.GetAttribute((int)EObjectAttr.Coins).OnAttrbutChangedEvent -= OnCoinChanged;
        playerData.GetAttribute((int)EObjectAttr.Diamond).OnAttrbutChangedEvent -= OnDiamondChanged;
    }

    private void OnOptition(GameObject go)
    {
        UIOption.Open();
    }

    private void OnCoinChanged(object obj, GKCommonValue attr)
    {
        m_ctl.CoinValue.text = attr.ValInt.ToString();
    } 

    private void OnDiamondChanged(object obj, GKCommonValue attr)
    {
        m_ctl.DiamondValue.text = attr.ValInt.ToString();
    } 

    private void OnBeliefChanged(object obj, GKCommonValue attr)
    {
        m_ctl.BeliefValue.text = attr.ValInt.ToString();
    }

    private void OnFoodChanged(object obj, GKCommonValue attr)
    {
        m_ctl.FoodValue.text = attr.ValInt.ToString();
    } 
    
    #endregion
}
