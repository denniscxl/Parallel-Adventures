using System.Collections.Generic;
using UnityEngine;
using GKBase;
using GKData;
using GKUI;

public class PlayerController : GKSingleton<PlayerController>
{
    #region PublicField
    // 新玩家角色进场.
    public System.Action OnFormationChangedEvent = null;
    // 获得物品.
    public System.Action OnGetNewItemEvent = null;
    // 背包升级.
    public System.Action OnUpGradeInventoryEvent = null;
    // 语言切换.
    public System.Action OnLanguageChangedEvent = null;

    public int Coin
    {
        get { return _data.GetAttribute((int)EObjectAttr.Coins).ValInt; }
        set
        {
            _data.SetAttribute((int)EObjectAttr.Coins, value, true);
            DataController.Instance().SavePlayerData();
        }
    }
    public int Diamond
    {
        get { return _data.GetAttribute((int)EObjectAttr.Diamond).ValInt; }
        set
        {
            _data.SetAttribute((int)EObjectAttr.Diamond, value, true);
            DataController.Instance().SavePlayerData();
        }
    }
    public int InventoryLevel
    {
        get { return _data.GetAttribute((int)EObjectAttr.InventoryLevel).ValInt; }
        set
        {
            _data.SetAttribute((int)EObjectAttr.InventoryLevel, value, true);
            DataController.Instance().SavePlayerData();
        }
    }

    public int Language
    {
        get { return _data.GetAttribute((int)EObjectAttr.Language).ValInt; }
        set
        {
            _data.SetAttribute((int)EObjectAttr.Language, value, true);
            DataController.Instance().SavePlayerData();
            if (null != OnLanguageChangedEvent)
                OnLanguageChangedEvent();
        }
    }

    public CampType Camp
    {
        get { return (CampType)_data.GetAttribute((int)EObjectAttr.Camp).ValInt; }
        set
        {
            _data.SetAttribute((int)EObjectAttr.Camp, (int)value, true);
        }
    }

    // 基础出阵卡牌.
    static public readonly int MAX_FIGHT_COUNT = 20;
    // 基础背包格数.
    static public readonly int MAX_INVENTORY_COUNT = 60;
    // 最大背包等级.
    static public readonly int MAX_INVENTORY_Level = 10;
    #endregion

    #region PrivateField
    private GKDataBase _data = new GKDataBase();
    // 玩家卡片.
    private Dictionary<int, Card> _cards = new Dictionary<int, Card>();
    // 出阵卡片索引.
    private List<int> _fightCardLst = new List<int>();
    // 出阵卡牌字典.
    private Dictionary<int, Card> _fightCards = new Dictionary<int, Card>();
    // 背包数据.
    private Dictionary<int, Item> _inventory = new Dictionary<int, Item>();
    #endregion

    #region PublicMethod
    public void Init()
    {
        _data.GetAttribute((int)EObjectAttr.Coins).OnAttrbutChangedEvent += OnAttrChanged;
        _data.GetAttribute((int)EObjectAttr.Diamond).OnAttrbutChangedEvent += OnAttrChanged;
    }

    public GKDataBase GetDataBase()
    {
        return _data;
    }
    public void SetDataBase(GKDataBase data)
    {
        _data = data;
    }
    #region Card
    // 获取玩家拥有卡片.
    public Dictionary<int, Card> GetPlayerCards()
    {
        return _cards;
    }

    public Card GetPlayerCard(int ID)
    {
        if (_cards.ContainsKey(ID))
            return _cards[ID];
        return null;
    }

    // 添加卡片.
    public void NewCards(List<int> cards)
    {
        foreach (var id in cards)
        {
            if (_cards.ContainsKey(id))
            {
                // 如果玩家已经拥有此卡. 提升卡片技能经验.
                _cards[id].AddSkillExp();
            }
            else
            {
                // 如果玩家不拥有此卡, 添加此卡到卡库中.
                _cards[id] = new Card(id);
            }
        }
        DataController.Instance().SaveCards();
    }

    // 初始化时添加卡片.
    public void AddCard(Card c)
    {
        if (null == c)
            return;
        _cards[c.dataBase.GetAttribute((int)EObjectAttr.ID).ValInt] = c;
    }

    // 获取玩家拥有卡片信息.
    public GKDataBase GetCardDetaileFromPlayer(int id)
    {
        if (!_cards.ContainsKey(id))
            return null;
        return _cards[id].dataBase;
    }

