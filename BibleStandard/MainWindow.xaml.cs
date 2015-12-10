using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using FirstFloor.ModernUI.Presentation;
using System.IO;
using Inri.Common;


namespace BibleStandard
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        //設定ファイル
        string _appearanceSettingsPath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();

            __createSaveFolder();

            string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            _appearanceSettingsPath = System.IO.Path.Combine(dir,"save", "AppearanceSettings.xml");

            AppearanceSettings.Load(_appearanceSettingsPath);

            AppearanceManager.Current.ThemeSource = new Uri(AppearanceSettings.Instance.ThemeSource, UriKind.Relative);
            AppearanceManager.Current.FontSize = AppearanceSettings.Instance.FontSize;
            AppearanceManager.Current.AccentColor = AppearanceSettings.Instance.AccentColor;

            //this.Closed += MainWindow_Closed;

        }

        /// <summary>
        /// アプリケーションの状態保存用のフォルダを作成する
        /// </summary>
        private void __createSaveFolder()
        {
            string saveFolder = System.IO.Path.Combine(StaticTools.GetAssembyPath(), "save");
            if (System.IO.Directory.Exists(saveFolder) == false)
            {
                Directory.CreateDirectory(saveFolder);
            }
        }

        //void MainWindow_Closed(object sender, EventArgs e)
        //{
        //    AppearanceSettings.Save(_appearanceSettingsPath);
        //}


        /// <summary>
        /// Windowを閉じる直前のイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ModernWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Windownの位置とサイズを記憶
            if(WindowState == System.Windows.WindowState.Normal)
            {
                Properties.Settings.Default.Main_Width = Width;
                Properties.Settings.Default.Main_Height = Height;
                Properties.Settings.Default.Main_Top = Top;
                Properties.Settings.Default.Main_Left = Left;
                Properties.Settings.Default.Save();


            }

            //Settingの変更を記憶
            AppearanceSettings.Save(_appearanceSettingsPath);

        }

    }

 
}
