using UnityEngine;
using System.Collections;
using GKBase;

namespace GKUI
{
    [DisallowMultipleComponent]
    public class UIBase : MonoBehaviour
    {

        public int priority = 0;

        public GameObject FindChild(string name)
        {
            var c = TryFindChild(name);
            if (!c) Debug.LogWarning("Panel cannot find child: Panel=" + this.gameObject.name + " child=" + name);
            return c;
        }

        public GameObject TryFindChild(string name)
        {
            return GK.FindChild(this.gameObject, name, true);
        }

        virtual public void OnOpen()
        {
        }

        virtual public void OnClose() { }
        public void DoClose()
        {
            OnClose();
            UIController.ClosePanel(this);
        }

        virtual public void OnActive(bool b) { }

        virtual public void OnPosition(Vector3 pos) { }

        virtual public void OnSize(Vector2 size) { }

    }

    public class SingletonUIBase<T> : UIBase where T : UIBase
    {
        protected static T instance_;
        public static T instance { get { return instance_; } }
        public static bool active { get { if (instance) return instance.gameObject.activeSelf; return false; } }
        private static bool _keepInMemory = false;
        public static bool keepInMemory
        {
            set
            {
                //			Debug.Log (string.Format ("keepInMemory Set: {0}", value));
                _keepInMemory = value;
            }
            get
            {
                return _keepInMemory;
            }
        }

        public static T Open(bool b)
        {
            return b ? Open() : Close();
        }

        public static T Open()
        {
            if (!instance_)
            {
                instance_ = UIController.LoadPanel<T>();
            }
            _Init(instance_);
            return instance_;
        }

        public static T OpenDemo()
        {
            if (!instance_)
            {
                instance_ = UIController.LoadDemoPanel<T>();
            }
            _Init(instance_);
            return instance_;
        }

        private static void _Init(T instance)
        {
            if (instance)
            {
                instance.gameObject.SetActive(true);
                RectTransform rt = GK.GetOrAddComponent<RectTransform>(instance.gameObject);
                rt.sizeDelta = Vector2.zero;
                rt.anchoredPosition = Vector3.zero;
                UIController.AddUI(instance);
                instance.OnOpen();
            }
        }

        public static T Close()
        {
            if (!instance_) return null;
            //		Debug.Log (string.Format("keepInMemory: {0}", keepInMemory));
            if (keepInMemory)
            {
                instance_.gameObject.SetActive(false);
                instance_.OnClose();
            }
            else
            {
                UIController.RemoveUI(instance_);
                Release();

            }
            return null;
        }

        public static void Release()
        {
            if (instance_)
            {
                instance_.DoClose();
                instance_ = null;
            }
        }

        public void PlayCilckSound()
        {

        }
    }
}