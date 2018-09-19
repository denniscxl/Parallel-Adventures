using UnityEngine;
using GKBase;
using GKToy;

// 游戏引导管理器.
public class SysTutorial : GKSingleton<SysTutorial> {

    #region PublicField

    #endregion

    #region PrivateField
    private GKToyBaseOverlord _tutorialOverlord = null;
    #endregion

    #region PublicMethod
    public void Init()
    {
        // 加载引导逻辑模块.
        _tutorialOverlord = GK.GetOrAddComponent<GKToyBaseOverlord>(GK.TryLoadGameObject(string.Format("Prefabs/Overlord/Tutorial")));
        if (null != _tutorialOverlord)
        {
            _tutorialOverlord.isPlaying = true;
            GK.SetParent(_tutorialOverlord.gameObject, MyGame.Instance.gameObject, false);
        }  
    }

    // 显示引导指示物.
    public void Show(Vector3 pos, Vector2 size, bool finger = true)
    {
        UITutorial.Open().SetStyle(pos, size, finger);
    }
    #endregion

    #region PrivateMethod

    #endregion
}
