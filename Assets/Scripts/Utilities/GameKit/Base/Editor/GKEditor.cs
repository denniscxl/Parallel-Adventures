using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using GKFile;

namespace GKBase
{
    public class GKEditor
    {
        static public List<string> IterateAllFilesInFolder(string folder, string ext, bool recursive)
        {
            var list = new List<string>();
            _IterateAllFilesInFolder(folder, ext, ref list, recursive);
            return list;
        }

        static void _IterateAllFilesInFolder(string folder, string ext, ref List<string> list, bool recursive)
        {
            foreach (var f in System.IO.Directory.GetFiles(folder))
            {
                if (f.EndsWith(ext, true, null)) list.Add(f);
            }

            if (recursive)
                foreach (var f in System.IO.Directory.GetDirectories(folder))
                {
                    _IterateAllFilesInFolder(f, ext, ref list, recursive);
                }
        }

        static public T[] LoadAllResourcesInFolder<T>(string folder, bool recursive) where T : UnityEngine.Object
        {
            var list = new List<T>();
            _LoadAllResourcesInFolder(folder, ref list, recursive);
            return list.ToArray();
        }

        static void _LoadAllResourcesInFolder<T>(string folder, ref List<T> list, bool recursive) where T : UnityEngine.Object
        {
            foreach (var f in System.IO.Directory.GetFiles(folder))
            {
                var o = (T)AssetDatabase.LoadAssetAtPath(f, typeof(T));
                if (o != null) list.Add(o);
            }

            if (recursive)
                foreach (var f in System.IO.Directory.GetDirectories(folder))
                {
                    _LoadAllResourcesInFolder(f, ref list, recursive);
                }
        }

        public static TextureImporter GetTextureSettings(string path)
        {
            TextureImporter textureImporter = AssetImporter.GetAtPath(path) as TextureImporter;
            //Texture Type
#if UNITY_2017_1_OR_NEWER
            textureImporter.textureType = TextureImporterType.Default;
#else
            textureImporter.textureType = TextureImporterType.Advanced;
#endif
            //Non power of2
            textureImporter.npotScale = TextureImporterNPOTScale.None;
            //Mipmap
            textureImporter.mipmapEnabled = false;
            return textureImporter;
        }

        static public Texture2D LoadTexture2D(string filename) { return LoadResource<Texture2D>(filename); }
        static public Texture LoadTexture(string filename) { return LoadResource<Texture>(filename); }
        static public Material LoadMaterial(string filename) { return LoadResource<Material>(filename); }
        static public Shader LoadShader(string filename) { return LoadResource<Shader>(filename); }
        static public AudioClip LoadAudioClip(string filename) { return LoadResource<AudioClip>(filename); }

        static public Texture2D TryLoadTexture2D(string filename) { return TryLoadResource<Texture2D>(filename); }
        static public Texture TryLoadTexture(string filename) { return TryLoadResource<Texture>(filename); }
        static public Material TryLoadMaterial(string filename) { return TryLoadResource<Material>(filename); }
        static public Shader TryLoadShader(string filename) { return TryLoadResource<Shader>(filename); }
        static public AudioClip TryLoadAudioClip(string filename) { return TryLoadResource<AudioClip>(filename); }

        static public T TryLoadResource<T>(string filename) where T : UnityEngine.Object
        {
            var fullpath = System.IO.Path.GetFullPath(filename).Replace('\\', '/');
            var pwd = System.IO.Directory.GetCurrentDirectory().Replace('\\', '/') + "/";
            var relPath = GKString.GetFromPrefix(fullpath, pwd, true);
            var o = (T)AssetDatabase.LoadAssetAtPath(relPath, typeof(T));
            return o;
        }

        static public T LoadResource<T>(string filename) where T : UnityEngine.Object
        {
            var o = TryLoadResource<T>(filename);
            if (o == null)
            {
                Debug.LogError("Cannot not load resource " + filename);
                return null;
            }
            return o;
        }

        static public GameObject TryLoadPrefab(string filename)
        {
            return AssetDatabase.LoadMainAssetAtPath(filename) as GameObject;
        }


        static public GameObject LoadPrefab(string filename)
        {
            var prefab = TryLoadPrefab(filename);
            if (prefab == null)
            {
                Debug.LogError("cannot load prefab " + filename);
            }
            return prefab;
        }

