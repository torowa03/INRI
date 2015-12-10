using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Newtonsoft.Json;
using DBUtility;
using Data;
//using Utility;
using BibleStandard.Parts;
using System.ComponentModel;
using System.Windows.Threading;

using Inri.Common;
using Inri.Controls;

namespace BibleStandard.Pages.Search
{
    /// <summary>
    /// SearchForm.xaml の相互作用ロジック
    /// </summary>
    public partial class SearchPage5 : UserControl
    {
        private const string FOLDER = "Save";
        private const string FILENAME = "SWork5.xml";

        private ReadingFormSetting _setting = new ReadingFormSetting();

        delegate void SetControl(List<BibleParagraph> lst, string word);
        delegate void ClearControl();


        /// <summary>
        /// 状態保存ファイルパス取得
        /// </summary>
        /// <returns></returns>
        private string _getResumeFilePath()
        {
            return System.IO.Path.Combine(StaticTools.GetAssembyPath(), FOLDER, FILENAME);
        }

        private string _getWorkFilePath()
        {
            return System.IO.Path.Combine(StaticTools.GetAssembyPath(), FOLDER, "%WORK%" + FILENAME);
        }

        /// <summary>
        /// 状態の保存
        /// </summary>
        public void Save()
        {
            Inri.Common.Serialize.WriteXml<ReadingFormSetting>(_setting, _getResumeFilePath());

        }

        /// <summary>
        /// 状態の復元
        /// </summary>
        public void Load()
        {
            string fn = _getResumeFilePath();
            if (System.IO.File.Exists(fn) == false) return;
            _setting = Inri.Common.Serialize.ReadXml<ReadingFormSetting>(fn);

            txtWord.Text = _setting.word;
            btnSearch_Click(null, null);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SearchPage5()
        {
            InitializeComponent();

            #region バックグラウンドワーカーの設定
            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.DoWork += OnDoWork;
            _backgroundWorker.RunWorkerCompleted += OnRunWorkerCompleted;
            _backgroundWorker.ProgressChanged += backgroundWorker1_ProgressChanged;
            #endregion

            #region ワークファイルが残っていた場合
            if (System.IO.File.Exists(_getWorkFilePath()) == true)
            {
                System.IO.File.Copy(_getWorkFilePath(), _getResumeFilePath(), true);
                System.IO.File.Delete(_getWorkFilePath());
            }
            #endregion

            btnCopy.Click += btnCopy_Click;

            Load();

        }

        private readonly BackgroundWorker _backgroundWorker;

        /// <summary>
        /// バックグラウンドワーカーの完了
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            //画面の表示を戻す
            btnSearch.Content = "検索";
            busyAnimation.Visibility = System.Windows.Visibility.Hidden;
            progressRing.IsActive = false;

            __doWorkEndSet();//終了時にワークファイルを削除する　ワークファイルが残っていたら検索中に強制終了されたことがわかる

            //入力履歴を保存
            txtWord.AddInputHistory();
            _setting.word = txtWord.Text;
            Save();
        }

        /// <summary>
        /// バックグランドワーカーの処理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void OnDoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            long _LIMIT = Properties.Settings.Default.Search_limit;
            if (worker == _backgroundWorker)
            {
                List<BibleParagraph> lst = DBAccess.SearchParagraph(_searchText);
                _resultCount = lst.Count;
                if(lst.Count > _LIMIT)
                {
                    worker.CancelAsync();
                    Dispatcher.BeginInvoke(
                        DispatcherPriority.Normal,
                        new ClearControl(__labelClear)
                    );
                    return;
                }

                _backgroundWorker.ReportProgress(10);

                Dispatcher.BeginInvoke(
                    DispatcherPriority.Normal,
                    new SetControl(__textBlockSet),
                    lst,
                    _searchText
                );
            }
        }

        /// <summary>
        /// 検索結果の件数がオーバー
        /// </summary>
        private void __labelClear()
        {
            progressRing.IsActive = false;
            string msg = string.Format("検索結果の該当件数が多すぎます。{0}件。\n検索するキーワードを変更してください",_resultCount);
            FirstFloor.ModernUI.Windows.Controls.ModernDialog.ShowMessage(msg, "検索できません", MessageBoxButton.OK);

            lblMessage.Content = string.Empty;

        }

