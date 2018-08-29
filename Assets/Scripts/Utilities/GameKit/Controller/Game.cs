using UnityEngine;
using GKBase;

namespace GKController
{
    public class Game : MonoBehaviour
    {

        #region Data
        #endregion

        #region PublicField
        #endregion

        #region PrivateField
        #endregion

        #region PublicMethod
        public virtual void Init()
        {
            InitRoot();
        }

        public virtual void InitRoot()
        {
        }
        #endregion

        #region PrivateMethod

        #endregion
    }

    public class SingletonGame<T> : Game where T : Game
    {
        static protected T _instance = null;
        static public T Instance
        {
            get
            {
                if (null == _instance)
                {
                    var o = GameObject.Find("Game");
                    if (!o)
                    {
                        _instance = GK.GetOrAddComponent<T>(GK.TryLoadGameObject(string.Format("Prefabs/Manager/Game")));
                    }
                    else
                    {
                        _instance = GK.GetOrAddComponent<T>(o);
                    }
                    _instance.gameObject.name = "Game";
                }
                return _instance;
            }
        }
    }
}
