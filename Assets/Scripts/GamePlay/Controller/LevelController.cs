using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using GKBase;
using GKMap;
using GKRole;
using GKData;
using GKFOW;

public class LevelController : MonoBehaviour
{
    #region PublicField
    // 新玩家角色进场.
    public System.Action OnNewPlayerUnitEvent = null;
    // 角色选择变更.
    public System.Action OnSelectUnitChangedEvent = null;
    // 地块选择变更.
    public System.Action OnSelectTileChangedEvent = null;

    #endregion

    #region PrivateField
    // 摄像机鸟瞰模式根节点.
    [SerializeField]
    private GameObject _birdsEyeRoot;
    // 角色根节点.
    [SerializeField]
    private GameObject _unitRoot;
    // AI管理器节点对象.
    [SerializeField]
    private GKCommanderController _commanderController;
    private static LevelController _instance = null;
    // 场景内所有对象字典.
    private Dictionary<long, GameObject> _objects = new Dictionary<long, GameObject>();
    // 阵营对象字典. Key 阵营, Value - key 角色ID val 角色GUID.
    private Dictionary<CampType, Dictionary<int, long>> _camps = new Dictionary<CampType, Dictionary<int, long>>();
    // 剩余出阵卡库.
    private Dictionary<CampType, List<int>> _remainderCard = new Dictionary<CampType, List<int>>();
    private List<CampType> _campTypeList = new List<CampType>();
    // 关卡数据.
    private Dictionary<CampType, GKDataBase> _dataList = new Dictionary<CampType, GKDataBase>();
    // 据点归属字典.
    private Dictionary<CampType, List<int>> _villageCampDict = new Dictionary<CampType, List<int>>();
    // 玩家当前光标
    private GameObject _cursor = null;
    // 当前选择的角色GUID. 小于0, 未还未选择.
    private long _curSelectUnit = -1;
    private GKUnit _curSelectUnitObj = null;
    // 当前选择的地块信息.
    private GKTerrainGrid _curSelectTile = null;
    // 当前命令选择目标对象. 用于指定于作用者. 例如攻击对象. 区别攻击者(_curSelectUnitObj).
    private GKUnit _curSelectTargetUnit = null;
    // 当前命令类型.
    private CommandType _curCommandType = CommandType.NoCommand;
    // 地块高亮接口.
    private int _lastHighLightMask = 0;
    #endregion

    #region PublicMethod
    public static LevelController Instance() { return _instance; }

    // 获取玩家数据.
    public GKDataBase GetDataBase(CampType idx)
    {
        if (!_dataList.ContainsKey(idx))
        {
            Debug.LogError(string.Format("GetDataBase fiale. CampType: {0}",idx));
            return null;
        }
        return _dataList[idx];
    }

    // 通过GUID获取场景内对象.
    public GameObject GetTargetByID(long id)
    {
        if (!_objects.ContainsKey(id))
        {
            Debug.LogWarning(string.Format("GetTargetByID fiale. _objects count: {0}", _objects.Count));
            return null;
        }
        return _objects[id];
    }

    // 获取阵营数据.
    public Dictionary<int, long> GetCampData(CampType camp)
    {
        if (!_camps.ContainsKey(camp))
            return null;
        return _camps[camp];
    }

    // 获取阵营总数.
    public int GetCampCount()
    {
        return _camps.Count;
    }

    // 选择角色.
    public void SelectUnit(long guid)
    {
        if(guid != _curSelectUnit)
        {
            // 切换角色后重置命令及其效果. 
            SetCommand(CommandType.NoCommand);

            _curSelectUnit = guid;
            _curSelectUnitObj = GetSelectUnitObject();
            if (null != OnSelectUnitChangedEvent)
                OnSelectUnitChangedEvent();
        }
        // 摄像机跟随.
        if(_objects.ContainsKey(guid))
        {
            GameObject go = _objects[guid];
            CameraController.Instance().SetFocus(go.transform);
            CameraController.Instance().SetTargetPos(go.transform.position.x, go.transform.position.z);
            _curSelectTile = go.GetComponent<GKUnit>().Grid;
            SetCursor();
        }
    }

