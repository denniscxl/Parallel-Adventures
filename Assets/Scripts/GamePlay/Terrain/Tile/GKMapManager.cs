using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKPathFinding;
using GKBase;

namespace GKMap
{
    public class GKMapManager : GKSingleton<GKMapManager>
    {

        #region PublicField
        public PolygonType PolygonType = PolygonType.Hexagon;
        #endregion

        #region PrivateField
        private GameObject _root;
        private GameObject Root
        {
            get
            {
                if (null == _root)
                {
                    _root = GameObject.Find("MapRoot");
                    if (null == _root)
                    {
                        _root = GK.TryLoadGameObject(string.Format("Prefabs/Terrain/_MapRoot"));
                    }
                }
                return _root;
            }
        }
        private Dictionary<int, GKTerrainGrid> _tileDict = new Dictionary<int, GKTerrainGrid>();
        private Dictionary<int, GKTerrainGrid> _roadDict = new Dictionary<int, GKTerrainGrid>();
        private Dictionary<int, GKTerrainGrid> _grassDict = new Dictionary<int, GKTerrainGrid>();
        private Dictionary<int, GKTerrainGrid> _riverDict = new Dictionary<int, GKTerrainGrid>();
        private Dictionary<int, GKTerrainGrid> _bridgeDict = new Dictionary<int, GKTerrainGrid>();
        private Dictionary<int, GKTerrainGrid> _villageDict = new Dictionary<int, GKTerrainGrid>();
        private Dictionary<int, GKTerrainGrid> _foodDict = new Dictionary<int, GKTerrainGrid>();
        // 可建造队列.
        private List<int> _buildList = new List<int>();
        #endregion

        #region PublicMethod
        public void Init()
        {
            Reset();
        }

        // 获取指定地块的指定节点.
        public GameObject GetGridNode(int tileID, int NodeID)
        {
            if (!_tileDict.ContainsKey(tileID))
                return null;
            return _tileDict[tileID].tile.GetNode(NodeID);
        }

        // 获取可建造地块.
        public List<int> GetCanBuildTile()
        {
            return _buildList;
        }

        static public int GetTileCount(PolygonType polygon, Size size)
        {
            switch (polygon)
            {
                case PolygonType.Hexagon:
                    switch (size)
                    {
                        case Size.Small:
                            return 169;
                        case Size.Normal:
                            return 331;
                        case Size.Large:
                            return 547;
                        case Size.World:
                            return 817;
                        case Size.Epic:
                            return 1141;
                    }
                    break;
            }
            return 0;
        }

        public Dictionary<int, GKTerrainGrid> GetGrids()
        {
            return _tileDict;
        }

        // 根据key值返回地块数据.
        public GKTerrainGrid GetGridByKey(int key)
        {
            if (!_tileDict.ContainsKey(key))
                return null;
            return _tileDict[key];
        }

        // 构建地图.
        public void Build(GKMapData data)
        {
            Reset();
            CreateTileRoot(data);
            LinkTileRoot(data);
            CreateRiver(data);
            CreateBridge();
            CreateGrass(data);
            CreateVillage(data);
            CreateFood(data);
            CreateRoad(data);
        }

        //  重置地图数据.
        public void Reset()
        {
            _tileDict.Clear();
            _roadDict.Clear();
            _grassDict.Clear();
            _riverDict.Clear();
            _bridgeDict.Clear();
            _villageDict.Clear();
            _foodDict.Clear();
            _buildList.Clear();
            _root = null;
        }

        #region HighLight
        // 高亮道路地块.
        public void HighLightRoad(bool show)
        {
            foreach (var t in _roadDict.Values)
            {
                if (null == t)
                    continue;
                t.HighLight.SetActive(show);
            }
        }

        // 高亮草地地块.
        // 草地包含道路. 故需要判断两种状态不等时, 以道路优先级为主.
        public void HighLightGrass(bool show, bool roadShow)
        {
            foreach (var t in _grassDict.Values)
            {
                if (null == t)
                    continue;

                if(show != roadShow)
                {
                    t.HighLight.SetActive(roadShow);
                }
                else
                {
                    t.HighLight.SetActive(show);
                }
            }
        }