        static public GameObject TryLoadGameObject(string filename, bool keepPrefabConnection)
        {
            var prefab = TryLoadPrefab(filename);
            if (!prefab) return null;

            if (keepPrefabConnection)
            {
                return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            }
            else
            {
                var obj = (GameObject)GameObject.Instantiate(prefab);
                obj.name = prefab.name;
                return obj;
            }
        }

        static public GameObject LoadGameObject(string filename, bool keepPrefabConnection)
        {
            var prefab = LoadPrefab(filename);
            if (!prefab) return null;

            if (keepPrefabConnection)
            {
                return (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            }
            else
            {
                var obj = (GameObject)GameObject.Instantiate(prefab);
                obj.name = prefab.name;
                return obj;
            }
        }

        //! replace prefab by filename to keep prefab connection
        static public void ReplacePrefab(string filename, GameObject obj)
        {
            var prefab = AssetDatabase.LoadAssetAtPath(filename, typeof(UnityEngine.Object));
            if (prefab == null)
            {
                Debug.LogError("replace prefab not found in " + filename);
            }
            PrefabUtility.ReplacePrefab(obj, prefab);
        }

        //! create prefab by filename and keep prefab connection if previous prefab exists
        static public GameObject CreateOrReplacePrefab(string filename, GameObject obj)
        {
            //Debug.Log("CreateOrReplacePrefab " + filename );

            var prefab = AssetDatabase.LoadMainAssetAtPath(filename) as GameObject;
            if (prefab == null)
            {
                GKFileUtil.CreateDirectoryFromFileName(filename);
                return PrefabUtility.CreatePrefab(filename, obj);
            }
            else
            {
                var o = PrefabUtility.ReplacePrefab(obj, prefab, ReplacePrefabOptions.ReplaceNameBased);
                if (!o)
                {
                    Debug.LogError("Cannot Replace Prefab " + filename);
                }
                return o;
            }
        }

        [MenuItem("GK/Create Asset File for selected ScriptableObject Script %#d")]
        static void MenuItem_CreateStaticAssetFile()
        {
            foreach (var o in Selection.objects)
            {
                var m = o as MonoScript;
                if (!m) return;

                var cls = m.GetClass();
                if (!cls.IsSubclassOf(typeof(ScriptableObject)))
                {
                    Debug.Log("class " + cls.Name + " is not base on SCriptableObject");
                    return;
                }

                var path = "Assets/Resources/" + cls.Name;
                if (string.Equals("UI_Settings", cls.Name))
                {
                    path = "Assets/Resources/UI/Settings/" + cls.Name;
                }
                else if (string.Equals("GKUISkin", cls.Name))
                {
                    path = "Assets/Resources/UI/Skins/" + cls.Name;
                }

                for (int i = 0; i < 500; i++)
                {
                    var filename = path;
                    if (i > 0)
                    {
                        filename += i.ToString().PadLeft(3, '0');
                    }

                    filename += ".asset";

                    if (!System.IO.File.Exists(filename))
                    {
                        var asset = ScriptableObject.CreateInstance(cls);

                        CreateAsset(asset, filename);
                        return;
                    }
                }
                Debug.LogError("Cannot create asset file for " + cls.Name);
            }
        }

        static public T LoadOrCreateAsset<T>(string filename) where T : UnityEngine.Object
        {
            if (System.IO.File.Exists(filename))
            {
                return LoadResource<T>(filename);
            }

            var o = ScriptableObject.CreateInstance(typeof(T)) as T;
            CreateAsset(o, filename);
            return o;
        }

        static public void CreateAsset(UnityEngine.Object asset, string filename)
        {
            GKFileUtil.CreateDirectoryFromFileName(filename);

            Debug.Log("Create Asset File " + filename);

            AssetDatabase.CreateAsset(asset, filename);
        }

        public delegate bool GetAssetSelectionFilter(string path);

        public static UnityEngine.Object[] GetAssetSelection()
        {
            return Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
        }

        public static string[] GetAssetSelectionFilePath(bool expandFileInFolder, GetAssetSelectionFilter filter)
        {
            var sel = GetAssetSelection();
            var output = new List<string>();

            foreach (var s in sel)
            {
                _GetAssetSelectionFilePath(AssetDatabase.GetAssetPath(s), expandFileInFolder, filter, ref output);
            }

            return output.ToArray();
        }

        static void _GetAssetSelectionFilePath(string path, bool expandFileInFolder, GetAssetSelectionFilter filter, ref List<string> output)
        {
            if (System.IO.Directory.Exists(path))
            {
                foreach (var f in System.IO.Directory.GetFileSystemEntries(path))
                {
                    _GetAssetSelectionFilePath(f, expandFileInFolder, filter, ref output);
                }
            }
            else
            {
                var ext = System.IO.Path.GetExtension(path);
                if (ext == ".meta") return;

                if (filter != null)
                {
                    if (!filter(path)) return;
                }
                output.Add(path.Replace('\\', '/'));
            }
        }

        [MenuItem("GK/Disconnect Prefab for Selected Scene Objects")]
        static void MenuItem_DisconnectPrefab()
        {
            foreach (var o in Selection.gameObjects)
            {
                PrefabUtility.DisconnectPrefabInstance(o);
            }
        }

        [MenuItem("Assets/GK/Get GUID")]
        static void MenuItem_GetGUID()
        {
            var list = GetAssetSelectionFilePath(true, null);
            var o = "==== GUID for " + list.Length + " assets === \n";
            foreach (var it in list)
            {
                o += it + "\t\t\t\t\t GUID: " + AssetDatabase.AssetPathToGUID(it) + "\n";
            }
            Debug.Log(o);
        }

        [MenuItem("Assets/GK/Get File Full Path")]
        static void MenuItem_GetAsset_FileFullPath()
        {
            var list = GetAssetSelectionFilePath(true, null);
            var o = "==== File Full Path for " + list.Length + " assets === \n";
            foreach (var it in list)
            {
                o += System.IO.Path.GetFullPath(it) + "\n";
            }
            Debug.Log(o);
        }

        [MenuItem("GK/File Tools/Delete Empty Folders")]
        static void MenuItem_DeleteEmptyFolders()
        {
            var list = DeleteEmptyFolders("Assets");
            Debug.Log("== Deleted folder " + list.Count + " ==\n" + GKString.FromList(list, "\n"));
        }

        //[MenuItem("GK/Copy Full Path %c")]
        [MenuItem("GK/Copy Full Path")]
        static public void MenuItem_CopyFullPath()
        {
            var files = GetAssetSelectionFilePath(false, null);
            if (files.Length != 1)
            {
                Debug.LogWarning("Attempting to copy full path when selecting more than 1 object. " + files.Length);
                return;
            }

            var path = System.IO.Directory.GetCurrentDirectory() + '/' + files[0];
            EditorGUIUtility.systemCopyBuffer = path.Replace('/', '\\');
            Debug.Log("Copy to clipboard: " + EditorGUIUtility.systemCopyBuffer);
        }

        //! return list of deleted folder
        public static List<string> DeleteEmptyFolders(string folder)
        {
            var list = new List<string>();
            _DeleteEmptyFolders(folder, ref list);
            return list;
        }

        static void _DeleteEmptyFolders(string folder, ref List<string> outputList)
        {
            foreach (var f in System.IO.Directory.GetDirectories(folder))
            {
                _DeleteEmptyFolders(f, ref outputList);
            }

            bool isEmpty = true;
            foreach (var f in System.IO.Directory.GetFileSystemEntries(folder))
            {
                var filename = System.IO.Path.GetFileName(f);
                if (filename == ".DS_Store") continue;
                var ext = System.IO.Path.GetExtension(filename).ToLower();
                if (ext == ".meta") continue;
                isEmpty = false;
                break;
            }

            if (isEmpty)
            {
                var f = folder.Replace('\\', '/');
                outputList.Add(f);
                //			AssetDatabase.MoveAssetToTrash( f );
                if (System.IO.Directory.Exists(f))
                {
                    System.IO.Directory.Delete(f, true);
                }
            }
        }

        public static string GetAssetDirectoryFromSelection()
        {
            var outpath = "Assets";
            var sel = Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets);
            if (sel != null && sel.Length > 0)
            {
                var path = AssetDatabase.GetAssetPath(sel[0]);
                if (System.IO.Directory.Exists(path))
                {
                    outpath = path;
                }
                else
                {
                    outpath = System.IO.Path.GetDirectoryName(path);
                }
            }

            return outpath;
        }