    // 获取当前选择角色的ID.
    public int GetSelectUnitID()
    {
        if (-1 == _curSelectUnit || !_objects.ContainsKey(_curSelectUnit))
            return -1;
        var obj = _objects[_curSelectUnit];
        if (null == obj)
            return -1;
        GKUnit unit = GK.GetOrAddComponent<GKUnit>(obj);
        GKPlayer player = GK.GetOrAddComponent<GKPlayer>(obj);
        return unit.GetAttribute(EObjectAttr.ID).ValInt;
    }

    // 获取当前选择角色GUID.
    public long GetCurSelectUnitGUID()
    {
        return _curSelectUnit;
    }

    public GKUnit GetCurSelectUnit()
    {
        return _curSelectUnitObj;
    }

    // 获取当前选择角色对象.
    public GKUnit GetSelectUnitObject()
    {
        if(-1 == _curSelectUnit || !_objects.ContainsKey(_curSelectUnit))
            return null;
        var obj = _objects[_curSelectUnit];
        if (null == obj)
            return null;
        return GK.GetOrAddComponent<GKUnit>(obj);
    }

    // 获取当前选择目标角色.
    // 通过命令点选赋值.
    public GKUnit GetCurSelectTargetUnit()
    {
        return _curSelectTargetUnit;
    }

    // 创建据点.
    public GKVillage CreateVillage(int id)
    {
        GameObject go = GK.TryLoadGameObject("Prefabs/Building/Village");
        if (null == go)
            return null;
        var village = GK.GetOrAddComponent<GKVillage>(go);
        if (null == village)
            return null;
        village.OnNew(new GKDataBase());
        village.SetAttribute(EObjectAttr.TileID, id, false);
        village.SetAttribute(EObjectAttr.Level, 1, false);
        village.SetAttribute(EObjectAttr.Name, GetVillageName(), false);

        bool bInit = false;
        // 判断当前阵营是否不存在据点, 如果不存在设置归属阵营.
        foreach (var camp in _campTypeList)
        {
            if (_villageCampDict.ContainsKey(camp) && 0 == _villageCampDict[camp].Count)
            {
                _villageCampDict[camp].Add(id);
                village.ChangeCamp(camp, false);
                bInit = true;
                break;
            }
        }

        // 初始化为中立单位.
        if(!bInit)
        {
            _villageCampDict[CampType.Yellow].Add(id);
        }

        return village;
    }

    public void ChangeVillageCamp(int id, CampType lastCamp, CampType curCamp)
    {
        if (!_villageCampDict.ContainsKey(lastCamp) || !_villageCampDict.ContainsKey(lastCamp))
        {
            Debug.LogError("ChangeVillageCamp faile.");
            return;
        }
        _villageCampDict[lastCamp].Remove(id);
        _villageCampDict[curCamp].Add(id);
    }

    // 创建资源点.
    public GKFood CreateFood(int id)
    {
        GameObject go = GK.TryLoadGameObject("Prefabs/Building/Food");
        if (null == go)
            return null;
        var food = GK.GetOrAddComponent<GKFood>(go);
        if (null == food)
            return null;
        food.OnNew(new GKDataBase());
        return food;
    }

