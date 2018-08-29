using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKBase;

public class LotteryController : GKSingleton<LotteryController>
{
    #region PublicField
    #endregion

    #region PrivateField
    private List<int> _list = new List<int>();
    #endregion

    #region PublicMethod
    public List<int> CreateLotteryCards(LotteryType type, int level)
    {
        _list.Clear();
        int id = 0;
        switch(type)
        {
            case LotteryType.Coin:
                for (int i = 0; i < level; i++)
                {
                    id = Random.Range(0, ConfigController.unitCount);
                    _list.Add(id);
                }
                break;
            case LotteryType.Diamond:
                for (int i = 0; i < level; i++)
                {
                    id = Random.Range(0, ConfigController.unitCount);
                    _list.Add(id);
                }
                break;
            case LotteryType.Equipment:
                for (int i = 0; i < level; i++)
                {
                    id = Random.Range(0, ConfigController.equipmentCount);
                    _list.Add(id);
                }
                break;
            case LotteryType.Consume:
                for (int i = 0; i < level; i++)
                {
                    id = Random.Range(0, ConfigController.consumeCount);
                    _list.Add(id);
                }
                break;
        }
        return _list;
    }

    // 判断玩家资源是否足够进行此抽卡. 如果足够, 返回金额.
    public  int IsEnoughResourceToLottery(LotteryType type, int level)
    {
        var data = DataController.Data.GetLotteryData(level);
        int coins = PlayerController.Instance().Coin;
        int diamond = PlayerController.Instance().Diamond;
        switch (type)
        {
            // 普通金币抽卡.
            case LotteryType.Coin:
                if (null == data)
                    return -1;
                if (coins >= data.coin)
                    return data.coin;
            return -1;
            // 普通钻石抽卡.
            case LotteryType.Diamond:
                if (null == data)
                    return -1;
                if (diamond >= data.diamond)
                    return data.diamond;
                return -1;
            // 普通装备抽卡.
            case LotteryType.Equipment:
                if (null == data)
                    return -1;
                if (diamond >= data.diamond)
                    return data.diamond;
                return -1;
            // 普通消耗品抽卡.
            case LotteryType.Consume:
                if (null == data)
                    return -1;
                if (diamond >= data.diamond)
                    return data.diamond;
                return -1;
        }
        return -1;
    }

    public bool Pay(LotteryType type, int value)
    {
        int money = 0;
        switch(type)
        {
            case LotteryType.Coin:
                money = PlayerController.Instance().Coin;
                if (money < value)
                    return false;
                PlayerController.Instance().Coin -= value;
                return true;
            case LotteryType.Diamond:
                money = PlayerController.Instance().Diamond;
                if (money < value)
                    return false;
                PlayerController.Instance().Diamond -= value;
                return true;
            case LotteryType.Equipment:
                money = PlayerController.Instance().Diamond;
                if (money < value)
                    return false;
                PlayerController.Instance().Diamond -= value;
                return true;
            case LotteryType.Consume:
                money = PlayerController.Instance().Diamond;
                if (money < value)
                    return false;
                PlayerController.Instance().Diamond -= value;
                return true;
        }
        return false;
    }

    // 获取抽卡结果, 并更新数据.
    public void Get(LotteryType type)
    {
        switch(type)
        {
            case LotteryType.Coin:
            case LotteryType.Diamond:
                PlayerController.Instance().NewCards(_list);
                break;
            case LotteryType.Equipment:
                foreach(var id in _list)
                {
                    PlayerController.Instance().NewItem(-1, (int)ItemType.Equipment, id, 1, true);
                }
                break;
            case LotteryType.Consume:
                foreach (var id in _list)
                {
                    PlayerController.Instance().NewItem(-1, (int)ItemType.Consume, id, 1, true);
                }
                break;
        }
        _list.Clear();
    }

    #endregion

    #region PrivateMethod
    #endregion
}

public enum LotteryType
{
    // 金币抽卡.
    Coin,
    // 钻石抽卡.
    Diamond,
    // 装备抽卡.
    Equipment,
    // 消耗品抽卡.
    Consume,

}