        public static bool CreateNewPrefabInAssets<T>() where T : Component
        {

            var t = typeof(T);
            var filename = GetCreateNewAssetFromSelectionFilename(t.Name + ".prefab");

            var obj = new GameObject();
            obj.AddComponent<T>();

            var prefab = PrefabUtility.CreatePrefab(filename, obj);

            Selection.activeObject = prefab;
            GameObject.DestroyImmediate(obj);
            return true;
        }

        public static bool CreateNewScriptableObjectInAssets<T>() where T : ScriptableObject
        {
            var t = typeof(T);
            var filename = GetCreateNewAssetFromSelectionFilename(t.Name + ".asset");

            var obj = ScriptableObject.CreateInstance<T>();
            GKEditor.CreateAsset(obj, filename);
            Selection.activeObject = obj;
            return true;
        }

        public static string GetCreateNewAssetFromSelectionFilename(string filenameSample)
        {
            var outpath = GKEditor.GetAssetDirectoryFromSelection();

            var name = System.IO.Path.GetFileNameWithoutExtension(filenameSample);
            var ext = System.IO.Path.GetExtension(filenameSample);

            int i = 0;

            string filename;
            for (; ; )
            {
                filename = outpath + "/" + name;
                if (i > 0)
                {
                    filename += " " + i.ToString();
                }
                filename += ext;

                if (!System.IO.File.Exists(filename))
                {
                    return filename;
                }

                i++;
            }
        }

