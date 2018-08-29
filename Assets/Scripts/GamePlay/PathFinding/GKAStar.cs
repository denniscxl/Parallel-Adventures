using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKMap;

namespace GKPathFinding
{
    public class GKAStar
    {
        #region PublicField

        #endregion

        #region PrivateField
        protected Dictionary<int, AStarPoint> _openList = new Dictionary<int, AStarPoint>();
        protected Dictionary<int, AStarPoint> _closeList = new Dictionary<int, AStarPoint>();

        #endregion

        #region PublicMethod
        /// <summary>
        /// Finds the way.
        /// </summary>
        /// <returns>Points.</returns>
        /// <param name="start">Start point.</param>
        /// <param name="destination">Destination point.</param>
        public List<AStarPoint> FindWay(AStarPoint start, AStarPoint destination)  
        {
            _openList.Clear();
            _closeList.Clear();

            List<AStarPoint> result = new List<AStarPoint>();
            _openList.Add(start.id, start);  
            while (!(_openList.ContainsKey(destination.id) || 0 == _openList.Count))  
            {  
                AStarPoint p = GetMinPointFromOpenList();  
                if (null == p) 
                    return null;  
                _openList.Remove(p.id);  
                _closeList.Add(p.id, p);  
                Check(p, destination);  
            }

            AStarPoint path = null;
            if (_openList.ContainsKey(destination.id))
            {
                _closeList.Add(destination.id, _openList[destination.id]);  
                path = _closeList[destination.id];  
            }
            else
            {
                // 目标点不存在, 遍历获取最接近点
                path = null;
                foreach(var node in _closeList.Values)
                {
                    if(null == path)
                    {
                        path = node;
                        continue;
                    }
                    if((path.G + path.H) > (node.G + node.H))
                    {
                        path = node;
                    }
                }
                //Debug.Log(string.Format("Find path faile. id: {0}", destination.id));
                //return null;

            }
            while (null != path)  
            {
                result.Add(path);
                path = path.last;  
            }
            return result;
        }  

        public List<AStarPoint> FindWay()
        {
            return null;
        }

        public AStarPoint MakePointByGKTerrainGrid(GKTerrainGrid tg, Vector2 destination)
        {
            int h = GetH(new Vector2(tg.row, tg.col), destination);
            return new AStarPoint(tg.tileID, tg.row, tg.col, 0, h, tg, null);
        }
        #endregion

        #region PrivateMethod
        protected AStarPoint GetMinPointFromOpenList()  
        {  
            AStarPoint min = null;  
            foreach (AStarPoint p in _openList.Values) {
                if (min==null || min.G + min.H > p.G + p.H) 
                {
                    min = p;  
                }
            }
            return min;  
        } 

        virtual protected bool IsBlock(GKTerrainGrid g, int from)  
        {
            return false;
        }  
         
        virtual protected int GetG(AStarPoint target)  
        {  
            if (null == target.last) 
                return 0;

            return 0;
        } 

        virtual protected int GetH(AStarPoint target, AStarPoint destination)
        {
            return Mathf.Abs(target.row - destination.row) + Mathf.Abs(target.col - destination.col);
        }

        virtual protected int GetH(Vector2 start, Vector2 destination)
        {
            int x = Mathf.Abs((int)(start.x - destination.x));
         
            if(start.x < destination.x)
            {
                destination.y = destination.y - Mathf.RoundToInt(x * 0.5f);
            }
            else
            {
                destination.y = destination.y + Mathf.RoundToInt(x * 0.5f);
            }

            return  (x + Mathf.Abs((int)(start.y - destination.y))) * 20;
        }

        protected void Check(AStarPoint target, AStarPoint destination)
        {
            if (null == target || null == target.grid || null == target.grid._nexts)
            {
                Debug.LogError(string.Format("AStar Check faile, grid is null. id: {0}", target.id));
                return;
            }
            List<int> block = new List<int>();
            for (int i = 0; i < target.grid._nexts.Length; i++)
            {
                var n = target.grid._nexts[i];
                if (null != n && !_closeList.ContainsKey(n.grid.tileID) && !IsBlock(n.grid, i))
                {
                    if (_openList.ContainsKey(n.grid.tileID))
                    {
                        AStarPoint op = _openList[n.grid.tileID];
                        int newG = GetG(target);
                        if (newG < op.G)
                        {
                            _openList.Remove(op.id);
                            op.last = target;
                            op.G = newG;
                            _openList.Add(op.id, op);
                        }
                    }
                    else
                    {
                        GKTerrainGrid tg = n.grid;
                        AStarPoint p = MakePointByGKTerrainGrid(tg, new Vector2(destination.row, destination.col));
                        p.last = target;
                        p.G = GetG(target);
                        _openList.Add(p.id, p);
                    }
                }
            }
        }
        #endregion

    }

    public class AStarPoint
    {
        public AStarPoint()
        {
        }
        public AStarPoint(int id, int x, int y, int g, int h, GKTerrainGrid grid, AStarPoint last)
        {
            this.id = id;
            row = x;
            col = y;
            G = g;
            H = h;
            this.grid = grid;
            this.last = last;
        }
        public int id;
        public int row;
        public int col;
        public int G;
        public int H;
        public GKTerrainGrid grid;
        public AStarPoint last;
    }
}