    // 创建角色.
    public GKUnit CreateUnit(CampType camp, GKDataBase data, int tileID)
    {
        //Debug.LogWarning(string.Format("CreateUnit. camp: {0}, id: {1}", camp, id));
        int id = data.GetAttribute((int)EObjectAttr.ID).ValInt;
        // 当前游戏中不存在此阵营角色或者阵营存在但不存在此角色. 创建阵营并添加角色.
        if(!_camps.ContainsKey(camp) || !_camps[camp].ContainsKey(id))
        {
            // 创建角色.
            var unit = SpawnUnit(camp, id, data);
            if(null == unit)
            {
                Debug.LogWarning(string.Format("CreateUnit fiale. SpawnUnit is null. camp: {0}, id: {1}", camp, id));
                return null;
            }

            // 设置角色阵营.
            unit.SetAttribute(EObjectAttr.Camp, (int)camp, false);

            // 判断当前场景内是否已经存在此ID.
            while(_objects.ContainsKey(unit.InstanceID))
            {
                unit.ReSetGUID();
                Debug.LogWarning(string.Format("CreateUnit, But guid exsit. guid: {0}", unit.InstanceID));
            }
            // 添加到对象池中.
            _objects[unit.InstanceID] = unit.gameObject;
            // 添加到阵营池中.
            Dictionary<int, long> newCamp = null;
            if(!_camps.ContainsKey(camp))
            {
                newCamp = new Dictionary<int, long>();
            }
            else
            {
                newCamp = _camps[camp];
            }
            newCamp.Add(id, unit.InstanceID);
            _camps[camp] = newCamp;

            // 默认创建角色位于地块的中心.
            GameObject node = GKMapManager.Instance().GetGridNode(tileID, (int)TileNodeType.Center);
            if (null != unit && null != node)
            {
                // 角色归于场景角色根节点上.
                GK.SetParent(unit.gameObject, _unitRoot, false);
                unit.myTransform.position = node.transform.position;
                unit.Grid = GKMapManager.Instance().GetGridByKey(tileID);
            }

            // 玩家角色进场消息分发.
            if (PlayerController.Instance().Camp == camp)
            {
                // 设置摄像机模式.
                CameraController.Instance().ChangeState(MachineStateID.Follow);
                // 先设置玩家当前选择角色, 再分发消息.
                // 可使程式在初始化当前角色时明确当前焦点角色.
                SelectUnit(unit.InstanceID);
                if (null != OnNewPlayerUnitEvent)
                    OnNewPlayerUnitEvent();
            }


            return unit;
        }
        else
        {
            Debug.LogWarning(string.Format("CreateUnit fiale. Already exist. camp: {0}, id: {1}", camp, id));
        }
        return null;
    }

    public bool PlayerExistUnitID(int id)
    {
        CampType camp = PlayerController.Instance().Camp;
        if (_camps.ContainsKey(camp) && _camps[camp].ContainsKey(id))
            return true;
        return false;
    }

    // 初始化各个阵营卡牌.
    public void InitCampCard()    
    {
        
        _remainderCard.Clear();

        foreach(var camp in _campTypeList)
        {
            List<int> lst = new List<int>();
            // 玩家阵营.
            if(camp == PlayerController.Instance().Camp)
            {
                PlayerController.Instance().InitFightCards();
                foreach (var card in PlayerController.Instance().GetFightCards().Keys)
                {
                    lst.Add(card);
                }
                _remainderCard.Add(camp, lst);
            }
            else
            {
                // 暂时未人机, 非玩家数据从配置中获取.
                // 之后增加非本地玩家数据加载.
                var enemyConfig = DataController.Data.GetEnemyConfigData(0);
                if(null != enemyConfig)
                {
                    foreach (var unit in enemyConfig.units)
                    {
                        var enemy = DataController.Data.GetEnemyData(unit);
                        if(null != enemy)
                            lst.Add(enemy.unit);
                    }
                    _remainderCard.Add(camp, lst);
                }
                else
                {
                    Debug.LogError(string.Format("InitCampCard faile. Cant get enemy config data. Index: {0}", 0));
                }
            }
        }
    }

    // 获得剩余未出战卡片.
    public List<int> GetRemainderCards(CampType camp)
    {
        if (!_remainderCard.ContainsKey(camp))
            return null;

        return _remainderCard[camp];
    }

