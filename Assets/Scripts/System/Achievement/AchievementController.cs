using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKBase;
using GKData;

public class AchievementController : GKSingleton<AchievementController>
{
    #region PublicField
    // 玩家成就变更.
    public System.Action OnTitleChangedEvent = null;
    public delegate void NewAchievementEvent(int newID);
    public NewAchievementEvent OnNewAchievementEvent = null;
    // 成就总数.
    static public readonly int MAX_ACHIEVEMENT_COUNT = 60;
    // 角色称号.
    public int Title
    {
        get { return _data.GetAttribute((int)EObjectAttr.Title).ValInt; }
        set
        {
            _data.SetAttribute((int)EObjectAttr.Title, value, true);
            DataController.Instance().SaveAchievement();
            if (null != OnTitleChangedEvent)
                OnTitleChangedEvent();
        }
    }
    #endregion

    #region PrivateField
    private GKDataBase _data = new GKDataBase();
    private Dictionary<int, AchievementLeListening> _listenings = new Dictionary<int, AchievementLeListening>();
    #endregion

    #region PublicMethod
    public void Init()
    {
        _listenings.Clear();
        // 获取达成成就ID链表.
        var lst = _data.GetAttributeList((int)EObjectAttr.Achievements).ValInt;
        // 初始化成就监听对象. 如果成就已完成, 跳过初始化.
        for (int i = 0; i < MAX_ACHIEVEMENT_COUNT; i++)
        {
            if (lst.Contains(i))
                continue;
            var achiData = DataController.Data.GetAchievementData(i);
            if (null == achiData)
                continue;
            _listenings.Add(i, new AchievementLeListening(i, (AchievementType)achiData.action, achiData.parameter));
        }
        SaveCreateTime();
    }

    public GKDataBase GetDataBase()
    {
        return _data;
    }
    public void SetDataBase(GKDataBase data)
    {
        _data = data;
    }

    // 获取已完成成就ID.
    public List<int> GetAchievements()
    {
        return _data.GetAttributeList((int)EObjectAttr.Achievements).ValInt;
    }

    public void SetAchievements(List<int> lst)
    {
        _data.SetAttributeList((int)EObjectAttr.Achievements, lst, false);
    }

    // 新增完成成就.
    public void AddCompleteAchievement(int id)
    {
        if(!_data.GetAttributeList((int)EObjectAttr.Achievements).ValInt.Contains(id))
        {
            // 添加新完成成就.
            var lst = _data.GetAttributeList((int)EObjectAttr.Achievements).ValInt;
            lst.Add(id);
            _data.SetAttributeList((int)EObjectAttr.Achievements, lst, true);

            // 存储成就数据.
            DataController.Instance().SaveAchievement();

            // 分发新成就事件.
            if(null != OnNewAchievementEvent)
            {
                OnNewAchievementEvent(id);
            }
        }
    }

    // 更新玩家数据总量.
    public void UpdateAchievementCount(EObjectAttr idx, int increment)
    {
        var type = TransitionEObjectAttrType(idx);
        // 无效的类型转换.
        if (EObjectAttr.BaseAttr_Start == type)
            return;
        int count = _data.GetAttribute((int)type).ValInt + increment;
        _data.SetAttribute((int)type, count, true);
        // 存储数据.
        DataController.Instance().SaveAchievement();
    }

    // 单个类型转换成累加型类型.
    public EObjectAttr TransitionEObjectAttrType(EObjectAttr type)
    {
        switch(type)
        {
            case EObjectAttr.KillCount:
                return EObjectAttr.AchiKillCount;
            case EObjectAttr.DeathCount:
                return EObjectAttr.AchiDeathCount;
            case EObjectAttr.UsedSkillPoint:
                return EObjectAttr.AchiSkillUpgrade;
            case EObjectAttr.FightCount:
                return EObjectAttr.AchiFightingCount;
            case EObjectAttr.Coins:
                return EObjectAttr.AchiCoinCost;
            case EObjectAttr.Diamond:
                return EObjectAttr.AchiDiamondCost;
            case EObjectAttr.AchiThrowCount:
                return EObjectAttr.AchiThrowCount;
            case EObjectAttr.AchiConsumeCost:
                return EObjectAttr.AchiConsumeCost;
        }
        return EObjectAttr.BaseAttr_Start;
    }

    // 对应ID成就是否完成.
    public bool IsCompleted(int id)
    {
        return _data.GetAttributeList((int)EObjectAttr.Achievements).ValInt.Contains(id);
    }

    // 清除成就数据.
    public void ClearAchievements()
    {
        _data.GetAttributeList((int)EObjectAttr.Achievements).ValInt.Clear();
    }

    // 获取成就描述.
    public string GetDescription(AchievementType type, List<int> parameters)
    {
        string content = string.Empty;
        switch(type)
        {
            case AchievementType.Accumulation:
                {
                    if (2 != parameters.Count)
                        break;
                    int curVal = _data.GetAttribute((int)(parameters[0] + EObjectAttr.PlayerAchievemt_Start)).ValInt;
                    int targetVal = parameters[1];
                    if (curVal > targetVal)
                        curVal = targetVal;
                    content = string.Format(DataController.Instance().GetLocalization(parameters[0], LocalizationSubType.AchievementDesc), curVal, targetVal);  
                }
                break;
        }

        return content;
    }

    #endregion

    #region PrivateMethod
    // 判断是否为第一次登陆, 如果为第一次登陆. 记录登陆时间.
    private void SaveCreateTime()
    {
        if (0 != _data.GetAttribute((int)EObjectAttr.CreateTime).longValue)
            return;

        _data.SetAttribute((int)EObjectAttr.CreateTime, System.DateTime.Now.Ticks, false);
        DataController.Instance().SaveAchievement();
    }
    #endregion
}

public enum AchievementType
{
    //  数字积累型成就.
    Accumulation = 0,
}