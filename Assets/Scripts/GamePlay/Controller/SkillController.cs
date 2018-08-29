using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using GKBase;
using GKData;

public class SkillController : GKSingleton<SkillController>  
{
    #region PublicField
    // 技能点变更回调.
    public Action OnSkillPointsChanged;
    #endregion

    #region PrivateField
    private GKDataBase _data = new GKDataBase();
    #endregion

    #region PublicMethod
    // 通过技能树索引来获取最大技能数.
    public int GetSkillCount(int treeID)
    {
        var skillTree = DataController.Data.GetSkillTreeData(treeID);
        if(null == skillTree)
            return 0;
        return skillTree.skills.Count;
    }

    // 通过技能树ID及当前卡牌技能等级获取技能索引.
    public List<int> GetSkillsFromTreeID(int treeID, List<int> skillLv)
    {
        var lst = DataController.Data.GetSkillTreeData(treeID).skills;
        if (null == lst)
            return null;

        List<int> curLst = new List<int>();
        for (int i = 0; i < lst.Count; i++)
        {
            int idx = lst[i];
            // 技能树索引加上当前卡牌技能ID.
            if (i < skillLv.Count)
                idx += skillLv[i];
            curLst.Add(idx);
        }
        return curLst;
    }

    // 判断当前是否满足升级条件. 0 为满足. 其他为格式错误码.
    public int CanLvUp(GameData.SkillData skill, int cardID)
    {
        // 获取卡牌数据.
        var data = PlayerController.Instance().GetCardDetaileFromPlayer(cardID);
        if (null == data || null == skill)
            return (int)ErrorCodeType.CardDataMissing;

        // 判断力量.
        if (data.GetAttribute((int)EObjectAttr.Strength).ValInt < skill.strength)
            return (int)ErrorCodeType.AttributeNotEnough;

        // 判断敏捷.
        if (data.GetAttribute((int)EObjectAttr.Agility).ValInt < skill.agility)
            return (int)ErrorCodeType.AttributeNotEnough;

        // 判断智力.
        if (data.GetAttribute((int)EObjectAttr.Intelligence).ValInt < skill.intelligence)
            return (int)ErrorCodeType.AttributeNotEnough;

        // 判断点数.
        int totalPoints = data.GetAttribute((int)EObjectAttr.SkillLevel).ValInt;
        int usePoints = data.GetAttribute((int)EObjectAttr.UsedSkillPoint).ValInt;
        if (totalPoints - usePoints -1 <= 0)
            return (int)ErrorCodeType.SkillPointNotEnough;

        // 判断技能等级上限.
        if ((skill.key % 100) >= 99)
            return (int)ErrorCodeType.MaxLevel;

        // 获取卡牌技能信息.
        int treeID = data.GetAttribute((int)EObjectAttr.SkillTreeID).ValInt;
        var skillLst = data.GetAttributeList((int)EObjectAttr.Unit_Skills).ValInt;
        var lst = SkillController.Instance().GetSkillsFromTreeID(treeID, skillLst);
        if (null == lst)
        {
            return (int)ErrorCodeType.CardDataMissing;
        }

        // 判断依赖技能A.
        if(!CardMeetingSkillRequirement(lst, skill.demandA))
            return (int)ErrorCodeType.DependentSkillLevelNotEnough;

        // 判断依赖技能B.
        if (!CardMeetingSkillRequirement(lst, skill.demandB))
            return (int)ErrorCodeType.DependentSkillLevelNotEnough;

        // 判断依赖技能C.
        if (!CardMeetingSkillRequirement(lst, skill.demandC))
            return (int)ErrorCodeType.DependentSkillLevelNotEnough;

        return 0;
    }

    // 判断卡牌是否满足技能需求.
    public bool CardMeetingSkillRequirement(List<int> lst, int demand)
    {
        int demandID = 0;
        int demandLv = 0;
        if (-1 != demand)
        {
            var data = DataController.Data.GetSkillData(demand);
            if (null == data)
                return false;

            demandID = data.key / 100;
            demandLv = data.key % 100;
            foreach (var l in lst)
            {
                if ((l / 100) == demandID)
                {
                    if (demandLv > (l % 100))
                        return false;
                    break;
                }
            }
        }
        return true;
    }

    // 技能升级. idx 升级技能索引.
    public void SkillUpLv(int cardID, int idx)
    {
        var card = PlayerController.Instance().GetPlayerCard(cardID);
        if (null == card)
            return;
        card.UsePointToSkillLevelUp(idx);

        if (null != OnSkillPointsChanged)
            OnSkillPointsChanged();
    }
    #endregion

    #region PrivateMethod

    #endregion
}
