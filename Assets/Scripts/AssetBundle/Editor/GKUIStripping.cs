using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using GKBase;
using GKFile;

public class GKUIStripping : EditorWindow
{
    #region PublicField

    #endregion

    #region privateField
    private static readonly float windowWidth = 660;    // Windows screen width;
    private static readonly float windowHeight = 116;   // Windows screen height;
    private string inputPath = "";
    private bool bRaw = true;
    private bool bSprite = true;
    #endregion

    #region PublicMethod
    [MenuItem("GK/UI/UI GKUIStripping %#s", false, GKEditorConfiger.MenuItemPriorityB)]
    public static void MenuItem_Window()
    {
        var w = EditorWindow.GetWindow<GKUIStripping>("UI Stripping");
        w.autoRepaintOnSceneChange = true;
        w.minSize = new Vector2(windowWidth, windowHeight);
        w.maxSize = new Vector2(windowWidth, windowHeight);
        w.Show();
    }
    #endregion

    #region PrivateMethod
    private void OnGUI()
    {

        GUILayoutOption[] options;

        EditorGUILayout.BeginVertical("Box");
        {
            GUI.skin.label.fontSize = 18;
            GUI.skin.label.alignment = TextAnchor.MiddleCenter;
            GUILayout.Label("UI Stripping", GUILayout.MaxWidth(windowWidth));
            GKEditor.DrawInspectorSeperator();

            GUI.skin.label.fontSize = 10;
            GUI.skin.label.alignment = TextAnchor.MiddleLeft;
            options = new[] { GUILayout.Width(160), GUILayout.Height(16) };
            EditorGUILayout.BeginHorizontal("Box");
            {
                if (string.IsNullOrEmpty(inputPath))
                    inputPath = string.Format("{0}/UserInterface/Data/", Application.dataPath);
                GUILayout.Label("Input: " + inputPath);
                if (GUILayout.Button("Modify Input Paht", options))
                {
                    string tInputPath = EditorUtility.OpenFolderPanel("Path", inputPath, "");
                    if (!string.IsNullOrEmpty(tInputPath))
                        inputPath = tInputPath;
                }
            }
            EditorGUILayout.EndHorizontal();

            options = new[] { GUILayout.Width(100), GUILayout.Height(16) };
            EditorGUILayout.BeginHorizontal("Box");
            {
                bRaw = GUILayout.Toggle(bRaw, "Raw", options);
                bSprite = GUILayout.Toggle(bSprite, "Sprite", options);

                if (GUILayout.Button("Stripping"))
                {

                    if (!bRaw && !bSprite)
                    {
                        EditorUtility.DisplayDialog("Warning", "You have to choose a mode(Raw or Sprite)", "I Know");
                        return;
                    }

                    Generate();

                }

                if (GUILayout.Button("Revert Stripping"))
                {

                    if (!bRaw && !bSprite)
                    {
                        EditorUtility.DisplayDialog("Warning", "You have to choose a mode(Raw or Sprite)", "I Know");
                        return;
                    }

                    Revert();

                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    private void Generate()
    {
        var files = GKFileUtil.GetFilesInDirectory(inputPath);
        foreach (var f in files)
        {
            if (GKFileUtil.IsPrefab(f))
            {
                string rp = GKFileUtil.GetResourcesPath(f);
                string sp = GKFileUtil.GetAssetPath(f);
                string dirP = Path.GetDirectoryName(sp);
                if (dirP.Equals("Assets/UserInterface/Data/States/Init/Resources"))
                    continue;
                var prefab = GK.TryLoadGameObject(rp);
                if (null != prefab)
                {
                    // Raw.
                    if (bRaw)
                    {
                        var raws = prefab.GetComponentsInChildren<RawImage>(true);
                        foreach (var r in raws)
                        {
                            var tex = r.texture;
                            if (null != tex)
                            {
                                var si = GK.GetOrAddComponent<GKUIStripImage>(r.gameObject);
                                si.type = GKUIStripImage.ImageType.Raw;
                                si.assetBundle = GKFileEditor.GetBundleName(AssetDatabase.GetAssetPath(tex), tex.name);
                                si.assetName = tex.name;
                                r.texture = null;
                            }
                        }
                    }
                    // Sprite.
                    if (bSprite)
                    {
                        var images = prefab.GetComponentsInChildren<Image>(true);
                        foreach (var i in images)
                        {
                            var s = i.sprite;
                            if (null != s)
                            {
                                var si = GK.GetOrAddComponent<GKUIStripImage>(i.gameObject);
                                si.type = GKUIStripImage.ImageType.Sprite;
                                string tempPath = AssetDatabase.GetAssetPath(s);
                                if (tempPath.Contains("unity_builtin_extra"))
                                    continue;
                                //Debug.Log(string.Format("src path: {0} | tp path: {1}", s.name, tempPath));
                                si.assetBundle = GKFileEditor.GetBundleName(tempPath, s.name);
                                si.assetName = s.name;
                                i.sprite = null;
                            }
                        }
                    }

                    // Replace.
                    GKEditor.CreateOrReplacePrefab(sp, prefab);
                    AssetDatabase.SaveAssets();
                    GK.Destroy(prefab);

                }
                else
                {
                    Debug.LogWarning("prefab is null, path: " + f);
                }
            }
        }
    }

    private void Revert()
    {
        List<GKUIStripImage> siList = new List<GKUIStripImage>();
        var files = GKFileUtil.GetFilesInDirectory(inputPath);
        foreach (var f in files)
        {
            if (GKFileUtil.IsPrefab(f))
            {
                string rp = GKFileUtil.GetResourcesPath(f);
                string sp = GKFileUtil.GetAssetPath(f);
                string dirP = Path.GetDirectoryName(sp);
                if (dirP.Equals("Assets/UserInterface/Data/States/Init/Resources"))
                    continue;
                var prefab = GK.TryLoadGameObject(rp);
                if (null != prefab)
                {
                    siList.Clear();
                    GK.FindAllChild<GKUIStripImage>(ref siList, prefab);
                    foreach (var s in siList)
                    {
                        string p = string.Format("Assets/ExternalResources/Shanda/Essential/{0}/{1}.png", s.assetBundle, s.assetName);
                        if (s.type == GKUIStripImage.ImageType.Raw)
                        {
                            RawImage ri = s.gameObject.GetComponent<RawImage>();
                            if (ri != null)
                            {
                                Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(p);
                                if (null != tex)
                                    ri.texture = tex;
                            }
                        }
                        else
                        {
                            Image img = s.gameObject.GetComponent<Image>();
                            if (img != null)
                            {
                                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(p);
                                if (null != sprite)
                                    img.sprite = sprite;
                            }
                        }
                        Destroy(s);
                    }

                    // Replace.
                    GKEditor.CreateOrReplacePrefab(sp, prefab);
                    AssetDatabase.SaveAssets();
                    GK.Destroy(prefab);

                }
                else
                {
                    Debug.LogWarning("prefab is null, path: " + f);
                }
            }
        }
    }
    #endregion
}