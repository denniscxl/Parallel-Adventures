using UnityEngine;
using System.IO;
using System.Collections.Generic;
using GKBase;

namespace GKFile
{
    public class GKFileUtil
    {
        static public void CreateDirectoryFromFileName(string filename)
        {
            var path = System.IO.Path.GetDirectoryName(filename);
            CreateDirectory(path);
        }

        static public void CreateDirectory(string path)
        {
            if (!string.IsNullOrEmpty(path) && !Directory.Exists(path))
            {
                //			Debug.Log ("CreateDirectory  Path: " + path);
                System.IO.Directory.CreateDirectory(path);
            }
        }

        static public void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            // If the destination directory doesn't exist, create it. 
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (var file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);
            }

            // If copying subdirectories, copy them and their contents to new location. 
            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectory(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        public static string[] GetFilesInDirectory(string filename)
        {
            var list = new List<string>();
            _GetFilesInDirectory(filename, ref list);
            return list.ToArray();
        }

        static void _GetFilesInDirectory(string filename, ref List<string> list)
        {
            if (System.IO.Directory.Exists(filename))
            {
                foreach (var f in System.IO.Directory.GetFiles(filename))
                {
                    list.Add(f.Replace('\\', '/'));
                }

                foreach (var f in System.IO.Directory.GetDirectories(filename))
                {
                    _GetFilesInDirectory(f, ref list);
                }
            }

            if (System.IO.File.Exists(filename))
            {
                list.Add(filename.Replace('\\', '/'));
                return;
            }
        }

        public static System.Int64 CompareTimeStamp(string fileA, string fileB)
        {
            var fa = new System.IO.FileInfo(fileA);
            var fb = new System.IO.FileInfo(fileB);
            /*		
                    Debug.Log( 	fileA + " " + fa.LastWriteTimeUtc.Ticks + "\n"
                            +	fileB + " " + fb.LastWriteTimeUtc.Ticks );
            */
            return fb.LastWriteTimeUtc.Ticks - fa.LastWriteTimeUtc.Ticks;
        }

        public static bool DeleteFile(string path)
        {
            if (!System.IO.File.Exists(path)) return false;
            //		Debug.Log (string.Format("DeleteFile path: {0}", path));
            System.IO.File.SetAttributes(path, FileAttributes.Normal); //remove readonly
            System.IO.File.Delete(path);
            return true;
        }

        public static bool DeleteDirectory(string path)
        {
            if (!_DeleteSubDirectory(path))
                return false;
            if (!_DeleteDirectory(path))
                return false;
            return true;
        }

        public static bool _DeleteDirectory(string path)
        {
            if (!System.IO.Directory.Exists(path)) return false;
            System.IO.Directory.Delete(path);
            return true;
        }

        public static bool _DeleteSubDirectory(string path)
        {
            if (!Directory.Exists(path))
                return false;

            try
            {
                DirectoryInfo dir = new DirectoryInfo(path);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //返回目录中所有文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)            //判断是否文件夹
                    {
                        DirectoryInfo subdir = new DirectoryInfo(i.FullName);
                        subdir.Delete(true);          //删除子目录和文件
                    }
                    else
                    {
                        File.Delete(i.FullName);      //删除指定文件
                    }
                }
            }
            catch (System.Exception e)
            {
                throw;
            }

            return true;
        }

        public static string RelativeAssetPath(string path, string relativeTo)
        {
            path = path.Replace('\\', '/');

            if (path.StartsWith("/"))
            {
                path = "Assets" + path;
            }
            else
            {
                path = System.IO.Path.GetDirectoryName(relativeTo) + "/" + path;
            }

            path = System.IO.Path.GetFullPath(path).Replace('\\', '/');
            var curDir = System.IO.Directory.GetCurrentDirectory().Replace('\\', '/');
            path = GKString.GetFromPrefix(path, curDir + "/", true);

            return path;
        }

        // Move same rename.
        static public void MoveFile(string src, string dest)
        {
            System.IO.File.Move(src, dest);
            System.IO.File.Move(src + ".meta", dest + ".meta");
        }

        // Get file path. level = 1, A/B/C -> A/B. Same(../ * lv).
        static public string GetDirectoryByLastIndex(string directory, int lv)
        {
            string val = "";

            // If xxx/xxx/, remove last '/'.
            directory = RemoveLastSlash(directory);

            string[] array = directory.Split('/');
            for (int i = 0; i < array.Length - lv; i++)
            {
                val += string.Format("{0}/", array[i]);
            }

            Debug.Log(string.Format("[MoveFile] targetPath: {0}", val));

            return val;
        }

        static public string GetDirctoryName(string path)
        {
            path = RemoveLastSlash(path);
            string[] array = path.Split('/');
            if (array.Length > 2)
                return array[array.Length - 2];
            return "";
        }

        static public bool FilterInvalidFiles(string path, bool isUsingMeta = true)
        {
            string extension = System.IO.Path.GetExtension(path);

            if (extension.Contains("DS_Store") || extension.Contains("manifest"))
                return false;

            if (isUsingMeta && extension.Contains("meta"))
                return false;

            return true;
        }

        static public bool IsTexture(string path)
        {
            string extension = System.IO.Path.GetExtension(path);

            if (extension.Contains("png") || extension.Contains("jpg"))
                return true;

            return false;
        }

        static public bool IsPrefab(string path)
        {
            string extension = System.IO.Path.GetExtension(path);

            if (extension.Contains("prefab"))
                return true;

            return false;
        }

        static public string RemoveLastSlash(string path)
        {
            string dir = path;
            if (0 == path.LastIndexOf('/'))
                dir = path.Substring(0, path.Length - 1);
            return dir;
        }

        static public string GetPath(string key, string path, bool needKey = true, bool needSuffix = true)
        {
            int idx = path.IndexOf(key + "/");
            string p = path.Substring(idx + key.Length + 1);

            if (needKey)
                p = string.Format("{0}/{1}", key, p);
            else
                p = string.Format("{0}", p);

            if (!needSuffix)
            {
                string[] suffixArray = p.Split('.');
                p = suffixArray[0];
            }

            return p;
        }
        static public string GetAssetPath(string path)
        {
            return GetPath("Assets", path);
        }
        static public string GetResourcesPath(string path)
        {
            return GetPath("Resources", path, false, false);
        }

        static public string GetPreviousFolderPath(string path)
        {
            if (string.IsNullOrEmpty(path))
                return path;
            var folders = path.Split('/');
            int idx = 1;
            if (string.IsNullOrEmpty(folders[folders.Length - 1]))
                idx++;
            if (folders.Length < idx + 1)
                return path;
            string t = path.Replace(string.Format("{0}/", folders[folders.Length - idx]), "");
            return t;
        }
    }
}