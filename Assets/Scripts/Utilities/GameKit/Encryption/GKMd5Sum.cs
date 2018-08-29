using System;
using System.IO;
using System.Text;

namespace GKEncryption
{
    public class GKMd5Sum
    {
        public static string Calc(string input)
        {
            System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
            byte[] bytes = ue.GetBytes(input);

            // Encrypt bytes
#if UNITY_WP8
        System.Security.Cryptography.SHA1Managed md5 = new System.Security.Cryptography.SHA1Managed();
#else
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
#endif
            byte[] hashBytes = md5.ComputeHash(bytes);

            // Convert the encrypted bytes back to a string (base 16)
            string hashString = "";

            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }

            return hashString.PadLeft(32, '0');
        }




        public static string Calc(FileStream fileStream)
        {
            // Encrypt bytes
#if UNITY_WP8
        System.Security.Cryptography.SHA1Managed md5 = new System.Security.Cryptography.SHA1Managed();
#else
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
#endif
            byte[] hashBytes = md5.ComputeHash(fileStream);
            string hashString = "";

            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }

            return hashString.PadLeft(32, '0');
        }


        public static string GetMD5HashFromFile(string fileName)
        {
            try
            {
                FileStream reader = new FileStream(fileName, FileMode.Open);
                string md5Str = Calc(reader);
                reader.Close();
                return md5Str;
            }
            catch (Exception ex)
            {
                throw new Exception("GetMD5HashFromFile() fail,error:" + ex.Message);
            }
        }

        public static bool CompareTwoFileBinary(string filePath1, string filePath2)
        {
            string md51 = GetMD5HashFromFile(filePath1);
            string md52 = GetMD5HashFromFile(filePath2);
            return md51.Equals(md52);
        }
    }
}