        //! create texture and make those pixels readable
        public static Texture2D CreateReadableTexture(Texture tex)
        {
            if (!tex) return null;
            var filename = AssetDatabase.GetAssetPath(tex);

            var ext = System.IO.Path.GetExtension(filename).ToLower();
            if (ext != ".png" && ext != ".jpg")
            {
                Debug.LogError("Only PNG or JPG is supported ext=" + ext);
                return null;
            }

            if (string.IsNullOrEmpty(filename)) return null;

            var srcTex = new Texture2D(0, 0, TextureFormat.ARGB32, false);

            if (!srcTex.LoadImage(System.IO.File.ReadAllBytes(filename)))
            {
                return null;
            }
            return srcTex;
        }

        public static bool FileUpdateNeeded(string dstFile, string srcFile)
        {
            if (!System.IO.File.Exists(dstFile)) return true;

            var dt = System.IO.File.GetLastWriteTime(dstFile);
            var st = System.IO.File.GetLastWriteTime(srcFile);

            if (st.ToFileTime() > dt.ToFileTime()) return true;

            return false;
        }

        public static void DrawInspectorSeperator()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            var rc = GUILayoutUtility.GetRect(1, 1);

            var c = 0.1f;
            EditorGUI.DrawRect(rc, new Color(c, c, c));
            rc.y++;
            EditorGUI.DrawRect(rc, Color.gray);
            EditorGUILayout.Space();
            EditorGUILayout.Space();
        }

        public static void DrawMiniInspectorSeperator()
        {
            var rc = GUILayoutUtility.GetRect(1, 1);
            var c = 0.1f;
            EditorGUI.DrawRect(rc, new Color(c, c, c));
            rc.y++;
            EditorGUI.DrawRect(rc, Color.gray);
        }

        static public SerializedProperty GetArrayPropFromElementProp(SerializedProperty prop)
        {
            var p = System.IO.Path.GetFileNameWithoutExtension(prop.propertyPath);
            var ext = System.IO.Path.GetExtension(p);
            if (ext != ".Array") return null;
            return prop.serializedObject.FindProperty(p);
        }

        static public int GetArrayElementPropIndex(SerializedProperty prop)
        {
            var ext = System.IO.Path.GetExtension(prop.propertyPath);
            var s = GKString.GetFromPrefix(ext, ".data[", false);
            if (string.IsNullOrEmpty(s)) return -1;

            s = GKString.GetFromSuffix(s, "]", false);
            if (string.IsNullOrEmpty(s)) return -2;

            int i;
            if (!int.TryParse(s, out i)) return -3;

            return i;
        }