        // 高亮河流地块.
        // 河流包含道路. 故需要判断两种状态不等时, 以道路优先级为主.
        public void HighLightRiver(bool show, bool roadShow)
        {
            foreach (var t in _riverDict.Values)
            {
                if (null == t)
                    continue;
                
                if (show != roadShow)
                {
                    t.HighLight.SetActive(roadShow);
                }
                else
                {
                    t.HighLight.SetActive(show);
                }
            }
        }

        // 高亮据点地块.
        public void HighLightVillage(bool show)
        {
            foreach (var t in _villageDict.Values)
            {
                if (null == t)
                    continue;

                t.HighLight.SetActive(show);
            }
        }

        // 高亮资源点地块.
        public void HighLightFood(bool show)
        {
            foreach (var t in _foodDict.Values)
            {
                if (null == t)
                    continue;

                t.HighLight.SetActive(show);
            }
        }
        #endregion

        // 根据地块索引获取据点对象.
        public GKVillage GetVillageByTileID(int id)
        {
            if (!_villageDict.ContainsKey(id))
                return null;
            return _villageDict[id].Villager;
        }

        #endregion

        #region PrivateMethodth
        private void CreateTileRoot(GKMapData data)
        {
            GK.DestroyAllChildren(Root);
            float width = 10;
            float height = 10;
            float offestX = 0;
            float offestY = 0;
            int count = 0;
            switch(PolygonType)
            {
                case PolygonType.Hexagon:
                    offestX = 3/4*(10*0.5f) + 1;
                    offestY = height * 0.5f;
                    float startX = 0;
                    float startY = 0;
                    int half = (int)((int)data.size * 0.5f);
                    // Left
                    // Row
                    for (int i = half; i > 0; i-- )
                    {
                        startX = (width - offestX) * i;
                        startY = height * half - i * offestY;
                        // Col
                        for (int j = 0; j < (int)data.size-i ; j++)
                        {
                            GameObject node = new GameObject("" + ++count);
                            GK.SetParent(node, Root, false);
                            node.transform.localPosition = new Vector3(-startX, 0, startY - j * height);
                            GKTerrainGrid grid = GK.GetOrAddComponent<GKTerrainGrid>(node);
                            grid.tileID = count;
                            grid.row = half - i;
                            grid.col = j;
                            _tileDict.Add(count, grid);
                            _buildList.Add(count);
                            // 设置包围盒.
                            GK.GetOrAddComponent<BoxCollider>(node).size = new Vector3(10, 1, 10);
                            // 设置层.
                            node.gameObject.layer = LayerMask.NameToLayer("Terrain");
                        }
                    }
                    startY = height * half;
                    for (int i = 0; i < (int)data.size; i++)
                    {
                        GameObject node = new GameObject("" + ++count);
                        GK.SetParent(node, Root, false);
                        node.transform.localPosition = new Vector3(0, 0, startY - i * height);
                        GKTerrainGrid grid = GK.GetOrAddComponent<GKTerrainGrid>(node);
                        grid.tileID = count;
                        grid.row = half;
                        grid.col = i;
                        _tileDict.Add(count, grid);
                        _buildList.Add(count);
                        // 设置包围盒.
                        GK.GetOrAddComponent<BoxCollider>(node).size = new Vector3(10, 1, 10);
                        // 设置层.
                        node.gameObject.layer = LayerMask.NameToLayer("Terrain");
                    }
                    // Right.
                    // Row.
                    for (int i = 1; i <= half; i++)
                    {
                        startX = (width - offestX) * i;
                        startY = height * half - i * offestY;
                        // Col
                        for (int j = 0; j < (int)data.size - i; j++)
                        {
                            GameObject node = new GameObject("" + ++count);
                            GK.SetParent(node, Root, false);
                            node.transform.localPosition = new Vector3(startX, 0, startY - j * height);
                            GKTerrainGrid grid = GK.GetOrAddComponent<GKTerrainGrid>(node);
                            grid.tileID = count;
                            grid.row = half + i;
                            grid.col = j;
                            _tileDict.Add(count, grid);
                            _buildList.Add(count);
                            // 设置包围盒.
                            GK.GetOrAddComponent<BoxCollider>(node).size = new Vector3(10, 1, 10);
                            // 设置层.
                            node.gameObject.layer = LayerMask.NameToLayer("Terrain");
                        }
                    }
                    break;
            }
        }

