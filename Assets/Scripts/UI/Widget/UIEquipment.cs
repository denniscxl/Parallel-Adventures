using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GKBase;
using GKUI;

public class UIEquipment : SingletonUIBase<UIEquipment>
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public GameObject Root;
        public Text UnitNameText;
        public Button LeftBtn;
        public Button RightBtn;
        public GameObject SoltSample;
        public GameObject EquipmentContent;
        public Button BackBtn;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private List<int> _cardIDList = new List<int>();
    private int _curCardIdx = 0;
    // 角色装备槽实例.
    private UIEquipmentSoltSample[] _solts = new UIEquipmentSoltSample[(int)EquipmentPart.Count];
    // 存储刷新标志位.
    private bool _bChange = false;
    #endregion

    #region PublicMethod
    public void SetData(int unitID)
    {
        // 设置角色卡牌循环链表及索引.
        _cardIDList.Clear();
        int idx = 0;
        bool find = false;
        foreach (int id in PlayerController.Instance().GetPlayerCards().Keys)
        {
            _cardIDList.Add(id);
            if (id == unitID)
            {
                _curCardIdx = idx;
                find = true;
            }
            idx++;
        }
        // 如果unitID未指定, 默认索引为0.
        if (!find)
            _curCardIdx = 0;

    }

    // 获取当前卡片ID.
    public int GetCurrentCardID()
    {
        return _cardIDList[_curCardIdx];
    }

    public void Changed()
    {
        _bChange = true;
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
        GKUIEventTriggerListener.Get(m_ctl.LeftBtn.gameObject).onClick = OnLeft;
        GKUIEventTriggerListener.Get(m_ctl.RightBtn.gameObject).onClick = OnRight;
        GKUIEventTriggerListener.Get(m_ctl.BackBtn.gameObject).onClick = OnBack;
        PlayerController.Instance().OnLanguageChangedEvent += OnLanguageChanged;
    }

    private void Init()
    {
        if (0 == _cardIDList.Count)
        {
            SetData(-1);
        }

        for (int i = 0; i < (int)EquipmentPart.Count; i++)
        {
            var go = GameObject.Instantiate(m_ctl.SoltSample);
            go.SetActive(true);
            GK.SetParent(go, m_ctl.EquipmentContent, false);
            var sample = GK.GetOrAddComponent<UIEquipmentSoltSample>(go);
            // 设置装备信息.
            sample.SetData((EquipmentPart)i);
            _solts[i] = sample;
        }
        Refresh();
    }

    public void Refresh()
    {
        m_ctl.Root.SetActive(0 != _cardIDList.Count);

        if (0 == _cardIDList.Count)
            return;

        // 获取卡牌信息.
        var data = PlayerController.Instance().GetCardDetaileFromPlayer(_cardIDList[_curCardIdx]);
        if (null == data)
        {
            Debug.LogError(string.Format("Refresh equipment faile. Cna't find card data. _curCardIdx: {0}, cardID: {1}", _curCardIdx, _cardIDList[_curCardIdx]));
            return;
        }

        // 卡牌名称刷新.
        m_ctl.UnitNameText.text = DataController.Instance().GetLocalization(data.GetAttribute((int)EObjectAttr.Name).ValInt, LocalizationSubType.Unit);

        List<int> soltList = data.GetAttributeList((int)EObjectAttr.Unit_Equipments).ValInt;

        // 刷新装备槽.
        for (int i = 0; i < (int)EquipmentPart.Count; i++)
        {
            int id = 0;
            if (i < soltList.Count)
                id = soltList[i];
            _solts[i].Refresh(id, _cardIDList[_curCardIdx]);
        }
    }

    private void OnLeft(GameObject go)
    {
        _curCardIdx--;
        if (_curCardIdx < 0)
            _curCardIdx = _cardIDList.Count - 1;

        Refresh();
    }

    private void OnRight(GameObject go)
    {
        _curCardIdx++;
        if (_curCardIdx >= _cardIDList.Count)
            _curCardIdx = 0;

        Refresh();
    }

    private void OnDestroy()
    {
        PlayerController.Instance().OnLanguageChangedEvent -= OnLanguageChanged;
    }

    // 刷新语言.
    private void OnLanguageChanged()
    {
        Refresh();
    }

    private void OnBack(GameObject go)
    {
        if(_bChange)
        {
            DataController.Instance().SaveCards();
            DataController.Instance().SaveInventory();
        }
        Close();
    }
    #endregion
}
