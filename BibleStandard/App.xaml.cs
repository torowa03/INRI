using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BibleStandard
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 現在のAppクラスのインスタンスを取得する
        /// </summary>
        public static new App Current
        {
            get { return Application.Current as App; }
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // モダンな見た目にするために、ここで呼び出しておく。
            System.Windows.Forms.Application.EnableVisualStyles();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //スプラッシュを３秒表示
            System.Threading.Thread.Sleep(2000);

            base.OnStartup(e);
        }
    }
}
