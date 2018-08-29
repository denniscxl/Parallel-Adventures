using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 游戏中可交互对象结构体.
public class InteractiveObject
{
    #region PublicField
    public int id;
    public string name;
    public int bDestory;
    public int maxHp;
    public float speed;
    #endregion

    #region PrivateField
    #endregion

    #region PublicMethod
    public InteractiveObject(int id)
    {
        this.id = id;
    }
    #endregion

    #region PrivateMethod

    #endregion
}
