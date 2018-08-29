using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using GKBase;
using GKMap;
using GKRole;
using GKUI;

public class UIMain : SingletonUIBase<UIMain>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {

        public GameObject ContentRoot;
        public UIMainCard CardSample;
        public GameObject CardRoot;
        // 当前角色卡.
        public ScrollRect ScrollViewRoot;
        public Toggle ShowContentBtn;
        public Toggle CommandChangedBtn;
        // 角色命令.
        public GameObject Conmmand;
        public Toggle MoveToggle;
        public Toggle FightToggle;
        public Toggle SupportToggle;
        public Toggle DefenseToggle;
        public Toggle PursuitToggle;
        public Toggle RetreatToggle;
        public Toggle AmbushToggle;
        public Toggle DetailToggle;
        public Toggle VillagerToggle;
        public Toggle FoodToggle;
        // MiniMap.
        public Toggle Cameramode;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private UIVirtualJoyStick _vietualJoyStick = null;
    private UITitle _uiTitle;
    #endregion

    #region PublicMethod
    // 摄像机变更按钮只处理鸟瞰与跟随视角命令.
    public void OnCameraModeChanged()
    {
        if (MachineStateID.Overall == CameraController.Instance().GetCurrentState())
            return;
        
        bool bBirdsEye = m_ctl.Cameramode.isOn;
        CameraController.Instance().ChangeState(bBirdsEye ? MachineStateID.BirdsEye : MachineStateID.Follow);
    }

    public void OnShowContentChanged()
    {
        bool bList = m_ctl.ShowContentBtn.isOn;
        float offestY = m_ctl.ShowContentBtn.isOn ? 75 : -75;
        m_ctl.ContentRoot.GetComponent<RectTransform>().anchoredPosition = new Vector3(0, offestY, 0);
        // 如果为显示, 需要判断原始状态是否为显示.
        if(!m_ctl.ShowContentBtn.isOn)
        {
            ShowCommand(false);
        }
        else
        {
            ShowCommand(m_ctl.CommandChangedBtn.isOn);
        }
    }

    private void ShowCommand(bool show)
    {
        float offestX = show ? -100 : 100;
        m_ctl.Conmmand.GetComponent<RectTransform>().anchoredPosition = new Vector3(offestX, -235, 0);
    }

    public void OnCommandChanged()
    {
        bool bShowCommand = m_ctl.CommandChangedBtn.isOn;
        ShowCommand(bShowCommand);
        m_ctl.Cameramode.gameObject.SetActive(!bShowCommand);
        if(!bShowCommand)
        {
            if(MachineStateID.Overall != CameraController.Instance().LastState)
                CameraController.Instance().ChangeState(CameraController.Instance().LastState);
        }
        else
        {
            CameraController.Instance().ChangeState(MachineStateID.Overall);
        }
        // 重置命令状态.
        ResetCommandToggle();
        LevelController.Instance().SetCommand(CommandType.NoCommand);
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
        GKUIEventTriggerListener.Get(m_ctl.MoveToggle.gameObject).onClick = OnMove;
        GKUIEventTriggerListener.Get(m_ctl.FightToggle.gameObject).onClick = OnFight;
        GKUIEventTriggerListener.Get(m_ctl.SupportToggle.gameObject).onClick = OnSupport;
        GKUIEventTriggerListener.Get(m_ctl.DefenseToggle.gameObject).onClick = OnDefense;
        GKUIEventTriggerListener.Get(m_ctl.PursuitToggle.gameObject).onClick = OnPursuit;
        GKUIEventTriggerListener.Get(m_ctl.RetreatToggle.gameObject).onClick = OnRetreat;
        GKUIEventTriggerListener.Get(m_ctl.AmbushToggle.gameObject).onClick = OnAmbush;
        GKUIEventTriggerListener.Get(m_ctl.DetailToggle.gameObject).onClick = OnDetail;
        GKUIEventTriggerListener.Get(m_ctl.VillagerToggle.gameObject).onClick = OnVillager;
        GKUIEventTriggerListener.Get(m_ctl.FoodToggle.gameObject).onClick = OnFood;
        LevelController.Instance().OnNewPlayerUnitEvent += RefreshCards;
        LevelController.Instance().OnSelectUnitChangedEvent += RefreshSelectCards;
        LevelController.Instance().OnSelectTileChangedEvent += OnSelectTileChanged;
        CameraController.Instance().OnStateChangedEvent += CameraStateChange;
    }

    private void Init()
    {
        RefreshCards();
        CameraStateChange(CameraController.Instance().GetCurrentState());
        if (null == _vietualJoyStick)
            _vietualJoyStick = UIVirtualJoyStick.Open();
        _uiTitle = UITitle.Open();
        _uiTitle.SetState(true);

        OnShowContentChanged();
        OnCommandChanged();
    }

    private void OnDestroy()
    {
        LevelController.Instance().OnNewPlayerUnitEvent -= RefreshCards;
        LevelController.Instance().OnSelectUnitChangedEvent -= RefreshSelectCards;
        LevelController.Instance().OnSelectTileChangedEvent -= OnSelectTileChanged;
        CameraController.Instance().OnStateChangedEvent -= CameraStateChange;
    }

