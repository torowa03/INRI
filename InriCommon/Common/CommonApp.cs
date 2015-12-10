using System;
//using System.Windows;

namespace Inri.Common
{
    public class CommonApp
    {
        public static void ExceptionLog(Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.Message);
            //MessageBox.Show(ex.ToString());
        }
    }
}
