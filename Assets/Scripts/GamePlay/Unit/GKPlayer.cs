using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKData;
using GKRole;

public class GKPlayer : GKUnit 
{
    #region PublicField
    #endregion

    #region PrivateField
    #endregion

    #region PublicMethod
    public override void OnNew(GKDataBase data)
    {
        base.OnNew(data);
        SetAttribute(EObjectAttr.Type, 1, false);
    }


    #endregion

    #region PrivateMethod
    protected void Start()
    {
        base.Start();
        AI.StartWhenEnabled = true;
        AI.RestartWhenComplete = true;
    }
    #endregion

    #region Delegate
  
    #endregion
}
