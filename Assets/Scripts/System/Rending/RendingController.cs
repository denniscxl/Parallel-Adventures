using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKBase;
using GKData;

public class RendingController : GKSingleton<RendingController>  
{
    #region PublicField
    public int Quality
    {
        get { return _data.GetAttribute((int)EObjectAttr.RendingQuality).ValInt; }
        set
        {
            _data.SetAttribute((int)EObjectAttr.RendingQuality, value, true);
            DataController.Instance().SaveRending();
        }
    }
    #endregion

    #region PrivateField
    private GKDataBase _data = new GKDataBase();
    #endregion

    #region PublicMethod
    public GKDataBase GetDataBase()
    {
        return _data;
    }
    public void SetDataBase(GKDataBase data)
    {
        _data = data;
    }
    #endregion

    #region PrivateMethod
    #endregion
}
