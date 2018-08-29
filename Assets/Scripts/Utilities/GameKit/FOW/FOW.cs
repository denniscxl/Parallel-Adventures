using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKBase;

namespace GKFOW
{
    // 战争迷雾模块.
    public class FOW : GKSingleton<FOW>
    {

        #region PublicField
        public delegate void DiscoverNewArea(int camp, int areaIdx);
        public DiscoverNewArea OnDiscoverNewAreaEvent = null;
        public delegate void SightChange(List<int> lst);
        public SightChange OnSightChange = null;
        #endregion

        #region PrivateField
        // 探索/为探索全局索引.
        private Dictionary<int, List<int>> _discoveryDict = new Dictionary<int, List<int>>();
        // 以探索区域.
        private Dictionary<int, int> _discoveryCount = new Dictionary<int, int>();
        // 效果显示阵营.
        private int _myCamp = 0;
        // 探索范围.
        private int _size = 0;
        // 视野地块链表.
        private List<int> _sightLst = new List<int>();
        #endregion

        #region PublicMethod
        // 初始化迷雾数据.
        public void Init(List<int>campLst,int myCamp, int mapSize)
        {
            _size = mapSize;
            _discoveryDict.Clear();
            foreach(var camp in campLst)
            {
                List<int> discover = new List<int>(mapSize);
                for (int i = 0; i < mapSize; i++)
                {
                    discover.Add(i);
                }
                _discoveryDict.Add(camp, discover);
                _discoveryCount.Add(camp, 0);
            }
        }

        // 更新探索区域.
        public void UpdateDiscoverArea(int camp, List<int> lst)
        {
            if (!_discoveryDict.ContainsKey(camp))
                return;
            
            foreach(var idx in lst)
            {
                if(_discoveryDict[camp].Contains(idx))
                {
                    if (null != OnDiscoverNewAreaEvent)
                    {
                        _discoveryDict[camp].Remove(idx);
                        _discoveryCount[camp] += 1;
                        OnDiscoverNewAreaEvent(camp, idx);
                    }
                }
            }
        }

        // 获取未探索地块数量.
        public int UnexploredCount(int camp)
        {
            if (!_discoveryCount.ContainsKey(camp))
                return 0;
            return _size - _discoveryCount[camp];
        }

        // 获取未探索区域索引.
        public List<int> GetUnDiscoverLst(int camp)
        {
            if (!_discoveryDict.ContainsKey(camp))
                return null;
            return _discoveryDict[camp];
        }

        // 是否在视野内.
        public bool InSight(int idx)
        {
            return _sightLst.Contains(idx);
        }

        // 更新视野.
        public void UpdateSight(List<int> lst)
        {
            _sightLst.Clear();
            foreach (var l in lst)
                _sightLst.Add(l);

            if (null != OnSightChange)
                OnSightChange(_sightLst);
        }
        #endregion

        #region PrivateMethod
        #endregion
    }
}
