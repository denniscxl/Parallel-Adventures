using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using UnityEngine;
using GKBase;

/**
 * 游戏AI管理器.
 * 此管理器控制非玩家阵营AI逻辑.
 * 逻辑实现通过Behavior Designer实现.
 */
public class GKCommanderController : MonoBehaviour {

    #region PublicField
    #endregion

    #region PrivateField
    // 各个阵营指挥官AI对象.
    private Dictionary<CampType, BehaviorTree> _campCommanderDict = new Dictionary<CampType, BehaviorTree>();
    #endregion

    #region PublicMethod
    // 初始化指挥官角色对象.
    public void InitCommander()
    {
        _campCommanderDict.Clear();
        var lst = LevelController.Instance().GetCampLst();
        if (0 < lst.Count)
        {
            foreach (var camp in lst)
            {
                if (camp != PlayerController.Instance().Camp)
                {
                    GameObject go = new GameObject(camp.ToString());
                    GK.SetParent(go, gameObject, false);
                    var behaviorTree = GK.GetOrAddComponent<BehaviorTree>(go);
                    var extBt = GK.TryLoadResource<ExternalBehaviorTree>("AI/Commander/" + 0);
                    behaviorTree.ExternalBehavior = extBt;
                    behaviorTree.StartWhenEnabled = true;
                    behaviorTree.RestartWhenComplete = true;
                    behaviorTree.GetVariable("Camp").SetValue((int)camp);
                    _campCommanderDict.Add(camp, behaviorTree);
                }
            }
        }
    }
    #endregion

    #region PrivateMethod
    #endregion
}