        /// <summary>
        /// 進捗の更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            // プログレスバーを進捗させる
            //pgbar1.Value = e.ProgressPercentage;
            lblMessage.Content = string.Format("結果 {0}件", _resultCount);

            System.Diagnostics.Debug.WriteLine("###");
 
        }

        /// <summary>
        /// 検索ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //btnSearch.Click -= btnSearch_Click;

                if (_backgroundWorker.IsBusy)
                {
                    _backgroundWorker.CancelAsync();
                    btnSearch.Content = "検索";
                    lblMessage.Content = string.Empty;
                    return;
                }
                else
                {
                    _searchText = txtWord.Text;
                    if (_searchText == string.Empty)
                    {
                        lblMessage.Content = string.Empty;
                        return;
                    }

                    __removeAll();
                    lblMessage.Content = "検索中です・・・・";
                    busyAnimation.Visibility = System.Windows.Visibility.Visible;

                    __doWorkStartSet();//開始時にフラグとしてワークファイルを作成する

                    _backgroundWorker.RunWorkerAsync();
                    btnSearch.Content = "ｷｬﾝｾﾙ";
                    progressRing.IsActive = true;
                }

                //入力履歴を保存
                //txtWord.AddInputHistory();

            }
            finally
            {
                //__doWorkEndSet();//終了時にワークファイルを削除する　ワークファイルが残っていたら検索中に強制終了されたことがわかる
                //btnSearch.Click += btnSearch_Click;
            }

 
        }

        /// <summary>
        /// 検索完了時にワークファイルを削除する
        /// </summary>
        private void __doWorkEndSet()
        {
            System.IO.File.Delete(_getWorkFilePath());
        }

        /// <summary>
        /// 検索開始時にワークファイルを作成する
        /// </summary>
        private void __doWorkStartSet()
        {
            if(System.IO.File.Exists(_getResumeFilePath()) == true)
            {
                System.IO.File.Copy(_getResumeFilePath(), _getWorkFilePath());
            }
        }

        /// <summary>
        /// 検索結果をクリア
        /// </summary>
        private void __removeAll()
        {
            stackPanel1.Children.Clear();

        }

        string _searchText = string.Empty;
        SearchableTextControl _bufText = new SearchableTextControl();
        long _resultCount = 0;
        string _resultText = string.Empty;

        /// <summary>
        /// 節を結合して１つの文字列にする
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        private  void __textBlockSet(List<BibleParagraph> lst, string searchWord)
        {
            string bufBook = string.Empty;
            string bufChapter = string.Empty;
            ChapterBox chpBox = new ChapterBox(); ;
            chpBox.IsKeyword = Properties.Settings.Default.Search_Highlight;
            foreach (BibleParagraph pg in lst)
            {
                if (bufBook != pg.BookName)
                {

                    if (bufBook != string.Empty)
                    {
                        stackPanel1.Children.Add(chpBox);
                        chpBox.Width = stackPanel1.Width;
                    }
                    chpBox = new ChapterBox();
                    chpBox.IsKeyword = Properties.Settings.Default.Search_Highlight;
                    chpBox.Title = pg.BookName;
                    bufBook = pg.BookName;
                    bufChapter = string.Empty;
                }

                if(bufChapter != pg.Chapter)
                {
                    bufChapter = pg.Chapter;
                    string cp = string.Format("{0}章", pg.Chapter);
                    chpBox.AddChapter(cp);

                }

                chpBox.AddScripture(pg.Section, pg.Paragraph, searchWord);

                StaticTools.DoEvents();
            }

            if (bufBook != string.Empty)
            {
                stackPanel1.Children.Add(chpBox);
                chpBox.Width = stackPanel1.Width;

            }
        }

        /// <summary>
        /// Copyボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            __selectedToClipboard();

        }

        /// <summary>
        /// 選択された節をクリップボードにコピー
        /// </summary>
        private void __selectedToClipboard()
        {
            StringBuilder selected = new StringBuilder();
            selected.AppendLine(string.Format("検索キーワード：{0}", _searchText));
            List<string> numbers = new List<string>();
            foreach (var obj in stackPanel1.Children)
            {
                if (obj.GetType() == typeof(ChapterBox))
                {
                    ChapterBox cb = obj as ChapterBox;
                    selected.AppendLine(cb.GetText());
                }
            }
            Clipboard.SetText(selected.ToString());
        }


    }
}
