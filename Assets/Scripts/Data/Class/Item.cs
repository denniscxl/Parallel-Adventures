using System.Collections;
using System.Collections.Generic;
using GKData;
using UnityEngine;

// 游戏中角色信息结构体.
[System.Serializable]
public class Item
{
    #region PublicField
    public ItemType type;
    public int id;
    public int count;
    public int solt;
    #endregion

    #region PrivateField
    #endregion

    #region PublicMethod
    public Item(int solt, int type, int id, int count)
    {
        this.solt = solt;
        this.type = (ItemType)type;
        this.id = id;
        this.count = count;
    }
    #endregion

    #region PrivateMethod

    #endregion
}

public enum ItemType
{
    Empty = 0,
    Equipment,
    Consume,
}
