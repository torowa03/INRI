using System.IO;
using System.Text;
using System.Xml.Serialization;


namespace Inri.Common
{
    public static class Serialize
    {
        public static void WriteXml<T>(this T o, string path) where T : new()
        {
            var serializer = new XmlSerializer(typeof(T));

            StreamWriter sw = null;

            try
            {
                sw = new StreamWriter(path, false, new UTF8Encoding(false));

                serializer.Serialize(sw, o);

            }
            finally
            {
                if (sw != null)
                {
                    sw.Close();
                    sw.Dispose();
                }
            }
        }

        public static T ReadXml<T>(this string path) where T : new()
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                throw new FileNotFoundException("ファイルが見つかりません。", path);
            }

            T ret = new T();

            var serializer = new XmlSerializer(typeof(T));

            StreamReader sr = null;

            try
            {
                sr = new StreamReader(path, new System.Text.UTF8Encoding(false));

                ret = (T)serializer.Deserialize(sr);

            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                    sr.Dispose();
                }
            }

            return ret;
        }


    }
}