        static string[] PropertyDrawerArrayElementPopupOptions = new string[] {
        "Move Up",
        "Move Down",
        "Insert",
        "Delete"
    };

        static public void PropertyDrawerArrayElementPopup(Rect rect, SerializedProperty prop, string label)
        {
            PropertyDrawerArrayElementPopup(rect, prop, label, 80);
        }

        static public void PropertyDrawerArrayElementPopup(Rect rect, SerializedProperty prop, string label, float width)
        {
            float w = width;

            var rc = rect;
            rc.width -= w;

            EditorGUI.LabelField(rc, label);

            var arr = GKEditor.GetArrayPropFromElementProp(prop);
            var idx = GKEditor.GetArrayElementPropIndex(prop);

            rc.x = rect.xMax - rect.x - w;
            rc.width = w;

            var sel = EditorGUI.Popup(rc, -1, PropertyDrawerArrayElementPopupOptions);

            if (arr != null)
            {

                switch (sel)
                {
                    case 0: arr.MoveArrayElement(idx, idx - 1); break;
                    case 1: arr.MoveArrayElement(idx, idx + 1); break;
                    case 2: arr.InsertArrayElementAtIndex(idx); break;
                    case 3: arr.DeleteArrayElementAtIndex(idx); break;
                }
            }
        }

        [MenuItem("GK/UI/UI Texture To Prefab")]
        static private void MakeAtlas()
        {
            //Debug.Log(string.Format("UI Texture To Prefab"));

            string spriteDir = Application.dataPath + "/Resources/UI/Sprites/";

            var filesPath = GKFileUtil.GetFilesInDirectory(Application.dataPath + "/Art/_UI/Sprite2Prefab/");

            foreach (var str in filesPath)
            {
                if (string.Empty != GKString.GetFromSuffix(str, ".png", true)
                 || string.Empty != GKString.GetFromSuffix(str, ".jpg", true))
                {
                    string allPath = str;
                    string assetPath = allPath.Substring(allPath.IndexOf("Assets"));
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    GameObject go = new GameObject(sprite.name);
                    go.AddComponent<SpriteRenderer>().sprite = sprite;
                    var dir = GKFileUtil.GetDirctoryName(allPath);
                    allPath = string.Format("{0}{1}/{2}.prefab", spriteDir, dir, sprite.name);
                    GKFileUtil.CreateDirectoryFromFileName(allPath);
                    string prefabPath = allPath.Substring(allPath.IndexOf("Assets"));
                    PrefabUtility.CreatePrefab(prefabPath, go);
                    GameObject.DestroyImmediate(go);
                }
            }
        }

        public static void CopyAsset(string srcPath, string dstPath)
        {
            if (!File.Exists(srcPath))
            {
                Debug.LogError(string.Format("The source file cannot be found. file path: {0} ", srcPath));
                return;
            }

            GKFileUtil.CreateDirectoryFromFileName(dstPath);

            // Copy asset to destination directory.
            File.Copy(srcPath, dstPath, true);

            string dstMetaPath = string.Format("{0}.meta", dstPath);
            string dstMetaGuid = null;
            string dstMetaTime = null;

            //Read the guid and timeCreated in the file.
            if (File.Exists(dstMetaPath))
            {
                using (StreamReader sr = new StreamReader(dstMetaPath))
                {
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        if (line.Contains("guid"))
                        {
                            dstMetaGuid = line;
                        }
                        else if (line.Contains("timeCreated"))
                        {
                            dstMetaTime = line;
                            break;
                        }
                    }
                    sr.Close();
                }
            }

            // Copy meta to destination directory.
            File.Copy(string.Format("{0}.meta", srcPath), dstMetaPath, true);

