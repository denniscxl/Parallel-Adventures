using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using GKBase;
using GKMap;
using GKData;
using GKUI;

public class UICreateUnitSample : UIBase
{
    #region Serializable
    [System.Serializable]
    public class Controls
    {
        public RawImage Icon;
        public Text LevelVal;
        public Text BeliefVal;
        public Text FoodVal;
        public Text StrVal;
        public Text AgiVal;
        public Text IntVal;
        public GameObject PassiveSkillRoot;
        public GameObject MoveTypeRoot;
        public Image MoveTypeSample;
    }
    #endregion

    #region PublicField

    #endregion

    #region PrivateField
    [System.NonSerialized]
    private Controls m_ctl;
    private int _tileID;
    private int _id;
    private GameObject _sample;
    #endregion

    #region PublicMethod
    public void SyncData(int tileID, GKDataBase data, GameObject sample)
    {
        _tileID = tileID;
        _id = data.GetAttribute((int)EObjectAttr.ID).ValInt;
        _sample = sample;
    }

    public void OnClick(GameObject go)
    {
        var data = LevelController.Instance().UseCard(PlayerController.Instance().Camp, _id);
        if (null != data)
        {
            LevelController.Instance().CreateUnit(PlayerController.Instance().Camp, data, _tileID);
        }
        UICreateUnit.Close();
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
        //GKUIEventTriggerListener.Get(gameObject).onClick = OnClick;
    }

    private void Init()
    {
        Refresh();
    }

    private void Refresh()
    {
        var c = PlayerController.Instance().GetCardDetaileFromFight(_id);
        if (null == c)
        {
            Debug.LogWarning(string.Format("Create unit sample faile. Can't find card data. ID: {0}", _id));
            return;
        }
        m_ctl.Icon.texture = ConfigController.Instance().GetCardIconTexture(_id);
        m_ctl.LevelVal.text = c.GetAttribute((int)EObjectAttr.Level).ValInt.ToString();
        m_ctl.StrVal.text = c.GetAttribute((int)EObjectAttr.TotalStrength).ValInt.ToString();
        m_ctl.AgiVal.text = c.GetAttribute((int)EObjectAttr.TotalAgility).ValInt.ToString();
        m_ctl.IntVal.text = c.GetAttribute((int)EObjectAttr.TotalIntelligence).ValInt.ToString();

        // 获取卡牌技能信息.
        int treeID = c.GetAttribute((int)EObjectAttr.SkillTreeID).ValInt;
        List<int> skillLst = new List<int>();
        List<int> lst = SkillController.Instance().GetSkillsFromTreeID(treeID, skillLst);
        if (null != lst)
        {
            foreach (var id in lst)
            {
                var go = GameObject.Instantiate(_sample);
                go.SetActive(true);
                var skillSample = GK.GetOrAddComponent<UICreateUnitSkillSample>(go);
                skillSample.SyncData(id);
                GK.SetParent(go, m_ctl.PassiveSkillRoot, false);
            }
        }
        RefreshMoveTypeIcon(c);
    }

    // 刷新移动类型.
    private void RefreshMoveTypeIcon(GKDataBase data)
    {
        GK.DestroyAllChildren(m_ctl.MoveTypeRoot);
        int layermask = data.GetAttribute((int)EObjectAttr.LayerMask).ValInt;
        if ((layermask & (int)MoveType.Road) == (int)MoveType.Road)
        {
            CloneMoveTypeIcon(0);
        }
        if ((layermask & (int)MoveType.Grass) == (int)MoveType.Grass)
        {
            CloneMoveTypeIcon(1);
        }
        if ((layermask & (int)MoveType.River) == (int)MoveType.River)
        {
            CloneMoveTypeIcon(2);
        }
    }

    // 克隆移动类型图标.
    private void CloneMoveTypeIcon(int id)
    {
        var moveIcon = GameObject.Instantiate(m_ctl.MoveTypeSample) as Image;
        moveIcon.gameObject.SetActive(true);
        GK.SetParent(moveIcon.gameObject, m_ctl.MoveTypeRoot, false);
        moveIcon.sprite = ConfigController.Instance().GetMoveTypeSprite(id);
    }
    #endregion
}
