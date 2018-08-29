using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKBase;
using GKData;

public class AudioController : GKSingleton<AudioController>  
{
    #region PublicField
    public float Sound
    {
        get { return _data.GetAttribute((int)EObjectAttr.Sound).ValFloat; }
        set
        {
            _data.SetAttribute((int)EObjectAttr.Sound, value, true);
            DataController.Instance().SaveAudioData();
        }
    }
    public float Music
    {
        get { return _data.GetAttribute((int)EObjectAttr.Music).ValFloat; }
        set
        {
            _data.SetAttribute((int)EObjectAttr.Music, value, true);
            DataController.Instance().SaveAudioData();
        }
    }

    public GKDataBase GetDataBase()
    {
        return _data;
    }
    public void SetDataBase(GKDataBase data)
    {
        _data = data;
    }
    #endregion

    #region PrivateField
    private GKDataBase _data = new GKDataBase();
    #endregion

    #region PublicMethod
    #endregion

    #region PrivateMethod
    #endregion
}
