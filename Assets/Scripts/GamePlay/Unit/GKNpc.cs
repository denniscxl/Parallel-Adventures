using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKData;
using GKRole;

// Interactive object.
public class GKNpc : GKUnit {

    #region PublicField
    public override void OnNew(GKDataBase data)
    {
        base.OnNew(data);
        SetAttribute(EObjectAttr.Type, 3, false);
    }
    #endregion

    #region PrivateField
    protected InteractiveObject _ineteractiveObj;
    #endregion

    #region PublicMethod
    #endregion

    #region PrivateMethod

    #endregion

    #region Delegate

    #endregion
}
