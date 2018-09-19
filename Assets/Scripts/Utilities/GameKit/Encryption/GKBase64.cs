using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using GKBase;


namespace GKEncryption
{
    public class GKBase64 : GKSingleton<GKBase64>
    {
        /// 创建文本文件  
        public void CreateTextFile(string fileName, string strFileData, bool isEncryption)
        {
            //写文件流.
            StreamWriter writer;
            string strWriteFileData;
            //是否加密处理.
            if (isEncryption)
            {
                strWriteFileData = Encrypt(strFileData);
            }
            else
            {
                //写入的文件数据.
                strWriteFileData = strFileData;
            }

            writer = File.CreateText(fileName);
            writer.Write(strWriteFileData);
            //关闭文件流.
            writer.Close();
        }


        /// 读取文本文件.  
        public string LoadTextFile(string fileName, bool isEncryption)
        {
            //读文件流.
            StreamReader sReader;
            //读出的数据字符串.
            string dataString;

            sReader = File.OpenText(fileName);
            dataString = sReader.ReadToEnd();
            //关闭读文件流.
            sReader.Close();
            //是否解密处理.
            if (isEncryption)
            {
                return Decrypt(dataString);
            }
            else
            {
                return dataString;
            }
        }
        /// 加密方法  
        /// 描述： 加密和解密采用相同的key,具体值自己填，但是必须为32位.
        public string Encrypt(string toE)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12348578902223367877723456789012");
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateEncryptor();
            byte[] toEncryptArray = UTF8Encoding.UTF8.GetBytes(toE);
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return Convert.ToBase64String(resultArray, 0, resultArray.Length);
        }

        /// 解密方法  
        /// 描述： 加密和解密采用相同的key,具体值自己填，但是必须为32位.
        public string Decrypt(string toD)
        {
            byte[] keyArray = UTF8Encoding.UTF8.GetBytes("12348578902223367877723456789012");
            RijndaelManaged rDel = new RijndaelManaged();
            rDel.Key = keyArray;
            rDel.Mode = CipherMode.ECB;
            rDel.Padding = PaddingMode.PKCS7;
            ICryptoTransform cTransform = rDel.CreateDecryptor();
            byte[] toEncryptArray = Convert.FromBase64String(toD);
            byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

            return UTF8Encoding.UTF8.GetString(resultArray);
        }
    }
}
