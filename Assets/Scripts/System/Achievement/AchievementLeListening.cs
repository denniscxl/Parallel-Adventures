using System.Collections;
using System.Collections.Generic;
using GKData;
using UnityEngine;

/// <summary>
/// 成就监听类.
/// 主要功能为: 监听尚未完成成就达成逻辑及数据.
/// 当达成时, 通知AchievementController 修改成就达成状态.
/// </summary>
public class AchievementLeListening 
{
    #region PublicField
    #endregion

    #region PrivateField
    private int _id;
    private AchievementType _type = AchievementType.Accumulation;
    private List<int> _lst;
    // 子类型索引.
    private int _subType = 0;
    // 目标值.
    private int _targetVal = 0;
    private GKDataBase _data;
    #endregion

    #region PublicMethod
    public AchievementLeListening(int id, AchievementType type, List<int> lst) 
    {
        _id = id;
        _type = type;
        _lst = lst;

        switch (_type)
        {
            //  数字积累型成就.
            case AchievementType.Accumulation:
                if (2 == _lst.Count)
                {
                    _subType = _lst[0];
                    _targetVal = _lst[1];
                }
                else
                {
                    Debug.LogError(string.Format("AchievementLeListening Init faile. _lst.Count: {0}", _lst.Count));
                }
                break;
        }
        _data = AchievementController.Instance().GetDataBase();
        StartListening();
    }
    #endregion

    #region PrivateMethod
    private void StartListening()
    {
        switch(_type)
        {
            //  数字积累型成就.
            case AchievementType.Accumulation:
                _data.GetAttribute((int)(EObjectAttr.PlayerAchievemt_Start + (int)_subType)).OnAttrbutChangedEvent += OnAccumulationChanged;
                break;
        }
    }

    private void OnAccumulationChanged(object obj, GKCommonValue attr)
    {
        //Debug.Log(string.Format("OnAccumulationChanged current: {0}, target: {1}", attr.ValInt, _targetVal));
        if(attr.ValInt >= _targetVal)
        {
            Completed();
        }
    }

    private void Completed()
    {
        switch (_type)
        {
            //  数字积累型成就.
            case AchievementType.Accumulation:
                _data.GetAttribute((int)(EObjectAttr.PlayerAchievemt_Start + (int)_subType)).OnAttrbutChangedEvent -= OnAccumulationChanged;
                break;
        }

        AchievementController.Instance().AddCompleteAchievement(_id);
    }
    #endregion
}
