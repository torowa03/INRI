using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using System.Xml;
using System.Windows.Threading;
using System.Reflection;

namespace Inri.Common
{

    public static partial class StaticTools
    {
        #region フィールドとプロパティ

        #endregion

        public static void DoEvents()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                new DispatcherOperationCallback(ExitFrames), frame);
            Dispatcher.PushFrame(frame);
        }

        public static object ExitFrames(object f)
        {
            ((DispatcherFrame)f).Continue = false;

            return null;
        }

        public static string GetAssembyPath()
        {
            Assembly myAssembly = Assembly.GetEntryAssembly();
            string path = System.IO.Path.GetDirectoryName(myAssembly.Location);
            return path;
        }

        //public static void SetComboItemSource(ComboBox cb, List<KeyValuePair<string, string>> kvs)
        //{
        //    cb.ItemsSource = kvs;
        //    cb.DisplayMemberPath = "Key";
        //    cb.SelectedValuePath = "Value";
        //    //cb.SelectedIndex = 0;
        //}


    }
}
