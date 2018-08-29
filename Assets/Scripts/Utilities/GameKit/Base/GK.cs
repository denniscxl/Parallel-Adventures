using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace GKBase
{
    public class GK
    {
        static public bool isGamePlaying
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorApplication.isPlaying;
#else
			return true;		
#endif
            }
        }

        static public T[] ReverseArray<T>(T[] arr)
        {
            if (arr == null) return null;
            var n = arr.Length;
            var o = new T[n];
            for (int i = 0; i < n; i++)
            {
                o[n - i - 1] = arr[i];
            }
            return o;
        }

        static public List<T> ReverseArray<T>(List<T> arr)
        {
            if (arr == null) return null;
            var n = arr.Count;
            var o = new List<T>(n);
            for (int i = 0; i < n; i++)
            {
                o.Add(arr[n - i - 1]);
            }
            return o;
        }

        static public void NewObjectArray<T>(ref T[] arr, int size) where T : new()
        {
            arr = NewObjectArray<T>(size);
        }

        static public T[] NewObjectArray<T>(int size) where T : new()
        {
            var o = new T[size];
            for (int i = 0; i < size; i++)
            {
                o[i] = new T();
            }
            return o;
        }

        static public T[,] NewObjectArray<T>(int width, int height) where T : new()
        {
            var o = new T[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    o[x, y] = new T();
                }
            }
            return o;
        }

        static public void SetArrayElement<T>(T[,] arr, T value)
        {
            for (int x = 0; x < arr.GetLength(0); ++x)
                for (int y = 0; y < arr.GetLength(1); ++y)
                    arr[x, y] = value;
        }

        static public T SafeGetElement<T>(T[] arr, int element)
        {
            if (arr == null) return default(T);
            if (element < 0 || element >= arr.Length) return default(T);
            return arr[element];
        }

        static public T SafeGetElement<T>(List<T> arr, int element)
        {
            if (arr == null) return default(T);
            if (element < 0 || element >= arr.Count) return default(T);
            return arr[element];
        }

        static public T SafeGetLastElement<T>(T[] arr, int element)
        {
            if (arr == null) return default(T);
            if (element < 0 || element >= arr.Length) return default(T);
            return arr[arr.Length - 1 - element];
        }

        static public T SafeGetLastElement<T>(List<T> arr, int element)
        {
            if (arr == null) return default(T);
            if (element < 0 || element >= arr.Count) return default(T);
            return arr[arr.Count - 1 - element];
        }

        static public void RemoveElementBySwapLast<T>(List<T> arr, int element)
        {
            if (arr == null) return;
            if (element < 0 || element >= arr.Count) return;
            var last = arr.Count - 1;

            if (element < last)
            {
                arr[element] = arr[last];
            }
            arr.RemoveAt(last);
        }

        static public void RemoveLastElement<T>(List<T> arr)
        {
            if (arr == null) return;
            if (arr.Count <= 0) return;
            arr.RemoveAt(arr.Count - 1);
        }

        static public int GetElementIdxByList<T>(T[] arr, T element)
        {
            if (arr == null) return -1;
            if (arr.Length <= 0) return -2;

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i].Equals(element))
                {
                    return i;
                }
            }

            return -4;
        }

        static public int GetElementIdxByList<T>(List<T> arr, T element)
        {
            if (arr == null) return -1;
            if (arr.Count <= 0) return -2;
            if (!arr.Contains(element)) return -3;

            for (int i = 0; i < arr.Count; i++)
            {
                if (arr[i].Equals(element))
                {
                    return i;
                }
            }

            return -4;
        }

        static public bool IsNullOfEmpty<T>(T[] arr)
        {
            return (arr == null || arr.Length == 0);
        }

        static public bool IsNullOfEmpty<T>(List<T> arr)
        {
            return (arr == null || arr.Count == 0);
        }

        static public void AddPtr(ref System.IntPtr p, int offset)
        {
            p = new System.IntPtr(p.ToInt64() + offset);
        }

        static public T TryLoadResource<T>(string path) where T : UnityEngine.Object
        {
            //Debug.Log("Load resource " + typeof(T).Name + " \"" + path + "\"");
            return Resources.Load(path, typeof(T)) as T;
        }

        static public T LoadResource<T>(string path) where T : UnityEngine.Object
        {
            //Debug.Log("Load resource " + typeof(T).Name + " \"" + path + "\"");
            var o = TryLoadResource<T>(path);
            if (o == null)
            {
                Debug.LogWarning("Cannot load resource " + typeof(T).Name + " \"" + path + "\"");
            }
            return o;
        }

        static public GameObject LoadGameObject(string path, bool objectUsingPrefabName = false)
        {
            var prefab = LoadResource<GameObject>(path);
            if (!prefab) return null;
            var obj = GameObject.Instantiate(prefab) as GameObject;
            if (objectUsingPrefabName && obj)
            {
                obj.name = prefab.name;
            }
            return obj;
        }

        static public GameObject TryLoadGameObject(string path)
        {
            var prefab = TryLoadResource<GameObject>(path);
            if (!prefab) return null;
            return GameObject.Instantiate(prefab) as GameObject;
        }

        static public GameObject LoadPrefab(string path) { return LoadResource<GameObject>(path); }
        static public Texture LoadTexture(string path) { return LoadResource<Texture>(path); }
        static public Texture2D LoadTexture2D(string path) { return LoadResource<Texture2D>(path); }
        static public Material LoadMaterial(string path) { return LoadResource<Material>(path); }
        static public AnimationClip LoadAnimClip(string path) { return LoadResource<AnimationClip>(path); }

        static public GameObject TryLoadPrefab(string path) { return TryLoadResource<GameObject>(path); }
        static public Texture TryLoadTexture(string path) { return TryLoadResource<Texture>(path); }
        static public Texture2D TryLoadTexture2D(string path) { return TryLoadResource<Texture2D>(path); }
        static public Material TryLoadMaterial(string path) { return TryLoadResource<Material>(path); }
        static public AnimationClip TryLoadAnimClip(string path) { return TryLoadResource<AnimationClip>(path); }

        static public void DestroyAllChildren(GameObject o) { DestroyAllChildren(o.transform); }
        static public void DestroyAllChildren(Transform t)
        {
            if (t.childCount == 0) return;
            var c = new Transform[t.childCount];

            int i = 0;
            foreach (Transform ct in t)
            {
                c[i] = ct;
                i++;
            }

            foreach (var p in c)
            {
                GK.Destroy(p.gameObject);
            }
        }
        static public void DestroyAllChildren2D(RectTransform t)
        {
            if (t.childCount == 0) return;
            var c = new RectTransform[t.childCount];

            int i = 0;
            foreach (RectTransform ct in t)
            {
                c[i] = ct;
                i++;
            }

            foreach (var p in c)
            {
                GK.Destroy(p.gameObject);
            }
        }

        static public bool HasBits<T>(T val, T bits) where T : struct
        {
            int v = (int)(object)val;
            int b = (int)(object)bits;
            return (v & b) == b;
        }

        static public void Swap<T>(ref T lhs, ref T rhs) { T temp = lhs; lhs = rhs; rhs = temp; }

        static public string Fullname(GameObject o)
        {
            if (o == null) return "null";
            if (o.transform.parent)
            {
                return Fullname(o.transform.parent.gameObject) + "/" + o.name;
            }
            else
            {
                return o.name;
            }
        }

        static public string Fullname(Component o)
        {
            if (o == null) return "null";
            return Fullname(o.gameObject) + "." + o.GetType().Name;
        }

        static public void SetPos(Transform t, Vector3 p)
        {
            if (t == null) return;
            if (t.position == p) return;    //avoid dirty transformation if nothing changed
            t.position = p;
        }

        static public void SetLocalPos(Transform t, Vector3 p)
        {
            if (t == null) return;
            if (t.localPosition == p) return;   //avoid dirty transformation if nothing changed
            t.localPosition = p;
        }

        static public void SetParent(GameObject t, GameObject parent, bool keepWorldPos)
        {
            if (!t) return;
            SetParent(t.transform, parent ? parent.transform : null, keepWorldPos);
        }

        static public void SetParent(Transform t, Transform parent, bool keepWorldPos)
        {
            if (!t) return;
            if (keepWorldPos)
            {
                t.transform.parent = parent;
            }
            else
            {
                var pos = t.localPosition;
                var rot = t.localRotation;
                var s = t.localScale;

                t.transform.SetParent(parent);

                t.localPosition = pos;
                t.localRotation = rot;
                t.localScale = s;
            }
        }

        static public GameObject FindChild(GameObject root, string name, bool ignoreCase = true, bool recursive = true)
        {
            if (!root) return null;
            foreach (Transform t in root.transform)
            {
                if (t.name.Equals(name, ignoreCase ? System.StringComparison.OrdinalIgnoreCase : System.StringComparison.Ordinal))
                {
                    return t.gameObject;
                }

                if (recursive)
                {
                    var o = FindChild(t.gameObject, name, ignoreCase, recursive);
                    if (o) return o;
                }
            }
            return null;
        }

        static public GameObject[] FindAllChild(GameObject root, string name, bool ignoreCase = true, bool recursive = true)
        {
            var outList = new List<GameObject>();
            _FindAllChildWithPrefix(ref outList, root, name, ignoreCase, recursive);
            return outList.ToArray();
        }

        static void _FindAllChild(ref List<GameObject> outList, GameObject root, string name, bool ignoreCase, bool recursive)
        {
            if (!root) return;
            foreach (Transform t in root.transform)
            {
                if (t.name.Equals(name, ignoreCase ? System.StringComparison.OrdinalIgnoreCase : System.StringComparison.Ordinal))
                {
                    outList.Add(t.gameObject);
                }

                if (recursive)
                {
                    _FindAllChild(ref outList, t.gameObject, name, ignoreCase, recursive);
                }
            }
        }

        static public void FindAllChild<T>(ref List<T> outList, GameObject root, bool recursive = true)
        {
            if (!root) return;
            foreach (Transform t in root.transform)
            {
                T target = t.gameObject.GetComponent<T>();
                if (null != target)
                {
                    outList.Add(target);
                }
                if (recursive)
                {
                    FindAllChild(ref outList, t.gameObject);
                }
            }
        }

        static public GameObject FindChildWithPrefix(GameObject root, string name, bool ignoreCase = true, bool recursive = true)
        {
            if (!root) return null;
            foreach (Transform t in root.transform)
            {
                if (t.name.StartsWith(name, ignoreCase, null))
                {
                    return t.gameObject;
                }

                if (recursive)
                {
                    var o = FindChild(t.gameObject, name, ignoreCase, recursive);
                    if (o) return o;
                }
            }
            return null;
        }

        static public GameObject FindChildWithSuffix(GameObject root, string name, bool ignoreCase = true, bool recursive = true)
        {
            if (!root) return null;
            foreach (Transform t in root.transform)
            {
                if (t.name.EndsWith(name, ignoreCase, null))
                {
                    return t.gameObject;
                }

                if (recursive)
                {
                    var o = FindChild(t.gameObject, name, ignoreCase, recursive);
                    if (o) return o;
                }
            }
            return null;
        }

        static public GameObject[] FindAllChildWithPrefix(GameObject root, string prefix, bool ignoreCase = true, bool recursive = true)
        {
            var outList = new List<GameObject>();
            _FindAllChildWithPrefix(ref outList, root, prefix, ignoreCase, recursive);
            return outList.ToArray();
        }

        static void _FindAllChildWithPrefix(ref List<GameObject> outList, GameObject root, string prefix, bool ignoreCase, bool recursive)
        {
            if (!root) return;
            foreach (Transform t in root.transform)
            {
                if (t.name.StartsWith(prefix, ignoreCase, null))
                {
                    outList.Add(t.gameObject);
                }

                if (recursive)
                {
                    _FindAllChildWithPrefix(ref outList, t.gameObject, prefix, ignoreCase, recursive);
                }
            }
        }

        static public T FindChildOfType<T>(GameObject root, bool recursive = true) where T : Component
        {
            if (!root) return null;
            foreach (Transform t in root.transform)
            {
                T c = t.gameObject.GetComponent<T>();
                if (c) return c;

                if (recursive)
                {
                    c = FindChildOfType<T>(t.gameObject, recursive);
                    if (c) return c;
                }
            }
            return null;
        }

        static public void PrintArray2D<T>(T[] arr, int dim1, int dim2)
        {
            int size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(T));
            var fmt = "{0:X" + size * 2 + "} ";
            var msg = "";
            for (int y = 0; y < dim2; ++y)
            {
                for (int x = 0; x < dim1; ++x)
                {
                    msg += string.Format(fmt, arr[x + y * dim1]);
                }
                msg += '\n';
            }
            Debug.Log(msg);
        }

        static public void Assert(bool expr, params string[] msg)
        {
            if (!expr)
            {
                string finalMsg = "";
                foreach (string s in msg)
                    finalMsg += s;
                Debug.LogError("ASSERT FAILED! Msg: " + finalMsg);
            }
        }

        static public void ResetLocalTransform(GameObject o) { ResetLocalTransform(o.transform); }

        static public void ResetLocalTransform(Transform t)
        {
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
            t.localScale = Vector3.one;
        }

        static public int LayerBits(params string[] name)
        {
            int m = 0;
            foreach (var p in name)
            {
                m |= 1 << LayerMask.NameToLayer(p);
            }
            return m;
        }

        static public int LayerId(string name)
        {
            return LayerMask.NameToLayer(name);
        }

        static public void SetLayerRecursively(GameObject o, string layer)
        {
            SetLayerRecursively(o, LayerMask.NameToLayer(layer));
        }

        static public void SetLayerRecursively(GameObject o, int layer)
        {
            if (!o) return;
            o.layer = layer;
            foreach (Transform t in o.transform)
            {
                SetLayerRecursively(t.gameObject, layer);
            }
        }

        static public Dictionary<string, GameObject> IterateTransformHierarchy(GameObject o, Dictionary<string, GameObject> objDict, bool ignoreCase)
        {
            if (!o) return null;
            if (null == objDict)
                objDict = new Dictionary<string, GameObject>();
            objDict.Add(ignoreCase ? o.name.ToUpper() : o.name, o);
            foreach (Transform t in o.transform)
            {
                IterateTransformHierarchy(t.gameObject, objDict, ignoreCase);
            }
            return objDict;
        }

        static public T GetOrAddComponent<T>(GameObject o) where T : Component
        {
            var c = o.GetComponent<T>();
            if (c) return c;
            return o.AddComponent<T>();
        }

        static public GameObject GetOrAddGameObject(GameObject o, string name)
        {
            if (!o) return null;
            var f = o.transform.Find(name);
            if (f) return f.gameObject;

            var c = new GameObject(name);
            c.transform.parent = o.transform;
            GK.ResetLocalTransform(c);
            return c;
        }

        static public void Destroy(UnityEngine.Object obj)
        {
            if (!obj) return;
            if (Application.isEditor && !Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(obj);
            }
            else
            {
                // NGUI require remove parent instantly
                var go = obj as GameObject;
                if (go)
                {
                    // Parent of RectTransform is being set with parent property. 
                    // Consider using the SetParent method instead, with the worldPositionStays argument set to false. 
                    // This will retain local orientation and scale rather than world orientation and scale, 
                    // which can prevent common UI scaling issues.
                    // go.transform.parent = null;
                }
                UnityEngine.Object.Destroy(obj);
            }
        }

        static public int EnumCount<T>() { return System.Enum.GetNames(typeof(T)).Length; }
        static public string[] EnumNames<T>() { return System.Enum.GetNames(typeof(T)); }
        static public T[] EnumValues<T>()
        {
            var arr = System.Enum.GetValues(typeof(T));
            var v = new T[arr.Length];
            for (int i = 0; i < arr.Length; i++)
            {
                v[i] = (T)arr.GetValue(i);
            }

            return v;
        }

        static public bool TryParseEnum<T>(out T o, string str, bool ignoreCase) where T : System.IConvertible, new()
        {
            try
            {
                o = (T)System.Enum.Parse(typeof(T), str, ignoreCase);
                return true;
            }
            catch
            {
                o = new T();
                return false;
            }
        }

        // Get Outer net ip, if use Network.player.ipAddress Function, ip is intranet ip.
        static public string GetOuterNetIP()
        {
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.Default;
            // Is vaild url.
            string response = client.UploadString("http://www.3322.org/dyndns/getip", "");
            return response.Trim();
        }

        // Compare date time.
        static public int CompareDateTime(string time1, string time2)
        {
            DateTime t1 = Convert.ToDateTime(time1);
            DateTime t2 = Convert.ToDateTime(time2);

            return DateTime.Compare(t1, t2);
        }
        static public int CompareDateTime(DateTime time1, string time2)
        {
            DateTime t2 = Convert.ToDateTime(time2);

            //		Debug.Log (string.Format("{0} | {1}", time1, t2));
            return DateTime.Compare(time1, t2);
        }

        /*
         * Rectangle recognition in picture.
         * A two pixel buffer is created. One buffer is the picture pixel buffer, and the other is the read flag buffer.
         * The pixels that have not been operated in the picture buffer have been read. 
         * If the pixel color value is not the default color. the element recognition logic is carried out 
         * and the operation of all the involved pixels in the recognition process is marked.
         * return: x left-up x, y left-up y, z width, w height.
         * */
        static public List<Vector4> RectangleRecognition(Texture2D tex)
        {
            List<Vector4> l = new List<Vector4>();
            bool[,] opt = new bool[tex.height, tex.width];
            bool finished = false;

            while (!finished)
            {
                var v = GetRectangleLTByColor(tex, ref opt);
                if (0.9 < v.z)
                {
                    int w = GetRectangleWidthByColor(tex, (int)v.x, (int)v.y);
                    int h = GetRectangleHeightByColor(tex, (int)v.x, (int)v.y);

                    for (int i = 0; i < h; i++)
                    {
                        for (int j = 0; j < w; j++)
                        {
                            opt[(int)v.y + i, (int)v.x + j] = true;
                        }
                    }

                    Vector4 v4 = new Vector4(v.x, v.y, w, h);
                    l.Add(v4);
                }
                else
                {
                    finished = true;
                }
            }

            return l;
        }

        // Get the left upper corner of the color based rectangle in the picture.
        static private Vector3 GetRectangleLTByColor(Texture2D tex, ref bool[,] array)
        {
            Vector3 v = new Vector3();

            for (int i = 0; i < tex.height; i++)
            {
                for (int j = 0; j < tex.width; j++)
                {
                    if (!array[i, j])
                    {
                        array[i, j] = true;
                        if (Color.white != tex.GetPixel(j, i))
                        {
                            v.x = j;
                            v.y = i;
                            v.z = 1;
                            return v;
                        }
                    }
                }
            }

            v.z = 0;
            return v;
        }

        // Gets the color based rectangle width in the picture.
        static private int GetRectangleWidthByColor(Texture2D tex, int widthIdx, int heightIdx)
        {
            for (int i = widthIdx; i < tex.width; i++)
            {
                Color c = tex.GetPixel(i, heightIdx);
                if (c == Color.white)
                {
                    return i - widthIdx;
                }
            }
            return tex.width - widthIdx;
        }

        // Gets the color based rectangle height in the picture.
        static private int GetRectangleHeightByColor(Texture2D tex, int widthIdx, int heightIdx)
        {
            for (int i = heightIdx; i < tex.height; i++)
            {
                Color c = tex.GetPixel(widthIdx, i);
                if (c == Color.white)
                {
                    return i - heightIdx;
                }
            }
            return tex.height - heightIdx;
        }

        public delegate void DelegateType();

        // Default get texture method.
        static public Texture2D GetTextureByName(string name)
        {
            return LoadResource<Texture2D>(string.Format("Textures/{0}", name));
        }

        // Default get sprite method.
        static public Sprite GetSpriteByName(string name)
        {
            return LoadResource<Sprite>(string.Format("Textures/{0}", name));
        }

        public static DateTime GetDateTimeFrom1970Ticks(long curSeconds)
        {
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            return dtStart.AddSeconds(curSeconds);
        }

        public static DateTime GetDateTime(long curSeconds)
        {
            return new DateTime(curSeconds);
        }

        public static bool ExistLocationInList(int x, int y, List<Vector2> list)
        {
            foreach (var l in list)
            {
                if ((int)l.x == x && (int)l.y == y)
                    return true;
            }
            return false;
        }

        public static int CalcPointDistance(int x1, int y1, int x2, int y2)
        {
            return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
        }

        public static void ShuffleByList<T>(ref List<T> list)
        {
            for (int i = 0; i < list.Count - 1; i++)
            {
                int rand = UnityEngine.Random.Range(i + 1, list.Count);
                var temp = list[rand];
                list[rand] = list[i];
                list[i] = temp;
            }
        }

        /// <summary>
        /// Clones the specified list.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="List">The list.</param>
        /// <returns>List{``0}.</returns>
        public static List<T> CloneList<T>(object List)
        {
            using (Stream objectStream = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(objectStream, List);
                objectStream.Seek(0, SeekOrigin.Begin);
                return formatter.Deserialize(objectStream) as List<T>;
            }
        }

        public static void ReplaceShader(List<Renderer> renders, Shader replaceShader, string condition)
        {
            if (null == renders)
                return;
            for (int i = 0; i < renders.Count; i++)
            {
                Renderer r = renders[i];
                if (null == r)
                    continue;
                Material[] mats = r.materials;
                for (int j = 0; j < mats.Length; j++)
                {
                    Material m = mats[j];
                    if (null != m && m.shader.name.StartsWith(condition))
                    {
                        m.shader = replaceShader;
                    }
                }
                r.materials = mats;
            }
        }

        public static void SetMatColor(List<Renderer> renders, Color color, GameObject gameObject, float rimPower = 0.5f)
        {
            if (null != renders && !ReferenceEquals(gameObject, null) && gameObject.activeSelf)
            {
                for (int i = 0; i < renders.Count; i++)
                {
                    try
                    {
                        Renderer r = renders[i];
                        if (ReferenceEquals(r, null))
                            continue;
                        SetMatColor(r, color, rimPower);
                    }
                    catch (Exception)
                    {

                    }
                }
            }
        }
        // 边缘色及边缘强度定义随Shader定义而不同. 但为保证复用率, 尽可能确保项目内统一.
        private static readonly int _rimColorID = Shader.PropertyToID("_RimColor");
        private static readonly int _rimPower = Shader.PropertyToID("_RimPower");
        private static readonly int _srcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int _dstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int _renderQueue = 3100;
        private const string RIMLIGHT = "RIMLIGHT";
        private const string TRANSPARENT = "TRANSPARENT";
        private const string PROJECTION_ON = "PROJECTION_ON";
        private const string ALPHAOFFSET = "_AlphaOffset";
        public static void SetMatColor(Renderer render, Color color, float rimPower = 0.5f)
        {
            if (null != render)
            {
                Material[] mats = render.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    Material m = mats[i];
                    if (ReferenceEquals(m, null))
                        continue;
                    if (m.HasProperty(_rimColorID))
                    {
                        Color c = m.GetColor(_rimColorID);
                        if (c != color)
                        {
                            m.SetColor(_rimColorID, color);
                            m.SetFloat(_rimPower, rimPower);
                            // 判断是否启用了着色器关键字.
                            if (rimPower > 0.5f)
                            {
                                if (!m.IsKeywordEnabled(RIMLIGHT))
                                {
                                    m.EnableKeyword(RIMLIGHT);
                                }
                            }
                            else
                            {
                                if (m.IsKeywordEnabled(RIMLIGHT))
                                {
                                    m.DisableKeyword(RIMLIGHT);
                                }
                            }
                        }
                    }
                }
                render.materials = mats;
            }
        }
        // 设置透明度.
        private static Dictionary<int, int> _alphaRenderQueue = new Dictionary<int, int>();
        public static void SetMatAlpha(float alpha, List<Renderer> renders)
        {
            if (null == renders)
                return;
            for (int i = 0; i < renders.Count; i++)
            {
                Renderer r = renders[i];
                if (null == r)
                    continue;
                Material[] mats = r.materials;
                if (null == mats)
                    continue;
                for (int j = 0; j < mats.Length; j++)
                {
                    Material m = mats[j];
                    if (null == m)
                        continue;
                    if (alpha > 0f)
                    {
                        m.EnableKeyword(TRANSPARENT);
                        m.DisableKeyword(PROJECTION_ON);
                        // 修改渲染队列至Alpha级.
                        int id = m.GetInstanceID();
                        if (!_alphaRenderQueue.ContainsKey(id) && _renderQueue != m.renderQueue)
                            _alphaRenderQueue[id] = m.renderQueue;
                        m.renderQueue = _renderQueue;
                        m.SetInt(_dstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcColor);
                        m.SetFloat(ALPHAOFFSET, alpha);
                    }
                    else
                    {
                        m.DisableKeyword(TRANSPARENT);
                        m.EnableKeyword(PROJECTION_ON);
                        int id = m.GetInstanceID();
                        if (_alphaRenderQueue.ContainsKey(id))
                            m.renderQueue = _alphaRenderQueue[id];
                        m.SetInt(_dstBlend, 0);
                        m.SetFloat(ALPHAOFFSET, alpha);
                    }
                }
            }
            if (alpha <= 0f)
            {
                _alphaRenderQueue.Clear();
            }
        }

        public static void FindControls<T>(Component comp, ref T controls) where T : new()
        {
            FindControls(comp.gameObject, ref controls);
        }

        public static void FindControls<T>(GameObject obj, ref T controls) where T : new()
        {
            if (controls == null)
            {
                controls = new T();
            }

            var type = controls.GetType();
            var fields = type.GetFields();
            foreach (var f in fields)
            {
                var attrs = f.GetCustomAttributes(typeof(System.NonSerializedAttribute), false);
                if (attrs.Length != 0) continue;
                f.GetCustomAttributes(typeof(System.NonSerializedAttribute), false);
                var w = GK.FindChild(obj, f.Name, true);

                if (w == null)
                {
                    Debug.LogError("Cannot find widget [" + f.Name + "] in " + GK.Fullname(obj));
                    f.SetValue(controls, null);
                    continue;
                }

                if (f.FieldType == typeof(GameObject))
                {
                    f.SetValue(controls, w);

                }
                else if (f.FieldType.IsSubclassOf(typeof(Component)))
                {
                    var c = w.GetComponent(f.FieldType);
                    f.SetValue(controls, c);
                }
            }
        }

        public static void CrossFadeColorInChildren(GameObject root, float alpha, float duration, bool ignoreTimeScale)
        {
        }

    }
}