        private void LinkTileRoot(GKMapData data)
        {
            int raw = 0;
            int col = 0;
            int half = 0;
            int id = 0;
            switch (PolygonType)
            {
                case PolygonType.Hexagon:
                    half = (int)((int)data.size * 0.5f);

                    // Left
                    // Row
                    for (int i = half; i > 0; i--)
                    {
                        // Col
                        for (int j = 0; j < (int)data.size - i; j++)
                        {
                            id++;
                            SetNextByDirection(0, i, j, id, (int)data.size, half);
                        }
                    }
                    // Center
                    for (int i = 0; i < (int)data.size; i++)
                    {
                        id++;
                        SetNextByDirection(1, 0, i, id, (int)data.size, half);
                    }
                    // Right.
                    // Row
                    for (int i = 1; i <= half; i++)
                    {
                        // Col
                        for (int j = 0; j < (int)data.size - i; j++)
                        {
                            id++;
                            SetNextByDirection(2, i, j, id, (int)data.size, half);
                        }
                    }
                    break;
            }
        }

        private void SetNextByDirection(int pos, int row, int col, int id, int size, int half) 
        {
            
            int targetID = 0;
            int beg = 0;
            int last = half;
            // Right
            if(pos == 2)
            {
                beg = half;
                last = 0;
            }

            // Up col isn't 0;
            NextNode upNext = null;
            if (0 != col)
            {
                targetID = id - 1;
                upNext = new NextNode(0, targetID, _tileDict[targetID]);
            }
            _tileDict[id].SetNextNode(0, upNext);
            // RightUp id + cur col count < dict count;
            NextNode rightUpNext = null;
            if (1 == pos)
            {
                if(0 != col)
                {
                    targetID = id + size - 1;
                    rightUpNext = new NextNode(1, targetID, _tileDict[targetID]);
                }
            }
            else if (beg != row)
            {
                if(0 == pos)
                {
                    targetID = id + size - row;
                    rightUpNext = new NextNode(1, targetID, _tileDict[targetID]);
                }
                else if(0 != col)
                {
                    targetID = id + size - row - 1;
                    rightUpNext = new NextNode(1, targetID, _tileDict[targetID]);
                }
            }
            _tileDict[id].SetNextNode(1, rightUpNext);
            // RightDown id + cur col count + 1 < dict count;
            NextNode rightDownNext = null;
            if (0 == pos)
            {
                targetID = id + size - row + 1;
                rightDownNext = new NextNode(2, targetID, _tileDict[targetID]);
            }
            else if (size - row - 1 != col)
            {
                if (1 == pos)
                {
                    targetID = id + size;
                    rightDownNext = new NextNode(2, targetID, _tileDict[targetID]);
                }
                else if (beg != row)
                {
                    targetID = id + size - row;
                    rightDownNext = new NextNode(2, targetID, _tileDict[targetID]);
                }
            }
            _tileDict[id].SetNextNode(2, rightDownNext);
            // Down id + 1 <  cur col count;
            NextNode downNext = null;
            if (size - row - 1 != col)
            {
                targetID = id + 1;
                downNext = new NextNode(3, targetID, _tileDict[targetID]);
            }
            _tileDict[id].SetNextNode(3, downNext);
            // LeftDown id - cur col count + 1 > 0;
            NextNode leftDownNext = null;
            if (1 == pos)
            {
                if (size - 1 != col)
                {
                    targetID = id - size + 1;
                    leftDownNext = new NextNode(4, targetID, _tileDict[targetID]);
                }
            }
            else if (last != row)
            {
                if (2 == pos)
                {
                    targetID = id - (size - row);
                    leftDownNext = new NextNode(4, targetID, _tileDict[targetID]);
                }
                else if (size - row - 1 != col)
                {
                    targetID = id - (size - row) + 1;
                    leftDownNext = new NextNode(4, targetID, _tileDict[targetID]);
                }
            }
            _tileDict[id].SetNextNode(4, leftDownNext);
            // LeftUp id - cur col count > 0;
            NextNode leftUpNext = null;
            if (1 == pos)
            {
                if(0 != col)
                {
                    targetID = id - size;
                    leftUpNext = new NextNode(5, targetID, _tileDict[targetID]);
                }
            }
            else if (last != row)
            {
                if (2 == pos)
                {
                    targetID = id - (size - row) - 1;
                    leftUpNext = new NextNode(5, targetID, _tileDict[targetID]);
                }
                else if (0 != col)
                {
                    targetID = id - (size - row);
                    leftUpNext = new NextNode(5, targetID, _tileDict[targetID]);
                }
            }
            _tileDict[id].SetNextNode(5, leftUpNext);
        }

