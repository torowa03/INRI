using System;

namespace Inri.Common
{
    /// <summary>
    /// シリアライズ対象クラスのひな形
    /// </summary>
    [Serializable]
    public class Settings
    {
        public static Settings Instance { get; set; }

        public static Settings Load(string path)
        {
            Settings ret = null;

            try
            {
                ret = path.ReadXml<Settings>();
            }
            catch (Exception ex)
            {
                ret = new Settings();

                CommonApp.ExceptionLog(ex);
            }

            Instance = ret;

            return ret;
        }

        public static void Save(string path)
        {
            try
            {
                Instance.WriteXml(path);
            }
            catch (Exception ex)
            {
                CommonApp.ExceptionLog(ex);
            }
        }


    }
}