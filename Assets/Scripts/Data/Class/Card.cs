using System.Collections;
using System.Collections.Generic;
using GKData;
using UnityEngine;
using GKUI;

// 游戏中角色信息结构体.
[System.Serializable]
public class Card
{
    #region PublicField
    public GKDataBase dataBase;
    #endregion

    #region PrivateField
    //private Dictionary<int, >
    #endregion

    #region PublicMethod
    public Card() {}

    public Card(int id, bool showMsg = true)
    {
        dataBase = ConfigController.Instance().GetNewCardData(id);
        if(showMsg)
        {
            string name = DataController.Instance().GetLocalization(dataBase.GetAttribute((int)EObjectAttr.Name).ValInt, LocalizationSubType.Unit);
            UIMessageBox.ShowUIResMessage(string.Format(DataController.Instance().GetLocalization(83), name), 0, id);
        }
        Init();
    } 

    // 通过字符窜来获取职业。
    static public List<int> GetJobs(string str)
    {
        List<int> lst = new List<int>();
        var jobs = str.Split('|');
        foreach(var job in jobs)
        {
            if (string.IsNullOrEmpty(job))
                continue;
            lst.Add(int.Parse(job));
        }
        return lst;
    }
    static public string GetJobDescription(string str)
    {
        string jobs = string.Empty;
        List<int> lst = GetJobs(str);

        if(1 == lst.Count && -1 == lst[0])
        {
            return DataController.Instance().GetLocalization(40);
        }

        foreach (var job in lst)
        {
            jobs += (((Jobs)job).ToString() + " ");
        }
        return jobs;
    }

    // 计算战力时单位属性点权值.
    private static readonly int _calcAttrWeight = 10;
    // 计算战力时单位技能等级权值.
    private static readonly int _calcSkillLvWeight = 20;
    static public int CalcPower(GKDataBase data)
    {
        int power = 0;

        int str = data.GetAttribute((int)EObjectAttr.TotalStrength).ValInt * _calcAttrWeight;
        int agi = data.GetAttribute((int)EObjectAttr.TotalAgility).ValInt * _calcAttrWeight;
        int intell = data.GetAttribute((int)EObjectAttr.TotalIntelligence).ValInt * _calcAttrWeight;
        int skill = 0;
        foreach (var s in data.GetAttributeList((int)EObjectAttr.Unit_Skills).ValInt)
        {
            if (-1 == s)
                continue;

            var skillData = DataController.Data.GetSkillData(s);
            if (null == skillData)
                continue;

            skill += ((skillData.key % 100) * _calcSkillLvWeight);
        }

        int equip = 0;
        foreach (var e in data.GetAttributeList((int)EObjectAttr.Unit_Equipments).ValInt)
        {
            if (-1 == e)
                continue;

            var equipData = DataController.Data.GetEquipmentData(e);
            if (null == equipData)
                continue;

            // 属性计算并入总属性计算中. 不重复计算.
            //equip += equipData.strength * _calcAttrWeight;
            //equip += equipData.agility * _calcAttrWeight;
            //equip += equipData.intelligence * _calcAttrWeight;

            if(-1 != equipData.skillEffectA)
            {
                equip += ((equipData.skillEffectA % 100) * _calcSkillLvWeight);
            }

            if (-1 != equipData.skillEffectB)
            {
                equip += ((equipData.skillEffectB % 100) * _calcSkillLvWeight);
            }

            if (-1 != equipData.skillEffectC)
            {
                equip += ((equipData.skillEffectC % 100) * _calcSkillLvWeight);
            }
        }

        power = str + agi + intell + skill + equip;

        //Debug.Log(string.Format("Calc power. Powe: {0}, Str: {1}, Agi: {2}, Int: {3}, Skill: {4}, Equip: {5}", power, str, agi, intell, skill, equip));

        return power;
    }

    public void Init()
    {
        dataBase.GetAttributeList((int)EObjectAttr.Unit_Skills).OnAttrbutChangedEvent += OnSkillsChanged;
        dataBase.GetAttributeList((int)EObjectAttr.Unit_Equipments).OnAttrbutChangedEvent += OnEquipmentsChanged;

        dataBase.GetAttribute((int)EObjectAttr.KillCount).OnAttrbutChangedEvent += OnAttrChanged;
        dataBase.GetAttribute((int)EObjectAttr.DeathCount).OnAttrbutChangedEvent += OnAttrChanged;
        dataBase.GetAttribute((int)EObjectAttr.FightCount).OnAttrbutChangedEvent += OnAttrChanged;
        dataBase.GetAttribute((int)EObjectAttr.UsedSkillPoint).OnAttrbutChangedEvent += OnAttrChanged;
    }