        private void CreateGrass(GKMapData data)
        {
            
            foreach(var t in _tileDict.Values)
            {
                // If the terrain is empty, fill grass.
                if(null == t.tile)
                {
                    GameObject go = GK.TryLoadGameObject("Prefabs/Terrain/Grass0");
                    if (null != go)
                    {
                        t.LinkTile(go);
                    }
                    _grassDict[t.tileID] = _tileDict[t.tileID];
                }
            }
        }

        private void CreateVillage(GKMapData data)
        {
            data.RandomBuildPos(0);

            //Debug.Log(string.Format("CreateVillage count: {0}", data.villages.Count));

            foreach (var v in data.villages)
            {
                if(_tileDict.ContainsKey(v.Key))
                {
                    GKTile tile = _tileDict[v.Key].tile;
                    var GKVillage = LevelController.Instance().CreateVillage(v.Key);
                    if (null != GKVillage && null != tile)
                    {
                        GKVillage.Grid = _tileDict[v.Key];

                        _tileDict[v.Key].LinkTile(tile.gameObject);
                        tile.AddObject((int)TileNodeType.Village, GKVillage.gameObject);
                        _villageDict[v.Key] = _tileDict[v.Key];
                        _villageDict[v.Key].SetElement(TerrainElementType.Village, true);

                        _tileDict[v.Key].Villager = GKVillage;
                    }
                }else
                {
                    Debug.LogError(string.Format("Create village failed. Can't find tile. tile: {0}", v.Key));
                }
            }
        }

        private void CreateFood(GKMapData data)
        {
            data.RandomBuildPos(1);

            //Debug.Log(string.Format("CreateFood count: {0}", data.foods.Count));

            foreach (var v in data.foods)
            {
                if (_tileDict.ContainsKey(v.Key))
                {
                    GKTile tile = _tileDict[v.Key].tile;
                    GKFood gkFood = LevelController.Instance().CreateFood(1);
                    if (null != gkFood && null != tile)
                    {
                        gkFood.Grid = _tileDict[v.Key];

                        _tileDict[v.Key].LinkTile(tile.gameObject);
                        tile.AddObject((int)TileNodeType.Village, gkFood.gameObject);
                        _foodDict[v.Key] = _tileDict[v.Key];
                        _foodDict[v.Key].SetElement(TerrainElementType.Food, true);

                        _tileDict[v.Key].Food = gkFood;
                    }
                }
                else
                {
                    Debug.LogError(string.Format("Create food failed. Can't find tile. tile: {0}", v.Key));
                }
            }
        }

        private GKTile GetTile(TerrainType type, int idx, int angle = 0)
        {
            string path = string.Format("Prefabs/Terrain/{0}{1}", type.ToString(), idx);
            GameObject go = GK.TryLoadGameObject(path);
            GKTile t = GK.GetOrAddComponent<GKTile>(go);
            t.terrainIdx = idx;
            t.Rotate(angle);
            return t;
        }

