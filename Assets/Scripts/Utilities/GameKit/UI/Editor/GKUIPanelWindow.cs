using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using GKBase;

namespace GKUI
{
    public class GKUIPanelWindow : EditorWindow
    {
        List<Entry> settingAssetList = new List<Entry>();
        List<Entry> skinAssetList = new List<Entry>();
        List<Entry> panelList = new List<Entry>();

        static bool settingAssetFoldout = true;
        static bool skinsFoldout = true;
        static bool panelFoldout = true;

        static bool bShowDemoAssets = true;

        [MenuItem("GK/UI/Convert BoxCollider 2D to 3D")]
        public static void MenuItem_ConvertBoxCollider2Dto3D()
        {
            foreach (var o in Selection.gameObjects)
            {
                ConvertBoxCollider2Dto3D(o);
            }
        }

        [MenuItem("GK/UI/Panel Window", false, GKEditorConfiger.MenuItemPriorityA)]
        public static void MenuItem_Window()
        {
            var w = EditorWindow.GetWindow<GKUIPanelWindow>("UI Panel Window Panels");
            w.autoRepaintOnSceneChange = true;
            w.minSize = new Vector2(200, 18);
            w.Show();
        }

        static void ConvertBoxCollider2Dto3D(GameObject o)
        {
            o.layer = GK.LayerId("UI");

            var rb = o.GetComponent<Rigidbody>();
            if (rb)
            {

                GK.Destroy(rb);
            }

            var co = o.GetComponent<BoxCollider2D>();

            if (co)
            {

                Vector4 v4 = new Vector4(co.size.x, co.size.y, co.offset.x, co.offset.y);

                GK.Destroy(co);

                o.AddComponent<BoxCollider>();
                o.GetComponent<BoxCollider>().size = new Vector3(v4.x, v4.y, 1);
                o.GetComponent<BoxCollider>().center = new Vector3(v4.z, v4.w, 1);
            }

            foreach (Transform t in o.transform)
            {
                ConvertBoxCollider2Dto3D(t.gameObject);
            }
        }

        public class Entry
        {
            public string path;
            public string name;
            public GameObject sceneObj;

            public Entry(string path)
            {
                this.path = path.Replace('\\', '/');
                name = System.IO.Path.GetFileNameWithoutExtension(path);
                sceneObj = GameObject.Find("UIController/UIRoot/" + name);
            }

            public void Load()
            {
                GameObject go = GKEditor.LoadGameObject(path, true);
                sceneObj = go;

                GK.SetParent(go, GKUIEditor.ui.GetOrAddGroup("UIRoot"), false);
                var tran = go.GetComponent<RectTransform>();
                if (null != tran)
                {
                    tran.localPosition = Vector3.zero;
                    tran.sizeDelta = Vector2.zero;
                }
                Selection.activeGameObject = go;
            }

            public void Save()
            {
                if (sceneObj)
                {
                    GKEditor.CreateOrReplacePrefab(path, sceneObj);
                    AssetDatabase.SaveAssets();
                    GK.Destroy(sceneObj);
                    sceneObj = null;
                }
            }

            public void ToggleForSceneObject()
            {
                GUILayout.Space(16);
                var b = sceneObj != null;
                if (GUILayout.Toggle(b, name) != b)
                {
                    if (sceneObj)
                    {
                        Save();
                    }
                    else
                    {
                        Load();
                    }
                }
            }

            public void ButtonForSelect()
            {
                GUILayout.Space(16);
                if (GUILayout.Toggle(false, name))
                {
                    var o = GKEditor.LoadResource<Object>(path);
                    if (o)
                    {
                        Selection.activeObject = o;
                    }
                }
            }
        }


        void OnEnable()
        {
            RefreshList();
        }

        void RefreshList()
        {

            string rootPath = /*bShowDemoAssets ? "Assets/Resources/_Demo/" :*/ "Assets/Resources/";



            {
                settingAssetList.Clear();
                var list = System.IO.Directory.GetFiles(rootPath + "UI/Settings", "*.asset");
                foreach (var p in list)
                {
                    var e = new Entry(p);
                    settingAssetList.Add(e);
                }
            }

            {
                panelList.Clear();
                var list = System.IO.Directory.GetFiles(rootPath + "UI/Panels", "*.prefab");
                foreach (var p in list)
                {
                    var e = new Entry(p);
                    panelList.Add(e);
                }
            }

            //{
            //	skinAssetList.Clear(); 
            //	var list = System.IO.Directory.GetFiles( rootPath + "UI/Skins", "*.asset" );
            //	foreach( var p in list ) {
            //		var e = new Entry( p );
            //		skinAssetList.Add( e );
            //	}
            //}
        }

        Vector2 scroll;

        void OnGUI()
        {

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Box(" ", EditorStyles.toolbarButton);
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                {
                    RefreshList();
                }
                if (GUILayout.Toggle(bShowDemoAssets, "Demo", EditorStyles.toolbarButton) != bShowDemoAssets)
                {
                    bShowDemoAssets = !bShowDemoAssets;
                    RefreshList();
                }
                GUILayout.Box(" ", EditorStyles.toolbarButton);
            }
            EditorGUILayout.EndHorizontal();

            scroll = EditorGUILayout.BeginScrollView(scroll);

            settingAssetFoldout = EditorGUILayout.Foldout(settingAssetFoldout, "Settings");
            if (settingAssetFoldout)
            {
                foreach (var e in settingAssetList)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        e.ButtonForSelect();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);

            skinsFoldout = EditorGUILayout.Foldout(skinsFoldout, "Skins");
            if (skinsFoldout)
            {
                foreach (var e in skinAssetList)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        e.ButtonForSelect();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            panelFoldout = EditorGUILayout.Foldout(panelFoldout, "Panels");
            if (panelFoldout)
            {
                foreach (var e in panelList)
                {
                    EditorGUILayout.BeginHorizontal();
                    {
                        e.ToggleForSceneObject();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndScrollView();

        }
    }
}