            // Modify meta data.
            StringBuilder sb = new StringBuilder();
            using (StreamReader sr = new StreamReader(dstMetaPath))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    if (line.Contains("guid"))
                    {
#if UNITY_2017_1_OR_NEWER
                        sb.AppendLine(dstMetaGuid ?? "guid: " + GUID.Generate().ToString());
#else
                        sb.AppendLine(dstMetaGuid ?? "guid: " + Guid.NewGuid().ToString());
#endif
                    }
                    else if (line.Contains("timeCreated"))
                    {
                        sb.AppendLine(dstMetaTime ?? "timeCreated: " + GetGreenwichTime());
                    }
                    else
                    {
                        sb.AppendLine(line);
                    }
                }
                sr.Close();
            }
            using (StreamWriter sw = new StreamWriter(dstMetaPath))
            {
                sw.Write(sb.ToString());
                sw.Close();
            }
        }

        public static int GetGreenwichTime()
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(DateTime.Now - startTime).TotalSeconds;
        }

        public static void DrawArrow(Vector2 from, Vector2 to, Color color)
        {
            Handles.BeginGUI();
            Handles.color = color;
            Handles.DrawAAPolyLine(3, from, to);
            Vector2 v0 = from - to;
            v0 *= 10 / v0.magnitude;
            Vector2 v1 = new Vector2(v0.x * 0.866f - v0.y * 0.5f, v0.x * 0.5f + v0.y * 0.866f);
            Vector2 v2 = new Vector2(v0.x * 0.866f + v0.y * 0.5f, v0.x * -0.5f + v0.y * 0.866f); ;
            Handles.DrawAAPolyLine(3, to + v1, to, to + v2);
            Handles.EndGUI();
        }

        /// <summary>
        /// 设置基础的控件
        /// </summary>
        /// <param name="width"></param>
        /// <param name="enable"></param>
        /// <param name="value"></param>
        /// <param name="setValue"></param>
        public static void DrawBaseControl(bool enable, object value, Action<object> setValue)
        {
            if (value is Enum)
            {
                if (enable)
                {
                    value = EditorGUILayout.EnumPopup(value as Enum);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.LabelField((value as Enum).ToString());
                }
            }
            else if (value is Bounds)
            {
                if (enable)
                {
                    value = EditorGUILayout.BoundsField((Bounds)value);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.BoundsField((Bounds)value);
                }
            }
            else if (value is Color)
            {
                if (enable)
                {
                    value = EditorGUILayout.ColorField((Color)value);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.ColorField((Color)value);
                }
            }
            else if (value is AnimationCurve)
            {
                if (enable)
                {
                    value = EditorGUILayout.CurveField((AnimationCurve)value);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.CurveField((AnimationCurve)value);
                }
            }
            else if (value is string)
            {
                if (enable)
                {
                    value = EditorGUILayout.TextField(value as string);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.LabelField(value as string);
                }
            }
            else if (value is float)
            {
                if (enable)
                {
                    value = EditorGUILayout.FloatField((float)value);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.LabelField(value as string);
                }
            }
            else if (value is double)
            {
                if (enable)
                {
                    value = EditorGUILayout.DoubleField((double)value);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.LabelField(value as string);
                }
            }
            else if (value is int)
            {
                if (enable)
                {
                    value = EditorGUILayout.IntField((int)value);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.LabelField(value as string);
                }
            }
            else if (value is long)
            {
                if (enable)
                {
                    value = EditorGUILayout.LongField((long)value);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.LabelField(value as string);
                }
            }
            else if (value is bool)
            {
                if (enable)
                {
                    value = EditorGUILayout.Toggle((bool)value);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.LabelField(((bool)value).ToString());
                }
            }
            else if (value is Vector2)
            {
                if (enable)
                {
                    value = EditorGUILayout.Vector2Field("", (Vector2)value);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.Vector2Field("", (Vector2)value);
                }
            }
            else if (value is Vector3)
            {
                if (enable)
                {
                    value = EditorGUILayout.Vector3Field("", (Vector3)value);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.Vector3Field("", (Vector3)value);
                }
            }
            else if (value is Vector4)
            {
                if (enable)
                {
                    value = EditorGUILayout.Vector4Field("", (Vector4)value);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.Vector4Field("", (Vector4)value);
                }
            }
            else if (value is Rect)
            {
                if (enable)
                {
                    value = EditorGUILayout.RectField("", (Rect)value);
                    setValue(value);
                }
                else
                {
                    EditorGUILayout.RectField("", (Rect)value);
                }
            }
			else if(value is UnityEngine.Object)
			{
				if (enable)
				{
					value = EditorGUILayout.ObjectField((UnityEngine.Object)value, value.GetType(), true);
					setValue(value);
				}
				else
				{
					EditorGUILayout.ObjectField((UnityEngine.Object)value, value.GetType(), true);
				}
			}
        }
    }
}