    // 提升玩家卡片技能经验.
    public void AddSkillExp()
    {
        int curSkillExp = dataBase.GetAttribute((int)EObjectAttr.SkillExp).ValInt;
        curSkillExp += 10;
        int max = ConfigController.GetMaxSkillExp(dataBase.GetAttribute((int)EObjectAttr.SkillLevel).ValInt);
        // 升级.
        if(curSkillExp >= max)
        {
            int lv = dataBase.GetAttribute((int)EObjectAttr.SkillLevel).ValInt + 1;
            curSkillExp = curSkillExp - max;
            dataBase.SetAttribute((int)EObjectAttr.SkillExp, curSkillExp, false);
            dataBase.SetAttribute((int)EObjectAttr.SkillLevel, lv, false);
            // 设置最大经验.
            max = ConfigController.GetMaxSkillExp(lv);
            dataBase.SetAttribute((int)EObjectAttr.MaxSkillExp, max, false);
        }
        dataBase.SetAttribute((int)EObjectAttr.SkillExp, curSkillExp, true);
    }

    // 提升玩家卡片经验.
    public void AddExp()
    {
        int curExp = dataBase.GetAttribute((int)EObjectAttr.Exp).ValInt;
        curExp += 10;
        int max = ConfigController.GetMaxExp(dataBase.GetAttribute((int)EObjectAttr.Level).ValInt);
        // 升级.
        if (curExp >= max)
        {
            int lv = dataBase.GetAttribute((int)EObjectAttr.Level).ValInt + 1;
            curExp = curExp - max;
            dataBase.SetAttribute((int)EObjectAttr.Exp, curExp, false);
            dataBase.SetAttribute((int)EObjectAttr.Level, lv, false);
            // 设置最大经验.
            max = ConfigController.GetMaxExp(lv);
            dataBase.SetAttribute((int)EObjectAttr.Exp, max, false);
        }
        dataBase.SetAttribute((int)EObjectAttr.Exp, curExp, true);
    }

    // 使用技能点升级技能.
    public void UsePointToSkillLevelUp(int idx)
    {
        List<int> lst = dataBase.GetAttributeList((int)EObjectAttr.Unit_Skills).ValInt;
        if (idx < 0 || idx >= lst.Count)
        {
            Debug.LogError(string.Format("UsePointToSkillLevelUp faile. idx: {0}, lst count: {1}", idx, lst.Count));
            return;
        }
            
        lst[idx]++;
        // 技能升级.
        dataBase.SetAttributeList((int)EObjectAttr.Unit_Skills, lst, true);
        // 使用技能点.
        int val = dataBase.GetAttribute((int)EObjectAttr.UsedSkillPoint).ValInt + 1;
        dataBase.SetAttribute((int)EObjectAttr.UsedSkillPoint, val, true);
    }

    public void ResetSkillPoints()
    {
        List<int> lst = new List<int>();
        // 初始化技能等级.
        for (int i = 0; i < SkillController.Instance().GetSkillCount(dataBase.GetAttribute((int)EObjectAttr.SkillTreeID).ValInt); i++)
        {
            lst.Add(0);
        }
        dataBase.SetAttributeList((int)EObjectAttr.Unit_Skills, lst, true);
        dataBase.SetAttribute((int)EObjectAttr.UsedSkillPoint, 0, true);
    }

    // 游戏结算时数据处理.
    public void EndGameResult(out int score, out int kill)
    {
        score = dataBase.GetAttribute((int)EObjectAttr.InGameScore).ValInt;
        kill = dataBase.GetAttribute((int)EObjectAttr.InGameKillCount).ValInt;
        int totalScore = score + dataBase.GetAttribute((int)EObjectAttr.Score).ValInt;
        int totalKill = kill + dataBase.GetAttribute((int)EObjectAttr.KillCount).ValInt;
        int fight = 1 + dataBase.GetAttribute((int)EObjectAttr.FightCount).ValInt;
        int death = 1 + dataBase.GetAttribute((int)EObjectAttr.DeathCount).ValInt;
        dataBase.SetAttribute((int)EObjectAttr.Score, totalScore, true);
        dataBase.SetAttribute((int)EObjectAttr.KillCount, totalKill, true);
        dataBase.SetAttribute((int)EObjectAttr.InGameScore, 0, true);
        dataBase.SetAttribute((int)EObjectAttr.InGameKillCount, 0, true);
        dataBase.SetAttribute((int)EObjectAttr.FightCount, fight, true);
        if(1 == dataBase.GetAttribute((int)EObjectAttr.IsDead).ValInt)
            dataBase.SetAttribute((int)EObjectAttr.DeathCount, death, true);
    }