        private void CreateRiver(GKMapData data)
        {
            data.RandomRiverPos();
            // Shuffle.
            GK.ShuffleByList<int>(ref data.rivers);
            for (int i = 1; i < data.rivers.Count; i++)
            {
                int srcIdx = data.rivers[i - 1];
                int destIdx = data.rivers[i];
                var start = MyAStar.Instance().MakePointByGKTerrainGrid(_tileDict[srcIdx], new Vector2(_tileDict[destIdx].row, _tileDict[destIdx].col));
                var destination = MyAStar.Instance().MakePointByGKTerrainGrid(_tileDict[destIdx], new Vector2(_tileDict[destIdx].row, _tileDict[destIdx].col));
                var result = MyAStar.Instance().FindWay(start, destination, FindWayType.River);
                if(null != result)
                {
                    int bridge = data.RandomBride(result.Count);
                    if(-1 != bridge)
                    {
                        _bridgeDict[result[bridge].id] = _tileDict[result[bridge].id];
                        _bridgeDict[result[bridge].id].SetElement(TerrainElementType.Road, true);
                        // 道路包含桥梁.
                        _roadDict[result[bridge].id] = _tileDict[result[bridge].id];
                    }
                    foreach (var r in result)
                    {
                        int idx = 1;
                        if (_tileDict[r.id].GetElement(TerrainElementType.Road))
                            idx = 2;
                        var t = GetTile(TerrainType.River, idx);
                        _tileDict[r.id].ReplaceTile(t);
                        _tileDict[r.id].SetElement(TerrainElementType.River, true);
                        _riverDict[r.id] = _tileDict[r.id];
                        // 可建造队列中剔除.
                        if(_buildList.Contains(r.id))
                            _buildList.Remove(r.id);
                    }
                }
            }
        }
            
        /// <summary>
        /// Build a bridge on the river.
        /// The length of the river is increased one grid, the possibility of building the bridge is increased by 10%.
        /// </summary>
        /// <param name="data">Data.</param>
        private void CreateBridge()
        {
            foreach(var b in _bridgeDict.Values)
            {
                var nexts = b.GetNextNodes(1);
                bool revert = false;
                int r1 = 0;
                int r2 = 0;
                switch(nexts.Length)
                {
                    
                    case 0:
                        break;
                    case 1:
                        if (1 != nexts.Length)
                        {
                            revert = true;
                            break;
                        }
                        b.tile.Rotate((nexts[0].direction + 1));
                        break;
                    case 2:
                        if (2 != nexts.Length)
                        {
                            revert = true;
                            break;
                        }
                        r1 = nexts[0].direction;
                        r2 = nexts[1].direction;
                        // Symmetry.
                        if(((r1 + 3) % 6) == r2)
                        {
                            b.tile.Rotate((nexts[0].direction + 1));
                        }
                        else
                        {
                            r1 = nexts[0].direction % 3;
                            r2 = nexts[1].direction % 3;
                            if(1 == (r1 + r2))
                            {
                                b.tile.Rotate(2);
                            }
                            else if(2 == Mathf.Abs(r1 - r2))
                            {
                                b.tile.Rotate(1);
                            }
                        }
                        break;
                    case 3:
                        bool[] canBuildSolt = {true, true, true};
                        for (int i = 0; i < 3; i++)
                        {
                            if(null == nexts[i])
                            {
                                revert = true;
                                break;
                            }
                            int idx = nexts[i].direction % 3;
                            canBuildSolt[idx] = false;
                        }
                        revert = true;
                        for(int i = 0; i < 3; i++)
                        {
                            if(canBuildSolt[i])
                            {
                                b.tile.Rotate(i);
                                revert = false;
                                break;
                            }
                        }
                        break;
                    case 4:
                        if (4 != nexts.Length)
                        {
                            revert = true;
                            break;
                        }
                        var inversNexts = b.GetInversNextNodes(1);
                        if(2 == inversNexts.Length)
                        {
                            r1 = inversNexts[0].direction;
                            r2 = inversNexts[1].direction;
                            revert = true;
                            // Symmetry.
                            if (((r1 + 3) % 6) == r2)
                            {
                                revert = false;
                                b.tile.Rotate(r1);
                            }
                        }
                        break;
                        // There is water around and it can't make a bridge.
                    case 5:
                    case 6:
                        revert = true;
                        break;
                }
                if(revert)
                {
                    var t = GetTile(TerrainType.River, 1);
                    b.ReplaceTile(t);
                    b.SetElement(TerrainElementType.Road, false);  
                }
            }
        }

