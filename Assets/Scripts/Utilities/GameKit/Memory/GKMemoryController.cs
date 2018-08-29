using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GKBase;

namespace GKMemory
{
    public class GKMemoryController : GKSingleton<GKMemoryController>
    {

        #region PublicField

        #endregion

        #region PrivateField
        protected Dictionary<System.Type, object> _objectPool = new Dictionary<System.Type, object>();
        public int _lastGUID = 1;
        #endregion

        #region PublicMethod
        // 获取对象GUID.
        public int GetGUID()
        {
            return _lastGUID++;
        }

        // 创建对象池, 创建完毕后可调用函数, Spawn 与 Recycle进行管理.
        public GKObjectPool<T> GetOrCreateObjectPool<T> (int iMaxCount, bool bCreateWhenNoFind) where T : class, new ()
        {
            System.Type tType = typeof(T);
            object obj = null;
            if(!_objectPool.TryGetValue(tType, out obj) || null == obj)
            {
                GKObjectPool<T> newPool = new GKObjectPool<T>(iMaxCount);
                _objectPool.Add(tType, newPool);
                return newPool;
            }
            return obj as GKObjectPool<T>;
        }

        // 从对象池中取出一个对象.
        public T GetObjectFromPool<T>(int iMaxCount) where T : class, new()
        {
            GKObjectPool<T> pool = GetOrCreateObjectPool<T>(iMaxCount, true);
            if(null == pool)
            {
                return null;
            }
            return pool.Spawn(true);
        }

        // 回收对象到对象池中.
        public bool ReleaseObjectByPool<T>(T obj) where T: class, new()
        {
            GKObjectPool<T> pool = GetOrCreateObjectPool<T>(-1, false);
            if(null == pool)
            {
                return false;
            }
            return pool.Recycle(obj);
        }
        #endregion

        #region PrivateMethod

        #endregion
    }
}
