using System.IO;
using System.Xml.Serialization;

namespace ZNSO.Notepad.Editor.Services
{
    public static class ZnpSerializer
    {
        public static void Save<T>(string filePath, T data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using FileStream stream = new FileStream(filePath, FileMode.Create);
            serializer.Serialize(stream, data);
        }

        public static T Load<T>(string filePath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            using FileStream stream = new FileStream(filePath, FileMode.Open);
            return (T)serializer.Deserialize(stream);
        }
    }
}