        private void CreateRoad(GKMapData data)
        {
            List<int> roadList = new List<int>();
            List<int> buildingsList = new List<int>();
            foreach(var v in data.villages.Keys)
            {
                if(!buildingsList.Contains(v))
                {
                    buildingsList.Add(v);
                }
            }
            foreach (var v in data.foods.Keys)
            {
                if (!buildingsList.Contains(v))
                {
                    buildingsList.Add(v);
                }
            }
            // Add start pos.
            if(0 != buildingsList.Count)
            {
                roadList.Add(buildingsList[0]);
                _tileDict[buildingsList[0]].SetElement(TerrainElementType.Road, true);
                //Debug.Log("Start village id: " + villageList[0]);
            }

            // Shuffle.
            GK.ShuffleByList<int>(ref buildingsList);

            // Road path finding.
            for (int i = 1; i < buildingsList.Count; i++)
            {
                //Debug.Log("Other village id: " + villageList[i]);
                int srcIdx = buildingsList[i - 1];
                int destIdx = buildingsList[i];
                var start = MyAStar.Instance().MakePointByGKTerrainGrid(_tileDict[srcIdx], new Vector2(_tileDict[destIdx].row, _tileDict[destIdx].col));
                var destination = MyAStar.Instance().MakePointByGKTerrainGrid(_tileDict[destIdx], new Vector2(_tileDict[destIdx].row, _tileDict[destIdx].col));
                var result = MyAStar.Instance().FindWay(start, destination, FindWayType.Road);
                // If the target path can not be found, the target point will be replaced by the nearest city.
                if (null == result)
                {
                    List<int> tmpList = GK.CloneList<int>(buildingsList);
                    int count = 0;
                    int timeOutCount = tmpList.Count;
                    while(null == result && 0 != tmpList.Count)
                    {
                        if (null == start)
                            break;

                        destination = GetNearbyVillage(start, tmpList);
                        if (null != destination)
                        {
                            result = MyAStar.Instance().FindWay(start, destination, FindWayType.Road);
                            if(tmpList.Contains(destination.id))
                            {
                                tmpList.Remove(destination.id);
                            }
                        }
                        count++;
                        if (timeOutCount < count)
                        {
                            Debug.LogWarning(string.Format("Vliiage can't create road. timeoutcount: {0}, count: {1}", timeOutCount, count));
                            break;
                        }
                            
                    }
                }
                if(null != result)
                {
                    foreach (var r in result)
                    {
                        if (!roadList.Contains(r.id))
                        {
                            roadList.Add(r.id);
                            _tileDict[r.id].SetElement(TerrainElementType.Road, true);
                        }
                    }
                }
            }
            // Create road.
            foreach(var r in roadList)
            {
                // If the area is a rive, Jump it. Because here have bridge.
                if (null != _tileDict[r].tile && TerrainType.River == _tileDict[r].tile.terrain)
                    continue;

                var nexts = _tileDict[r].GetNextNodes();
                int tileID = 0;
                int angle = 0;
                switch(nexts.Length)
                {
                    case 1:
                        tileID = 1;
                        angle = nexts[0].direction;
                        break;
                    case 2:
                        {
                            int d_val = nexts[0].direction;
                            int dir0 = nexts[0].direction - d_val;
                            int dir1 = nexts[1].direction - d_val;

                            if (dir1 < 0)
                            {
                                dir1 = 6 + dir1;
                            }

                            if (3 == dir1)
                            {
                                tileID = 2;
                                angle = d_val;
                            }
                            else if(2 == dir1)
                            {
                                tileID = 3;
                                angle = d_val;
                            }
                            else if (4 == dir1)
                            {
                                tileID = 3;
                                angle = d_val - 2;
                            }
                            else if (1 == dir1)
                            {
                                tileID = 4;
                                angle = d_val;
                            }
                            else if (5 == dir1)
                            {
                                tileID = 4;
                                angle = d_val - 1;
                            }
                        }
                        break;
                    case 3:
                        {
                            int d_val = nexts[0].direction;
                            int dir0 = nexts[0].direction - d_val;
                            int dir1 = nexts[1].direction - d_val;
                            int dir2 = nexts[2].direction - d_val;

                            if (dir1 < 0)
                            {
                                dir1 = 6 + dir1;
                            }
                            else if (dir2 < 0)
                            {
                                dir2 = 6 + dir2;
                            }

                            if (2 == dir1 && 4 == dir2)
                            {
                                tileID = 7;
                                angle = d_val;
                            }
                            else if (1 == dir1 && 2 == dir2)
                            {
                                tileID = 5;
                                angle = d_val;
                            }
                            else if (1 == dir1 && 5 == dir2)
                            {
                                tileID = 5;
                                angle = d_val - 1;
                            }
                            else if (4 == dir1 && 5 == dir2)
                            {
                                tileID = 5;
                                angle = d_val - 2;
                            }
                            else if (2 == dir1 && 3 == dir2)
                            {
                                tileID = 6;
                                angle = d_val;
                            }
                            else if (1 == dir1 && 4 == dir2)
                            {
                                tileID = 6;
                                angle = d_val - 2;
                            }
                            else if (2 == dir1 && 5 == dir2)
                            {
                                tileID = 8;
                                angle = d_val;
                            }
                            else if (3 == dir1 && 4 == dir2)
                            {
                                tileID = 8;
                                angle = d_val - 2;
                            }
                            else if (1 == dir1 && 3 == dir2)
                            {
                                tileID = 8;
                                angle = d_val + 1;
                            }
                            else if (3 == dir1 && 5 == dir2)
                            {
                                tileID = 6;
                                angle = d_val + 3;
                            }
                        }
                        break;
                    case 4:
                        {
                            int d_val = nexts[0].direction;
                            int dir0 = nexts[0].direction - d_val;
                            int dir1 = nexts[1].direction - d_val;
                            int dir2 = nexts[2].direction - d_val;
                            int dir3 = nexts[3].direction - d_val;

                            if (dir1 < 0)
                            {
                                dir1 = 6 + dir1;
                            }
                            else if (dir2 < 0)
                            {
                                dir2 = 6 + dir2;
                            }
                            else if (dir3 < 0)
                            {
                                dir3 = 6 + dir3;
                            }
                            if (1 == dir1 && 2 == dir2 && 3 == dir3)
                            {
                                tileID = 10;
                                angle = d_val;
                            }
                            else if (1 == dir1 && 2 == dir2 && 5 == dir3)
                            {
                                tileID = 10;
                                angle = d_val - 1;
                            }
                            else if (1 == dir1 && 4 == dir2 && 5 == dir3)
                            {
                                tileID = 10;
                                angle = d_val - 2;
                            }
                            else if (3 == dir1 && 4 == dir2 && 5 == dir3)
                            {
                                tileID = 10;
                                angle = d_val + 3;
                            }


                            else if (1 == dir1 && 3 == dir2 && 4 == dir3)
                            {
                                tileID = 9;
                                angle = d_val;
                            }
                            else if (2 == dir1 && 3 == dir2 && 5 == dir3)
                            {
                                tileID = 9;
                                angle = d_val + 2;
                            }
                            else if (2 == dir1 && 3 == dir2 && 4 == dir3)
                            {
                                tileID = 11;
                                angle = d_val - 2;
                            }
                            else if (1 == dir1 && 2 == dir2 && 4 == dir3)
                            {
                                tileID = 11;
                                angle = d_val + 2;
                            }
                            else if (1 == dir1 && 3 == dir2 && 5 == dir3)
                            {
                                tileID = 11;
                                angle = d_val + 1;
                            }
                            else if (2 == dir1 && 4 == dir2 && 5 == dir3)
                            {
                                tileID = 11;
                                angle = d_val;
                            }
                        }
                        break;
                    case 5:
                        {
                            int d_val = nexts[0].direction;
                            int dir0 = nexts[0].direction - d_val;
                            int dir1 = nexts[1].direction - d_val;
                            int dir2 = nexts[2].direction - d_val;
                            int dir3 = nexts[3].direction - d_val;
                            int dir4 = nexts[4].direction - d_val;

                            if (dir1 < 0)
                            {
                                dir1 = 6 + dir1;
                            }
                            else if (dir2 < 0)
                            {
                                dir2 = 6 + dir2;
                            }
                            else if (dir3 < 0)
                            {
                                dir3 = 6 + dir3;
                            }
                            else if (dir4 < 0)
                            {
                                dir4 = 6 + dir4;
                            }

                            if (1 == dir1 && 2 == dir2 && 3 == dir3 && 4 == dir4)
                            {
                                tileID = 12;
                                angle = d_val;
                            }
                            else if (1 == dir1 && 2 == dir2 && 3 == dir3 && 5 == dir4)
                            {
                                tileID = 12;
                                angle = d_val - 1;
                            }
                            else if (1 == dir1 && 2 == dir2 && 4 == dir3 && 5 == dir4)
                            {
                                tileID = 12;
                                angle = d_val - 2;
                            }
                            else if (1 == dir1 && 3 == dir2 && 4 == dir3 && 5 == dir4)
                            {
                                tileID = 12;
                                angle = d_val + 3;
                            }
                            else if (2 == dir1 && 3 == dir2 && 4 == dir3 && 5 == dir4)
                            {
                                tileID = 12;
                                angle = d_val + 2;
                            }
                        }

                        break;
                    case 6:
                        tileID = 13;
                        angle = 0;
                        break;
                    default:
                        tileID = 0;
                        angle = 0;
                        break;
                }
                angle = (angle + 6) % 6;
                // 目前默认为草地. 以后根据设计决定不同道路地块.
                var t = GetTile(TerrainType.Grass, tileID, angle);
                _tileDict[r].ReplaceTile(t);
                _roadDict[r] = _tileDict[r];
                _grassDict[r] = _tileDict[r];
            }
        }