    // 每局游戏开始时初始化
    public void InitFightCards()
    {
        _fightCards.Clear();
        foreach (var id in _fightCardLst)
        {
            if (_cards.ContainsKey(id))
                _fightCards[id] = _cards[id];
        }
    }

    // 设置玩家卡片上阵状态.
    public void SetCardFighting(int id, bool bFighting, bool save = true)
    {
        if (!_cards.ContainsKey(id))
            return;
        if (bFighting)
        {
            // 上阵.
            if (_fightCardLst.Contains(id))
                return;
            _fightCardLst.Add(id);
        }
        else
        {
            // 下阵.
            if (!_fightCardLst.Contains(id))
                return;
            _fightCardLst.Remove(id);
        }

        if (null != OnFormationChangedEvent)
            OnFormationChangedEvent();

        if (save)
            DataController.Instance().SaveCardFightingList();
    }

    // 获取卡片是否上阵状态.
    public bool GetCardFightingState(int id)
    {
        return _fightCardLst.Contains(id);
    }

    // 获取当前上阵卡片数量.
    public int GetFightingCount()
    {
        return _fightCardLst.Count;
    }

    // 获取战斗卡片列表, 不同于获取战斗卡片. 
    // 不需要游戏开始时初始化卡片实例.
    public List<int> GetFightCardsList()
    {
        return _fightCardLst;
    }

    // 获取玩家出战卡片.
    public Dictionary<int, Card> GetFightCards()
    {
        return _fightCards;
    }

    // 获取玩家出战卡片信息.
    public GKDataBase GetCardDetaileFromFight(int id)
    {
        if (!_fightCards.ContainsKey(id))
            return null;
        return _fightCards[id].dataBase;
    }

    // 修改卡牌装备状态.
    public int ModifyCardEquipmentState(bool bEquip, int unitID, GameData.EquipmentData data, int solt)
    {
        int lastEquipID = -1;
        var unitData = PlayerController.Instance().GetCardDetaileFromPlayer(unitID);
        if (null == unitData)
        {
            Debug.LogError(string.Format("ModifyCardEquipmentState faile. Cna't find card data. _unitID: {0}", unitID));
            return (int)ErrorCodeType.CardDataMissing;
        }

        // 修改卡牌装备状态.
        List<int> soltList = unitData.GetAttributeList((int)EObjectAttr.Unit_Equipments).ValInt;
        if(bEquip)
        {
            // 判断装备对象于角色职业是否相同.
            var lst = Card.GetJobs(data.job);
            int job = unitData.GetAttribute((int)EObjectAttr.Job).ValInt;

            if((1 == lst.Count && -1 == lst[0]) || lst.Contains(job))
            {
                lastEquipID = soltList[data.part];
                soltList[data.part] = data.id;
                ReduceItem(solt, false);
                // 如果为装备替换状态时.
                if (-1 != lastEquipID)
                {
                    NewItem(solt, (int)ItemType.Equipment, lastEquipID, 1, false);
                }
            }
            else
            {
                return (int)ErrorCodeType.JobMismatching;
            }
        }
        else
        {
            NewItem(-1, (int)ItemType.Equipment, soltList[data.part], 1, false);
            soltList[data.part] = -1;
        }
        unitData.SetAttributeList((int)EObjectAttr.Unit_Equipments, soltList, true);

        SortInventory();

        return 0;
    }
    #endregion

    #region Inventory
    // 获取玩家背包.
    public Dictionary<int, Item> GetInventory()
    {
        return _inventory;
    }