    // 使用卡片.
    public GKDataBase UseCard(CampType camp, int id, List<int> skills = null, List<int> equips = null)
    {
        if(_remainderCard.ContainsKey(camp) && _remainderCard[camp].Contains(id))
        {
            // 资源扣除.
            var data = DataController.Data.GetUnitData(id);
            if(null != data)
            {
                SetFood(camp, _dataList[camp].GetAttribute((int)EObjectAttr.Food).ValInt - data.costFood);
                SetBelief(camp, _dataList[camp].GetAttribute((int)EObjectAttr.Belief).ValInt - data.costBelief);
            }
            _remainderCard[camp].Remove(id);
            if(camp == PlayerController.Instance().Camp)
                return PlayerController.Instance().GetCardDetaileFromFight(id);
            else
            {
                return ConfigController.Instance().GetNewCardData(id, skills, equips);
            }
        }
        return null;
    }

    // 设置获取信仰值.
    public void SetBelief(CampType idx, int val)
    {
        var data =  GetDataBase(idx);
        if(null != data)
        {
            data.SetAttribute((int)EObjectAttr.Belief, val, true);
        }
        else
        {
            Debug.LogWarning(string.Format("SetBelief faile. Can't find target data. idx:{0}, val:{1}", idx, val));
        }
    }
    public int GetBelief(CampType idx)
    {
        var data = GetDataBase(idx);
        if (null != data)
        {
            return data.GetAttribute((int)EObjectAttr.Belief).ValInt;
        }
        Debug.LogWarning(string.Format("GetBelief faile. Can't find target data. idx:{0}", idx));
        return -1;
    }

    // 设置获取食物值.
    public void SetFood(CampType idx, int val)
    {
        var data = GetDataBase(idx);
        if (null != data)
        {
            data.SetAttribute((int)EObjectAttr.Food, val, true);
        }
        else
        {
            Debug.LogWarning(string.Format("SetFood faile. Can't find target data. idx:{0}, val:{1}", idx, val));
        }
    }
    public int GetFood(CampType idx)
    {
        var data = GetDataBase(idx);
        if (null != data)
        {
            return data.GetAttribute((int)EObjectAttr.Food).ValInt;
        }
        Debug.LogWarning(string.Format("GetFood faile. Can't find target data. idx:{0}", idx));
        return -1;
    }

    // 获取关卡完结奖励.
    public List<Item> GetEndGameReward()
    {
        List<Item> lst = new List<Item>();

        int type = 0;
        int id = 0;
        for (int i = 0; i < 3; i++)
        {
            type = Random.Range((int)ItemType.Equipment, (int)ItemType.Consume + 1);
            switch (type)
            {
                case (int)ItemType.Equipment:
                    id = Random.Range(0, ConfigController.equipmentCount);
                    break;
                case (int)ItemType.Consume:
                    id = Random.Range(0, ConfigController.consumeCount);
                    break;
            }
            lst.Add(new Item(-1, type, id, 1));
        }

        return lst;
    }

    // 初始化游戏阵营链表.
    public void InitCampList(List<CampType> lst)
    {
        _campTypeList.Clear();
        _campTypeList = lst;
    }

    public void EndGame()
    {
        ReleaseData();
        MyGame.IsBattle = false;
    }

    // 返回鸟瞰模式根节点.
    public GameObject GetBridsEysRoot()
    {
        return _birdsEyeRoot;
    }

    // 返回角色根节点.
    public GameObject GetUnitRoot()
    {
        return _unitRoot;
    }

    // 返回当前选择地块数据.
    public GKTerrainGrid GetCurSelectTitle()
    {
        return _curSelectTile;
    }

    // 设置光标.
    public void SetCursor()
    {
        if(null == _curSelectTile)
        {
            _cursor.SetActive(false);
            Debug.LogError(string.Format("Set cursor faile. target is null."));
            return;
        }
        _cursor.SetActive(true);
        GK.SetParent(_cursor, _curSelectTile.gameObject, false);
    }

