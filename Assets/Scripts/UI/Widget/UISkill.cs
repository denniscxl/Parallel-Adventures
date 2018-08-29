using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GKBase;
using GKData;
using GKUI;

public class UISkill : SingletonUIBase<UISkill>
{
    void HandleOnConfirmBtnClick()
    {
    }


    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public GameObject TitleRoot;
        public Text UnitNameText;
        public Button LeftBtn;
        public Button RightBtn;
        public Text TotalPointsText;
        public Text UnusedPointsText;
        public Button ResetBtn;
        public GameObject SkillContent;
        public UISkillSample UISkillSample;
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
    private readonly int _resetCost = 300;
    #endregion

    #region PublicMethod
    public void SetData(int unitID)
    {
        // 设置角色卡牌循环链表及索引.
        _cardIDList.Clear();
        int idx = 0;
        bool find = false;
        foreach(int id in PlayerController.Instance().GetPlayerCards().Keys)
        {
            _cardIDList.Add(id);
            if(id == unitID)
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

    public override void OnClose()
    {
        base.OnClose();
        SkillController.Instance().OnSkillPointsChanged -= UpdateSkillPointCount;
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
        GKUIEventTriggerListener.Get(m_ctl.ResetBtn.gameObject).onClick = OnReset;
        GKUIEventTriggerListener.Get(m_ctl.BackBtn.gameObject).onClick = OnBack;
        SkillController.Instance().OnSkillPointsChanged += UpdateSkillPointCount;
        PlayerController.Instance().OnLanguageChangedEvent += OnLanguageChanged;
    }

    private void Init()
    {
        if(0 == _cardIDList.Count)
        {
            SetData(-1);
        }

        UpdateSkillPointCount();
    }

    private void Refresh(GKDataBase data)
    {
        GK.DestroyAllChildren(m_ctl.SkillContent);

        if (null == data)
            return;

        // 获取卡牌技能信息.
        int treeID = data.GetAttribute((int)EObjectAttr.SkillTreeID).ValInt;
        var skillLst = data.GetAttributeList((int)EObjectAttr.Unit_Skills).ValInt;
        var lst = SkillController.Instance().GetSkillsFromTreeID(treeID, skillLst);
        if (null == lst)
        {
            Debug.LogError(string.Format("Refresh skill list faile. Cna't find skill tree data. treeID: {0}", treeID));
            return;
        }

        int idx = 0;
        // 克隆并初始化卡片.
        foreach(var skill in lst)
        {
            var go = GameObject.Instantiate(m_ctl.UISkillSample.gameObject);
            go.SetActive(true);
            GK.SetParent(go, m_ctl.SkillContent, false);
            var sample = GK.GetOrAddComponent<UISkillSample>(go);
            // 设置技能信息.
            sample.SetData(skill, _cardIDList[_curCardIdx], idx);
            idx++;
        }       
    }

    // 技能点变更回调函数.
    private void UpdateSkillPointCount()
    {

        m_ctl.TitleRoot.SetActive(0 != _cardIDList.Count);

        if (0 == _cardIDList.Count)
            return;

        // 获取卡牌信息.
        var data = PlayerController.Instance().GetCardDetaileFromPlayer(_cardIDList[_curCardIdx]);
        if (null == data)
        {
            Debug.LogError(string.Format("UpdateSkillPointCount faile. Cna't find card data. _curCardIdx: {0}, cardID: {1}", _curCardIdx, _cardIDList[_curCardIdx]));
            return;
        }

        int totalPoints = data.GetAttribute((int)EObjectAttr.SkillLevel).ValInt - 1;
        int usePoints = data.GetAttribute((int)EObjectAttr.UsedSkillPoint).ValInt;
        int canUsePoints = totalPoints - usePoints;
        if (canUsePoints < 0)
        {
            canUsePoints = 0;
            Debug.LogError(string.Format("UpdateSkillPointCount faile. . canUsePoints < 0 totalPoints: {0}, usCount: {1}", totalPoints, usePoints));
        }

        m_ctl.TotalPointsText.text = totalPoints.ToString();
        m_ctl.UnusedPointsText.text = canUsePoints.ToString();

        // 卡片名称刷新.
        m_ctl.UnitNameText.text = DataController.Instance().GetLocalization(data.GetAttribute((int)EObjectAttr.Name).ValInt, LocalizationSubType.Unit);

        // 刷新技能列表.
        Refresh(data);
    }

    private void OnLeft(GameObject go)
    {
        _curCardIdx--;
        if (_curCardIdx < 0)
            _curCardIdx = _cardIDList.Count - 1;

        UpdateSkillPointCount();
    }

    private void OnRight(GameObject go)
    {
        _curCardIdx++;
        if (_curCardIdx >= _cardIDList.Count)
            _curCardIdx = 0;

        UpdateSkillPointCount();
    }

    private void OnReset(GameObject go)
    {
        if (0 == _cardIDList.Count)
            return;

        // 二次确认是否重置技能点.
        UIMessageBox.ShowUISelectMessage(DataController.Instance().GetLocalization(90), 
                                                  DataController.Instance().GetLocalization(86), 
                                                  DataController.Instance().GetLocalization(87),  () =>
        {
            // 确认重置技能点.
            int diamond = PlayerController.Instance().Diamond;
            if (diamond < _resetCost)
            {
                // 钻石不足, 提示是否进入商店.
                UIMessageBox.ShowUISelectMessage(DataController.Instance().GetLocalization(89),
                                                          DataController.Instance().GetLocalization(86),
                                                          DataController.Instance().GetLocalization(87), () =>
                {
                    UIStore.Open();
                }, null);
            }
            else
            {
                PlayerController.Instance().Diamond -= _resetCost;
                // 重置当前卡片技能点.
                var card = PlayerController.Instance().GetPlayerCard(_cardIDList[_curCardIdx]);
                card.ResetSkillPoints();
                UpdateSkillPointCount();
            }
        }, null);
    }

    private void OnBack(GameObject go)
    {
        Close();
        DataController.Instance().SaveCards();
    }

    private void OnDestroy()
    {
        SkillController.Instance().OnSkillPointsChanged -= UpdateSkillPointCount;
        PlayerController.Instance().OnLanguageChangedEvent -= OnLanguageChanged;
    }

    // 刷新语言.
    private void OnLanguageChanged()
    {
        UpdateSkillPointCount();
    }
    #endregion
}
