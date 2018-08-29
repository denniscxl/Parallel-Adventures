using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GKMap
{
    public class GKHexagon : GKTile
    {

        #region PublicField
        #endregion

        #region PrivateField
        private const int _count = 6;
        #endregion

        #region PublicMethod
        #endregion

        #region PrivateMethod
        override protected void SetPolygon()
        {
            _polygonID = new int[_count];
            _roadFlag = new bool[_count];
            _nodes = new GameObject[(int)TileNodeType.Count];
        }
        override protected void AdjustRoads(int angle)
        {
            bool[] _temp = new bool[6];
            for (int i = 0; i < 6; i++)
            {
                _temp[i] = _roadFlag[i];
            }
            for (int i = 0; i < 6; i++)
            {
                int idx = (i + angle) % 6;
                _roadFlag[idx] = _temp[i];
            }

        }
        #endregion
    }

    public enum TileNodeType
    {
        Center = 0,
        Up,
        RightUp,
        RightDown,
        Down,
        LeftDown,
        LeftUp,
        Village,
        Count
    }
}