    // 添加物品.
    public void NewItem(int solt, int type, int id, int count, bool DoEvent = true)
    {
        int emptySolt = 0;
        if(-1 != solt)
        {
            if (_inventory.ContainsKey(solt) && (int)_inventory[solt].type == type && _inventory[solt].id == id )
            {
                // 如果玩家已经拥有此类物品, 增加物品数量.
                _inventory[solt].count += count;
            }
            else
            {
                // 如果玩家不拥有此此类物品, 添加物品到背包中.
                emptySolt = GetEmptySoltFromInventory();
                if(-1 == emptySolt)
                {
                    Debug.LogWarning(string.Format("Inventory full. New item type:{0}, id: {1}, count: {2}", type, id, count));
                    return;
                }
                else
                {
                    _inventory[solt] = new Item(solt, type, id, count);
                }
               
            }
        }
        else 
        {
            // 检测背包中是否存在此物品.
            int ret = IsExistItem(type, id);
            // 不存在.
            if(-1 == ret)
            {
                emptySolt = GetEmptySoltFromInventory();
                if (-1 == emptySolt)
                {
                    Debug.LogWarning(string.Format("Inventory full. New item type:{0}, id: {1}, count: {2}", type, id, count));
                    return;
                }
                else
                {
                    _inventory[emptySolt] = new Item(solt, type, id, count);
                }
            }
            else
            {
                // 如果玩家已经拥有此类物品, 增加物品数量.
                _inventory[ret].count += count;
            }
        }

        if(DoEvent)
        {
            UIMessageBox.ShowUIResMessage(string.Format(DataController.Instance().GetLocalization(84), ConfigController.Instance().GetItemNameByType(type, id)), type, id);
        }
            
        if (DoEvent && null != OnGetNewItemEvent)
            OnGetNewItemEvent();
        
        DataController.Instance().SaveInventory();
    }

    // 获取背包槽数据.
    public Item GetInventorySolt(int solt)
    {
        if (!_inventory.ContainsKey(solt))
            return null;

        return _inventory[solt];
    }

    // 获得空槽索引.
    private int GetEmptySoltFromInventory()
    {
        if (_inventory.Count >= GetInventoryCapacity())
            return -1;
        return _inventory.Count;
    }

    private int IsExistItem(int type, int id)
    {
        foreach(var item in _inventory)
        {
            if(type == (int)item.Value.type && id == item.Value.id && ItemType.Equipment != item.Value.type)
            {
                return item.Key;
            }
        }
        return -1;
    }

    public void ClearInventory()
    {
        _inventory.Clear();
    }

    // 背包升级.
    public int UpgradeInventory()
    {
        // 判断是否为最高等级.
        if (InventoryLevel >= MAX_INVENTORY_Level)
            return (int)ErrorCodeType.MaxLevel;
        
        // 获取升级数据.
        var data = DataController.Data.GetInventoryUpgradeData(InventoryLevel);
        if (null == data)
            return (int)ErrorCodeType.EquipmentDataMissing;

        // 检测玩家是否足够资源支付.
        if (Diamond < data.diamond)
            return (int)ErrorCodeType.DiamondNotEnough;;

        Diamond -= data.diamond;
        InventoryLevel += 1;

        if (null != OnUpGradeInventoryEvent)
            OnUpGradeInventoryEvent();

        return 0;
    }

    // 获取玩家当前背包容量.
    public int GetInventoryCapacity()
    {
        return MAX_INVENTORY_COUNT + InventoryLevel * 20;
    }

    // 道具数量减少.
    // 可能为使用, 装备, 丢弃.
    public bool ReduceItem(int solt, bool bAll = false, int count = 1)
    {
        if (!_inventory.ContainsKey(solt))
            return false;

        if (_inventory[solt].count < count)
            return false;
        
        if (bAll)
        {
            _inventory.Remove(solt);
            return true;
        }
        _inventory[solt].count -= count;

        if(0 == _inventory[solt].count)
        {
            _inventory.Remove(solt);
        }
        return true;
    }

    // 背包内排序.
    public void SortInventory()
    {
        List<Item> lst = new List<Item>();
        foreach(var item in _inventory.Values)
        {
            lst.Add(item);
        }
        _inventory.Clear();
        for (int i = 0; i < lst.Count; i++)
        {
            _inventory[i] = lst[i];
            _inventory[i].solt = i;
        }
    }
    #endregion

    #endregion

    #region PrivateMethod
    // 消耗量统计.
    private void OnAttrChanged(object obj, GKCommonValue attr)
    {
        //Debug.Log("PlayerController OnAttrChanged");

        if (null != attr)
        {
            // 计算增值.
            int count = attr.ValInt - attr.LastValInt;
            if (count >= 0)
                return;
            // 更新成就数据.
            AchievementController.Instance().UpdateAchievementCount((EObjectAttr)attr.index, -count);
        }
    }
    #endregion
}

// 装备部位.
public enum EquipmentPart
{
    Weapon = 0,
    Head,
    Body,
    Leg,
    Hand,
    Foot,
    Jewelry,
    Wing,
    Count
}