    // 隐藏所有命令效果.
    public void HideAllCommandEffect(GKUnit unit)
    {
        HighLightTerrain(0);
        if (null != unit)
        {
            unit.ShowRange(false);
        }
    }

    public void SetCommand(CommandType type)
    {
        _curCommandType = type;
        GKUnit unit = GetCurSelectUnit();
        HideAllCommandEffect(unit);

        // 获取摄像机全局状态机.
        var machine = CameraController.Instance().GetStateMachine();
        GKCameraOverall state = machine._GetStateById(MachineStateID.Overall) as GKCameraOverall;
        // High Lisht.
        switch(type)
        {
            case CommandType.NoCommand:
                state.ChangePos(Vector3.zero);
                break;
            case CommandType.Move:
                if (null == unit)
                    return;
                state.ChangePos(Vector3.zero);
                HighLightTerrain(unit.GetAttribute(EObjectAttr.LayerMask).ValInt);
                break;
            case CommandType.Attack:
                if (null == unit)
                    return;
                state.ChangePos(unit.myTransform.position, 30);
                unit.ShowRange(true, unit.GetAttribute(EObjectAttr.AttackRange).ValInt);
                break;
            case CommandType.Defense:
                unit.Defense();
                break;
            case CommandType.Village:
                state.ChangePos(Vector3.zero);
                HighLightTerrain(0);
                HighLightTerrain((int)MoveType.Village);
                break;
            case CommandType.Food:
                state.ChangePos(Vector3.zero);
                HighLightTerrain(0);
                HighLightTerrain((int)MoveType.Food);
                break;
        }
    }

    public CommandType GetCommand()
    {
        return _curCommandType;
    }

    // 获取阵营索引对应的地块索引
    public int GetCampTileIDByIdx(CampType camp, int idx)
    {
        if (!_villageCampDict.ContainsKey(camp))
            return -1;
        var lst = _villageCampDict[camp];
        if (idx >= lst.Count)
            return -1;
        return lst[idx];
    }

    // 获取阵营列表.
    public List<CampType> GetCampLst()
    {
        return _campTypeList;
    }

