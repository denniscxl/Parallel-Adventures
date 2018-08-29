using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKBase;
using GKData;
using GKRole;
using GKUI;

public class GKFood : GKNpc {
    
    #region PublicField
    #endregion

    #region PrivateField
    private UIFoodHUD _hud;
    private GameObject _particle;
    #endregion

    #region PublicMethod
    public override void OnNew(GKDataBase data)
    {
        base.OnNew(data);
        // 营地不需要状态机变化.
        bState = false;
        Init();
    }

    // 变更阵营.
    public void ChangeCamp(CampType type, bool bDoEvent = true)
    {
        _data.SetAttribute((int)EObjectAttr.Camp, (int)type, bDoEvent);
    }

    #endregion

    #region PrivateMethod
    private void Init()
    {
        InitListener();
        LoadParticle();
    }

    private void InitListener()
    {
        _data.GetAttribute((int)EObjectAttr.Camp).OnAttrbutChangedEvent += OnCampChanged;
    }

    private void Destroy()
    {
        _data.GetAttribute((int)EObjectAttr.Camp).OnAttrbutChangedEvent -= OnCampChanged;
    }

    private UIFoodHUD SpawnHUD()
    {
        var hud = UIController.LoadPanel<UIFoodHUD>(); ;
        GK.SetParent(hud.gameObject, UIController.instance.hudRoot, false);
        return hud;
    }

    private void LoadParticle(CampType type = CampType.Yellow)
    {
        string path = string.Format("Prefabs/Partile/FX_Sparkle_{0}", type.ToString());
        _particle = GK.LoadGameObject(path);
        GK.SetParent(_particle, gameObject, false);
    }

    // 阵营变更回调.
    private void OnCampChanged(object obj, GKCommonValue attr)
    {
        if (null != _particle)
        {
            GK.Destroy(_particle);
            _particle = null;
        }
        LoadParticle((CampType)attr.ValInt);
    }
    #endregion

    #region Delegate
    #endregion
}
