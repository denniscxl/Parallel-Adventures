using UnityEngine;
using System.Collections.Generic;
using GKBase;

namespace GKUI
{
    public class UIController : MonoBehaviour
    {
        static UIController instance_;
        public static UIController instance
        {
            get
            {
                if (null == instance_)
                {
                    instance_ = GK.GetOrAddComponent<UIController>(GK.TryLoadGameObject("Prefabs/Manager/UIController"));
                }
                return instance_;
            }
        }

        private static List<UIBase> list = new List<UIBase>();
        public Camera m_camera = null;

        public static int width = 1124;
        public static int height = 2436;

        void Awake()
        {
            DontDestroyOnLoad(this);
            list.Clear();
        }

        static public void AddUI(UIBase ui)
        {
            if (!list.Contains(ui))
            {
                list.Add(ui);
            }

            Sort();
        }

        static public void RemoveUI(UIBase ui)
        {
            if (list.Contains(ui))
            {
                list.Remove(ui);
            }
        }

        static private void Sort()
        {
            UIBase temp;
            for (int i = 0; i < list.Count; i++)
            {
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (list[j].priority < list[i].priority)
                    {
                        temp = list[j];
                        list[j] = list[i];
                        list[i] = temp;
                    }
                }
            }

            foreach (var ui in list)
            {
                if (null == ui || null == ui.GetComponent<RectTransform>())
                {
                    Debug.LogError("UI rectTranform is null");
                    continue;
                }
                ui.GetComponent<RectTransform>().SetSiblingIndex(100);
            }
        }

        static UI_Settings settings_;
        public static UI_Settings settings
        {
            get
            {
                if (settings_ == null)
                {
                    settings_ = GK.LoadResource<UI_Settings>("UI/Settings/UI_Settings");
                }
                return settings_;
            }
        }

        public GameObject panelGroup;

        public static void ClosePanel<T>(T panel) where T : MonoBehaviour
        {
            if (panel != null)
            {
                GK.Destroy(panel.gameObject);
            }
        }

        public static T LoadDemoPanel<T>() where T : MonoBehaviour
        {
            return LoadPanel<T>("_Demo/UI/Panels/");
        }

        public static T LoadPanel<T>(string p = "UI/Panels/") where T : MonoBehaviour
        {
            var type = typeof(T);

            string path = p + type.Name;

            GameObject obj = null;

            if (!obj)
            {
                obj = GK.TryLoadGameObject(path);
            }

            if (!obj)
            {
                Debug.LogError("Error load UI panle " + type.Name);
                return null;
            }

            var c = obj.GetComponent<T>();
            if (!c)
            {
                Debug.LogError("Error GetComponent from UI Panel " + type.Name);
                return null;
            }

            GK.SetParent(c.gameObject, instance.panelGroup.gameObject, false);

            return c;
        }

        public static AnimationClip LoadAnimation(string clip)
        {
            string path = "UI/Animations/" + clip;

            AnimationClip obj = null;

            obj = Resources.Load(path, typeof(AnimationClip)) as AnimationClip;

            if (!obj)
            {
                Debug.LogError("Error load animation " + clip);
                return null;
            }

            return obj;
        }

        public GameObject GetOrAddGroup(string name)
        {
            var o = GK.FindChild(gameObject, name, true, false);
            if (!o)
            {
                o = new GameObject(name);
                GK.SetParent(o, gameObject, false);
                o.layer = GK.LayerId("UI");
            }
            return o;
        }

        public void Init()
        {
        }

        public void CloseAllPanel()
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                list[i].DoClose();
            }
            list.Clear();
        }

        public GameObject hudRoot;

        // 是否显示HUD.
        // Locked 锁定. 当锁定状态时，其他调用此函数除非同样为锁定, 否则无效.
        private bool _hudLocked = false;
        public void ShowHUD(bool show, bool lodked = false)
        {
            if (lodked)
                _hudLocked = !_hudLocked;
            // 如果此次操作为Lock级.
            if (lodked)
                hudRoot.SetActive(show);
            else if (!_hudLocked)
                hudRoot.SetActive(show);

        }

        public void ClearHUD()
        {
            GK.DestroyAllChildren(hudRoot);
        }
    }
}