        private AStarPoint GetNearbyVillage(AStarPoint point, List<int> building)
        {
            if (0 >= building.Count || null == point)
                return null;
            AStarPoint ret = null;
            int val = int.MaxValue;
            int idx = -1;

            for (int i = 0; i < building.Count; i ++)
            {
                if (building[i] == point.id)
                    continue;

                int k = GK.CalcPointDistance(point.row, _tileDict[building[i]].row, point.col, _tileDict[building[i]].col);
                if(val > k)
                {
                    val = k;
                    idx = building[i];
                }
            }

            if(-1 != idx)
            {
                ret = MyAStar.Instance().MakePointByGKTerrainGrid(_tileDict[idx], new Vector2(_tileDict[idx].row, _tileDict[idx].col));
            }

            return ret;
        }
        #endregion
    }

    public class VillageData
    {
        public VillageData(int id, VillageType type)
        {
            this.id = id;
            this.type = type;
        }
        public int id;
        public VillageType type = VillageType.Neutrality;
    }

    public class FoodData
    {
        public FoodData(int id)
        {
            this.id = id;
        }
        public int id;
    }

    public enum Size
    {
        Small = 15,     // 169.
        Normal = 21,    // 331.
        Large = 27,     // 547.
        World = 33,     // 817.
        Epic = 39       // 1141.
            
    }

    public enum MapType
    {
        Plain = 0,
        Island,
        InlandSea,
    }

    public enum VillageType
    {
        Red = 0,
        Neutrality,
        Blue
    }

}