    public void Update()
    {
        // 地块点击逻辑.
        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject() && null != _cursor)
        {
            _curSelectTargetUnit = null;

            // 设置目标层.
            int layerMask = 0;
            switch(_curCommandType)
            {
                case CommandType.Attack:
                    layerMask = 1 << LayerMask.NameToLayer("Unit");
                    break;
                default:
                    layerMask = 1 << LayerMask.NameToLayer("Terrain");
                    break;
            }

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            //Debug.DrawLine(ray.origin, new Vector3(ray.origin.x, ray.origin.y - 500, ray.origin.z));
            if (Physics.Raycast(ray, out hitInfo, 500, layerMask))
            {
                switch (_curCommandType)
                {
                    case CommandType.Attack:
                        // 设定当前选择目标对象.
                        _curSelectTargetUnit = hitInfo.collider.gameObject.GetComponent<GKUnit>();
                        Debug.Log(string.Format("Click target name: {0}", _curSelectTargetUnit.name));
                        // 设定当前选择地块.
                        if(null != _curSelectTargetUnit)
                        {
                            _curSelectTile = _curSelectTargetUnit.Grid;
                        }
                        break;
                    default:
                        // 设定当前选择地块.
                        _curSelectTile = hitInfo.collider.gameObject.GetComponent<GKTerrainGrid>();
                        break;
                }
                SetCursor();
                if (null != OnSelectTileChangedEvent)
                    OnSelectTileChangedEvent();

                // 如果当前为命令选择界面.
                if (CommandType.NoCommand != _curCommandType)
                {
                    PrecessCommand();
                }
            }
        }
    }

    public void HighLightTerrain(int mask)
    {
        if (mask == _lastHighLightMask)
            return;

        // Village.
        bool bLast = ((_lastHighLightMask & (int)MoveType.Village) == (int)MoveType.Village);
        bool bCurrent = ((mask & (int)MoveType.Village) == (int)MoveType.Village);
        if (bLast != bCurrent)
        {
            GKMapManager.Instance().HighLightVillage(bCurrent);
        }

        // Food.
        bLast = ((_lastHighLightMask & (int)MoveType.Food) == (int)MoveType.Food);
        bCurrent = ((mask & (int)MoveType.Food) == (int)MoveType.Food);
        if (bLast != bCurrent)
        {
            GKMapManager.Instance().HighLightFood(bCurrent);
        }

        // Grass.
        bLast = ((_lastHighLightMask & (int)MoveType.Grass) == (int)MoveType.Grass);
        bCurrent = ((mask & (int)MoveType.Grass) == (int)MoveType.Grass);
        bool bRoad = ((mask & (int)MoveType.Road) == (int)MoveType.Road);
        if(bLast != bCurrent)
        {
            GKMapManager.Instance().HighLightGrass(bCurrent, bRoad);
        }

        // River.
        bLast = ((_lastHighLightMask & (int)MoveType.River) == (int)MoveType.River);
        bCurrent = ((mask & (int)MoveType.River) == (int)MoveType.River);
        if (bLast != bCurrent)
        {
            GKMapManager.Instance().HighLightRiver(bCurrent, bRoad);
        }

        // Road.
        bLast = ((_lastHighLightMask & (int)MoveType.Road) == (int)MoveType.Road);
        bCurrent = bRoad;
        if (bLast != bCurrent)
        {
            GKMapManager.Instance().HighLightRoad(bCurrent);
        }

        _lastHighLightMask = mask;
    }

    // 获取玩家角色对象列表.
    public void GetSelfUnits(ref List<GKUnit> lst, long selfGuid)
    {
        lst.Clear();
        var data = LevelController.Instance().GetCampData(PlayerController.Instance().Camp);
        foreach (var guid in data.Values)
        {
            // 排除自身.
            if (selfGuid == guid)
                continue;

            var unit = LevelController.Instance().GetTargetByID(guid);
            if (null != unit)
            {
                lst.Add(unit.GetComponent<GKUnit>());
            }
        }
    } 

    // 获取所有非玩家阵营角色.
    public void GetNonPlayerUnits(ref List<GKUnit> lst)
    {
        lst.Clear();
        int count = LevelController.Instance().GetCampCount();
        for (int i = 0; i < count; i++)
        {
            if (PlayerController.Instance().Camp == (CampType)i)
                continue;

            var data = LevelController.Instance().GetCampData((CampType)i);
            foreach (var guid in data.Values)
            {
                var unit = LevelController.Instance().GetTargetByID(guid);
                if (null != unit)
                {
                    lst.Add(unit.GetComponent<GKUnit>());
                }
            }
        }
    }

    // 获取最近的角色.
    public GKUnit GetCloseUnit(List<GKUnit> lst, Transform unit)
    {
        GKUnit tmpUnit = null;
        float distance = 9999;
        float tmpDistance = 0;

        foreach (var u in lst)
        {
            tmpDistance = Vector3.SqrMagnitude(u.myTransform.position - unit.position);
            if (null == tmpUnit || distance > tmpDistance)
            {
                tmpUnit = u;
                distance = tmpDistance;
            }
        }

        return tmpUnit;
    }

    // 获取距离角色最近据点.
    public GKVillage GetCloseVillage(Transform unit)
    {
        GKVillage tmpVillage = null;
        float distance = 9999;
        float tmpDistance = 0;

        foreach(var id in _villageCampDict[(CampType)PlayerController.Instance().Camp])
        {
            GKVillage village =  GKMapManager.Instance().GetVillageByTileID(id);
            if (null == village)
                continue;

            tmpDistance = Vector3.SqrMagnitude(village.myTransform.position - unit.position);

            if(null == tmpVillage || distance > tmpDistance)
            {
                tmpVillage = village;
                distance = tmpDistance;
            }
        }

        return tmpVillage;
    }

    // 获取据点地块索引链表基于阵营.
    public List<int> GetVillageTilesByCamp(CampType camp)
    {
        if (!_villageCampDict.ContainsKey(camp))
            return null;
        return _villageCampDict[camp];
    }

    // 获取阵营视野内对象.
    public List<GKUnit> GetCampSightUnit(CampType camp)
    {
        List<GKUnit> unitLst = new List<GKUnit>();
        // 获取当前视野内对象.
        Dictionary<int, long> lst = LevelController.Instance().GetCampData(camp);
        if (null != lst && 0 < lst.Count)
        {
            foreach (var guid in lst.Values)
            {
                var go = LevelController.Instance().GetTargetByID(guid);
                if (null != go)
                {
                    var friend = go.GetComponent<GKUnit>();
                    if (null != friend)
                    {
                        foreach (var u in friend.GetUnitSightList())
                        {
                            // 排除中立交互NPC。
                            if (null != u && !unitLst.Contains(u) && 3 != u.GetAttribute(EObjectAttr.Type).ValInt)
                                unitLst.Add(u);
                        }
                    }
                }
            }
        }
        return unitLst;
    }

    // 更新视野内地块基于阵营.
    public void UpdateGlobalSightByCamp(CampType camp)
    {
        // 获取当前视野内对象.
        Dictionary<int, long> lst = LevelController.Instance().GetCampData(camp);
        if (null != lst && 0 < lst.Count)
        {
            List<int> tileLst = new List<int>();
            foreach (var guid in lst.Values)
            {
                var go = LevelController.Instance().GetTargetByID(guid);
                if (null != go)
                {
                    var unit = go.GetComponent<GKUnit>();
                    if (null != unit)
                    {
                        foreach (var t in unit.GetTileSightList())
                        {
                            if (null != t && !tileLst.Contains(t.tileID))
                                tileLst.Add(t.tileID);
                        }
                    }
                }
            }
            FOW.Instance().UpdateSight(tileLst);
        }
    }
    #endregion

    #region PrivateMethod
    private void Awake()
    {
        _instance = this;
    }

    // Use this for initialization
    protected void Start()
    {
        // 初始化游戏内阵营数据.
        InitData();
        // 初始化摄像机.
        CameraController.Instance().ResetMainCamera();
        // 初始化地形.
        GKMapManager.Instance().Reset();
        GKMapData mapData = new GKMapData(MyGame.Instance.MapSize);
        GKMapManager.Instance().Build(mapData);
        UIMain.Open();
        // 设置战场标志位.
        MyGame.IsBattle = true;
        // 加载光标对象.
        _cursor = GK.LoadGameObject("Prefabs/Partile/FX_Cursor");
        //_cursor.SetActive(false);
        // 打开起始创建角色界面.
        UICreateUnit.Open().SetData(GetCampTileIDByIdx(PlayerController.Instance().Camp, 0));
        MyGame.HUDText.m_Cam = Camera.main;
        FOW.Instance().OnDiscoverNewAreaEvent += Discover;
        _commanderController.InitCommander();
    }

    // 命令处理.
    private void PrecessCommand()
    {
        var unit = GetCurSelectUnit();
        if (null == unit || null == _curSelectTile)
            return;

        switch(_curCommandType)
        {
            case CommandType.Move:
            case CommandType.Attack:
            case CommandType.Support:
            case CommandType.Defense:
            case CommandType.Pursuit:
            case CommandType.Retreat:
            case CommandType.Ambush:
                unit.ChangeCommand(_curCommandType);
                break;
            case CommandType.Village:
                if(_curSelectTile.GetElement(TerrainElementType.Village))
                {
                    UICreateUnit.Open().SetData(_curSelectTile.tileID, true);
                }
                break;
            case CommandType.Food:
                break;
        }
    }

    // 关卡结束后释放链接, 避免内存泄漏.
    private void OnDestroy()
    {
        GKMapManager.Instance().Reset();
        CameraController.Instance().ReleaseCamera();
    }

    private GKUnit SpawnUnit(CampType camp, int id, GKDataBase dataBase)
    {
        var go = GK.LoadGameObject(string.Format("Prefabs/Unit/{0}", id));
        if (null != go)
        {
            go.SetActive(true);
            GKUnit unit = null;

            // 游戏中角色分为玩家所能控制角色及其他角色.
            if(camp == PlayerController.Instance().Camp)
            {
                unit = GK.GetOrAddComponent<GKPlayer>(go);
                //unit.OnNew(PlayerController.Instance().GetCardDetaileFromPlayer(id));
            }
            else 
            {
                unit = GK.GetOrAddComponent<GKEnemy>(go);
            }
            unit.OnNew(dataBase);
            return unit;
        }
        return null;
    }

    // 获取程式名称.
    private int GetVillageName()
    {
        return 0;
    }

    // 每局游戏开始时初始化游戏数据.
    private void InitData()
    {
        // 临时设置阵营信息.
        List<CampType> camps = new List<CampType>();
        List<int> fowLst = new List<int>();
        camps.Add(PlayerController.Instance().Camp);
        fowLst.Add((int)PlayerController.Instance().Camp);
        camps.Add(CampType.Red);
        fowLst.Add((int)CampType.Red);
        InitCampList(camps);
        FOW.Instance().Init(fowLst, (int)PlayerController.Instance().Camp, GKMapManager.GetTileCount(PolygonType.Hexagon, MyGame.Instance.MapSize));
        _dataList.Clear();
        for (int i = 0; i < _campTypeList.Count; i++)
        {
            GKDataBase dataBase = new GKDataBase();
            _dataList.Add(_campTypeList[i], dataBase);
            SetBelief(_campTypeList[i], 500);
            SetFood(_campTypeList[i], 0);
        }
        _villageCampDict.Clear();
        // 初始化中立阵营.
        _villageCampDict[CampType.Yellow] = new List<int>();
        // 初始化非中立阵营.
        foreach(var c in _campTypeList)
        {
            List<int> lst = new List<int>();
            _villageCampDict[c] = lst;
        }
        // 初始化各个阵营卡牌.
        InitCampCard();
    }

    // 更新玩家战争迷雾状态.
    private void Discover(int camp, int areaIdx)
    {
        // 仅更显玩家迷雾状态. 其他阵营为逻辑更新.
        if(camp == (int)PlayerController.Instance().Camp)
        {
            GKTerrainGrid grid = GKMapManager.Instance().GetGridByKey(areaIdx);
            if (null != grid)
                grid.Discover();
        }
    }

    // 每局游戏结束时释放所有数据.
    private void ReleaseData()
    {
        FOW.Instance().OnDiscoverNewAreaEvent -= Discover;
        //foreach(var data in _dataList)
        //{
        //    data.Value.RecycleAllAttribute();
        //}
        //_dataList.Clear();
    }
    #endregion

    
}

public enum CommandType
{
    NoCommand = 0,
    Move,               // 移动, 期间遇上敌人转化为攻击.
    Explore,            // 探索, 期间遇上敌人转化为撤退.
    Breakout,           // 突围, 期间遇上敌人无视以移动目标点为准.
    Attack,             // 攻击目标单位. 血量低时撤退.
    Deathmatch,         // 攻击目标单位, 直至目标死亡.
    Lure,               // 攻击敌人, 保持一定距离. 移动友军附近.
    Support,            // 移动至目标友军附近. 跟随友军位置变化而变更目标.
    Defense,            // 防御. 格挡及伤害下降.
    Pursuit,            // 移动至目标敌人附近进行攻击. 随敌人坐标变化而变.
    Retreat,            // 移动至最近的城市据点或友军处.
    Ambush,             // 隐藏军队.
    Village,
    Food,
}