    // 卡片删除时, 释放对应卡片资源.
    public void Release()
    {
        dataBase.GetAttributeList((int)EObjectAttr.Unit_Skills).OnAttrbutChangedEvent -= OnSkillsChanged;
        dataBase.GetAttributeList((int)EObjectAttr.Unit_Equipments).OnAttrbutChangedEvent -= OnEquipmentsChanged;

        dataBase.GetAttribute((int)EObjectAttr.KillCount).OnAttrbutChangedEvent -= OnAttrChanged;
        dataBase.GetAttribute((int)EObjectAttr.DeathCount).OnAttrbutChangedEvent -= OnAttrChanged;
        dataBase.GetAttribute((int)EObjectAttr.FightCount).OnAttrbutChangedEvent -= OnAttrChanged;
        dataBase.GetAttribute((int)EObjectAttr.UsedSkillPoint).OnAttrbutChangedEvent -= OnAttrChanged;

        dataBase.ClearAllAttributeValue();
    }
    #endregion

    #region PrivateMethod
    private void OnAttrChanged(object obj, GKCommonValue attr)
    {
        //Debug.Log("OnAttrChanged");

        if (null != attr)
        {
            // 计算增值.
            int count = attr.ValInt - attr.LastValInt;
            // 某些可以重置的数据被重置, 也会触发数据变更. 忽略此类数据变更情况.
            // 类似于技能点(UsedSkillPoint)使用累加判定.
            if (count <= 0)
                return;
            // 更新成就数据.
            AchievementController.Instance().UpdateAchievementCount((EObjectAttr)attr.index, count);
        }
    }

    // 技能调整影响战力.
    private void OnSkillsChanged(object obj, GKCommonListValue attr)
    {
        //Debug.Log("OnSkillsChanged");

        int power = CalcPower(dataBase);
        dataBase.SetAttribute((int)EObjectAttr.Power, power, true);
    }

    // 装备调整影响总和属性及战力.
    private void OnEquipmentsChanged(object obj, GKCommonListValue attr)
    {
        Debug.Log("OnEquipmentsChanged");

        int str = dataBase.GetAttribute((int)EObjectAttr.TotalStrength).ValInt;
        int agi = dataBase.GetAttribute((int)EObjectAttr.TotalAgility).ValInt;
        int intell = dataBase.GetAttribute((int)EObjectAttr.TotalIntelligence).ValInt;

        // 进行综述性计算.
        foreach (var e in dataBase.GetAttributeList((int)EObjectAttr.Unit_Equipments).ValInt)
        {
            if (-1 == e)
                continue;

            var equipData = DataController.Data.GetEquipmentData(e);
            if (null == equipData)
                continue;
            
            str     +=  equipData.strength;
            agi     +=  equipData.agility;
            intell  +=  equipData.intelligence;
        }

        dataBase.SetAttribute((int)EObjectAttr.TotalStrength, str, true);
        dataBase.SetAttribute((int)EObjectAttr.TotalAgility, agi, true);
        dataBase.SetAttribute((int)EObjectAttr.TotalIntelligence, intell, true);

        int power = CalcPower(dataBase);

        dataBase.SetAttribute((int)EObjectAttr.Power, power, true);
    }
    #endregion
}

public enum Jobs
{
    Warrior = 0,    // 战士.
    Soldier,        // 武士.
    Berserker,      // 狂战士.
    Assassin,       // 刺客.
    Elementalist,   // 元素法师.
    Pastor,         // 牧师.
    Bard,           // 吟游诗人.
    Butcher,        // 屠夫.
    Archer,         // 弓箭手.
    Roer,           // 火枪手.
    Ninja,          // 忍者.
    Pirate          // 海盗.
}
