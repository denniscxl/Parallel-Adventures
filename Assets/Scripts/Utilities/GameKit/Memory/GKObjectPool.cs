using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace GKMemory
{
    public class GKObjectPool<T> where T : class, new()
    {
        #region PublicField
        #endregion

        #region PrivateField
        protected Stack<T> _pool = new Stack<T>();
        // 最大对象数, 小于0表示无限.
        protected int _maxCount = 0;
        // 需要回收的对象个数.
        protected int _needRecycleCount = 0;
        #endregion

        #region PublicMethod
        public GKObjectPool(int iMaxCount)
        {
            _maxCount = iMaxCount;
            for (int i = 0; i < iMaxCount; i++)
            {
                _pool.Push(new T());
            }
        }

        public void Enlarge(int iExpandCound)
        {
            // 不限制长度.
            if (iExpandCound < 0)
            {
                _maxCount = -1;
                return;
            }

            if (_maxCount >= 0)
            {
                _maxCount = _needRecycleCount + _pool.Count + iExpandCound;
            }

            for (int i = 0; i < iExpandCound; i++)
            {
                _pool.Push(new T());
            }
        }

        public T Spawn(bool bCreateIfPoolEmpty)
        {
            T result = null;
            if (_pool.Count > 0)
            {
                result = _pool.Pop();
                if (null == result)
                {
                    if (bCreateIfPoolEmpty)
                    {
                        result = new T();
                    }
                }
                return result;
            }

            if (bCreateIfPoolEmpty)
            {
                result = new T();
                _needRecycleCount++;
            }

            return result;
        }

        public bool Recycle(T target)
        {
            if (null == target)
            {
                return false;
            }
            _needRecycleCount--;
            if (_pool.Count > _maxCount && _maxCount > 0)
            {
                target = null;
                return false;
            }
            _pool.Push(target);
            return true;
        }

        // 某些对象使用临时内存池, 在场景切换时可以释放这部分内存. 并调用 System.GC.Collect(), 将释放资源清理.
        public void Clear()
        {
            if(null != _pool)
            {
                _pool.Clear();
            }
        }
        #endregion

        #region PrivateMethod

        #endregion
    }
}

