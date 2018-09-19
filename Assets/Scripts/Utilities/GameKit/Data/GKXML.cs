using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using GKBase;

namespace GKData
{
    public class GKXML : GKSingleton<GKXML>
    {
        // 数据对象转换xml字符串.
        public string SerializeObject(object pObject, System.Type ty)
        {
            string XmlizedString = null;
            MemoryStream memoryStream = new MemoryStream();
            XmlSerializer xs = new XmlSerializer(ty);
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            xs.Serialize(xmlTextWriter, pObject);
            memoryStream = (MemoryStream)xmlTextWriter.BaseStream;
            XmlizedString = UTF8ByteArrayToString(memoryStream.ToArray());
            return XmlizedString;
        }

        // xml字符串转换数据对象.
        public object DeserializeObject(string pXmlizedString, System.Type ty)
        {
            XmlSerializer xs = new XmlSerializer(ty);
            MemoryStream memoryStream = new MemoryStream(StringToUTF8ByteArray(pXmlizedString));
            XmlTextWriter xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8);
            return xs.Deserialize(memoryStream);
        }
        // UTF8字节数组转字符串.
        public string UTF8ByteArrayToString(byte[] characters)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            string constructedString = encoding.GetString(characters);
            return (constructedString);
        }

        // 字符串转UTF8字节数组.
        public byte[] StringToUTF8ByteArray(string pXmlString)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            byte[] byteArray = encoding.GetBytes(pXmlString);
            return byteArray;
        }
    }
}