    //  刷新卡片列表.
    private void RefreshCards()
    {
        GK.DestroyAllChildren(m_ctl.CardRoot);
        // 初始化玩家角色池信息.
        var tCrads = LevelController.Instance().GetCampData(PlayerController.Instance().Camp);
        if (null == tCrads)
            return;
        foreach (var card in tCrads)
        {
            var go = GameObject.Instantiate(m_ctl.CardSample.gameObject);
            if (null != go)
            {
                go.SetActive(true);
                GK.SetParent(go, m_ctl.CardRoot, false);
                GK.GetOrAddComponent<UIMainCard>(go).SetID(card.Key, card.Value);
            }
        }
        // 新角色登录时, 隐藏Command面板.
        // 因为Command面板的切换按钮会使摄像机返回前一个状态.
        // 设置前一个状态为当前状态.
        m_ctl.CommandChangedBtn.isOn = false;
    }

    private void RefreshSelectCards()
    {
        int id = LevelController.Instance().GetSelectUnitID();
        ResetCommandToggle();
    }

    private void CameraStateChange(MachineStateID state)
    {
        bool show = (state == MachineStateID.BirdsEye) ? true : false;
        m_ctl.Cameramode.isOn = show;
        if (null != _vietualJoyStick)
            _vietualJoyStick.gameObject.SetActive(show);
    }

    private void OnSelectTileChanged()
    {
       
    }
    #endregion

    #region Commond
    private void OnMove(GameObject go)
    {
        if (IsSetNoCommand(m_ctl.MoveToggle.isOn, CommandType.Move))
            return;

        if (!m_ctl.MoveToggle.isOn)
            return;
        
        LevelController.Instance().SetCommand(CommandType.Move);
    }

    private void OnFight(GameObject go)
    {
        if (IsSetNoCommand(m_ctl.FightToggle.isOn, CommandType.Attack))
            return;

        if (!m_ctl.FightToggle.isOn)
            return;

        LevelController.Instance().SetCommand(CommandType.Attack);
    }

    private void OnSupport(GameObject go)
    {
        if (IsSetNoCommand(m_ctl.SupportToggle.isOn, CommandType.Support))
            return;

        if (!m_ctl.SupportToggle.isOn)
            return;

        LevelController.Instance().SetCommand(CommandType.Support);
    }

    private void OnDefense(GameObject go)
    {
        if (IsSetNoCommand(m_ctl.DefenseToggle.isOn, CommandType.Defense))
            return;

        if (!m_ctl.DefenseToggle.isOn)
            return;

        LevelController.Instance().SetCommand(CommandType.Defense);
    }

    private void OnPursuit(GameObject go)
    {
        if (IsSetNoCommand(m_ctl.PursuitToggle.isOn, CommandType.Pursuit))
            return;

        if (!m_ctl.PursuitToggle.isOn)
            return;

        LevelController.Instance().SetCommand(CommandType.Pursuit);
    }

    private void OnRetreat(GameObject go)
    {
        if (IsSetNoCommand(m_ctl.RetreatToggle.isOn, CommandType.Retreat))
            return;

        if (!m_ctl.RetreatToggle.isOn)
            return;

        LevelController.Instance().SetCommand(CommandType.Retreat);
    }

    private void OnAmbush(GameObject go)
    {
        if (IsSetNoCommand(m_ctl.AmbushToggle.isOn, CommandType.Ambush))
            return;

        if (!m_ctl.AmbushToggle.isOn)
            return;

        LevelController.Instance().SetCommand(CommandType.Ambush);
    }

    private void OnDetail(GameObject go)
    {

    }

    private void OnVillager(GameObject go)
    {
        if (IsSetNoCommand(m_ctl.VillagerToggle.isOn, CommandType.Village))
            return;

        if (!m_ctl.VillagerToggle.isOn)
            return;

        LevelController.Instance().SetCommand(CommandType.Village);
    }

    private void OnFood(GameObject go)
    {
        if (IsSetNoCommand(m_ctl.FoodToggle.isOn, CommandType.Food))
            return;

        if (!m_ctl.FoodToggle.isOn)
            return;

        LevelController.Instance().SetCommand(CommandType.Food);
    }

    // 是否取消当前设置.
    // 二次点选相同按钮触发.
    private bool IsSetNoCommand(bool isOn, CommandType type)
    {
        if (!isOn && type == LevelController.Instance().GetCommand())
        {
            LevelController.Instance().SetCommand(CommandType.NoCommand);
            return true;
        }
        return false;
    }
    // 重置勾选按钮
    private void ResetCommandToggle()
    {
        m_ctl.MoveToggle.isOn = false;
        m_ctl.FightToggle.isOn = false;
        m_ctl.SupportToggle.isOn = false;
        m_ctl.DefenseToggle.isOn = false;
        m_ctl.PursuitToggle.isOn = false;
        m_ctl.RetreatToggle.isOn = false;
        m_ctl.AmbushToggle.isOn = false;
        m_ctl.VillagerToggle.isOn = false;
        m_ctl.FoodToggle.isOn = false;
    }
    #endregion

}
