using UnityEngine.UI;
using GKBase;
using GKRole;
using GKData;
using GKUI;

public class UIMainCard : UIBase
{
    
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public RawImage Icon;
        public Scrollbar HpScrollBar;
        public Scrollbar MpScrollBar;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private int _id;
    private long _guid;
    private GKUnit _unit;
    #endregion

    #region PublicMethod
    public void SetID(int id, long guid)
    {
        if(0 <= id)
        {
            _id = id;
            _guid = guid;
            if (guid == LevelController.Instance().GetSelectUnitID())
                gameObject.GetComponent<Toggle>().isOn = true;
            var go = LevelController.Instance().GetTargetByID(_guid);
            if(null != go)
            {
                _unit = go.GetComponent<GKUnit>();
            }
            InitAttribute();
        }
    }

    public void OnClick()
    {
        LevelController.Instance().SelectUnit(_guid);

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
        m_ctl.Icon.texture = ConfigController.Instance().GetCardIconTexture(_id);
        // 设置当前选择对象.
        if (_id == LevelController.Instance().GetSelectUnitID())
            gameObject.GetComponent<Toggle>().isOn = true;
    }

    private void InitAttribute()
    {
        if (null == _unit)
            return;

        _unit.GetAttribute(EObjectAttr.Hp).OnAttrbutChangedEvent += OnHpChanged;
        _unit.GetAttribute(EObjectAttr.Mp).OnAttrbutChangedEvent += OnMpChanged;
    }

    private void OnDestroy()
    {
        if (null == _unit)
            return;

        _unit.GetAttribute(EObjectAttr.Hp).OnAttrbutChangedEvent -= OnHpChanged;
        _unit.GetAttribute(EObjectAttr.Mp).OnAttrbutChangedEvent -= OnMpChanged;
    }

    private void OnHpChanged(object obj, GKCommonValue attr)
    {
        if (null == _unit)
            return;
        int max = _unit.GetAttribute(EObjectAttr.MaxHp).ValInt;
        m_ctl.HpScrollBar.size = (float)attr.ValInt / max;
    }

    private void OnMpChanged(object obj, GKCommonValue attr)
    {
        if (null == _unit)
            return;
        int max = _unit.GetAttribute(EObjectAttr.MaxMp).ValInt;
        m_ctl.MpScrollBar.size = (float)attr.ValInt / max;
    }
    #endregion
}
