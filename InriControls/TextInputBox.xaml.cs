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

using System.Collections.ObjectModel;
using System.ComponentModel;

using Inri.Common;

namespace Inri.Controls
{
    /// <summary>
    /// TextInputBox.xaml の相互作用ロジック
    /// </summary>
    public partial class TextInputBox : UserControl
    {
        /// <summary>
        /// モデル
        /// </summary>
        AutoCompleteViewModel _viewModel;

        /// <summary>
        /// 入力したテキスト
        /// </summary>
        public string Text
        {
            get { return SearchBoxView.Text; }
            set { SearchBoxView.Text = value; }
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public TextInputBox()
        {
            InitializeComponent();
            SearchBoxView.Text = string.Empty;

            _viewModel = new AutoCompleteViewModel();

            //デザインモード時の処理
            //if((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            //{
            //    return;
            //}

            Load();

            DataContext = _viewModel;

        }

        /// <summary>
        /// 入力履歴追加
        /// </summary>
        public void AddInputHistory()
        {
            _viewModel.AddWord(SearchBoxView.Text);
            Save();
        }

        //入力履歴を保存するファイルとフォルダ
        private const string FOLDER = "Save";
        private const string FILENAME = "SearchHistory.xml";

        /// <summary>
        /// 状態保存ファイルパス取得
        /// </summary>
        /// <returns></returns>
        private string _getResumeFilePath()
        {
            return System.IO.Path.Combine(StaticTools.GetAssembyPath(), FOLDER, FILENAME);
        }

        /// <summary>
        /// 状態の保存
        /// </summary>
        public void Save()
        {
            Inri.Common.Serialize.WriteXml<AutoCompleteViewModel>(_viewModel, _getResumeFilePath());

        }

        /// <summary>
        /// 状態の復元
        /// </summary>
        public void Load()
        {
            string file = _getResumeFilePath();
            if (System.IO.File.Exists(file) == false) return;
            _viewModel = Inri.Common.Serialize.ReadXml<AutoCompleteViewModel>(file);

        }


        /// <summary>
        /// 検索ボタンクリック
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SearchBoxViewButtonClick(object sender, RoutedEventArgs e)
        {
            SearchBoxView.Text = string.Empty;
            SearchBoxView.Focus();

        }
    }

    /// <summary>
    /// オートコンプリートモデル
    /// </summary>
    public class AutoCompleteViewModel
    {
        private ObservableCollection<InputHistory> _inputHistory;
        public ObservableCollection<InputHistory> InputHistorySource
        {
            get
            {
                if (_inputHistory == null)
                {
                    _inputHistory = new ObservableCollection<InputHistory>();
                }
                return _inputHistory;
            }
        }

        /// <summary>
        /// フィルタ
        /// </summary>
        public AutoCompleteFilterPredicate<object> InputWordFilter
        {
            get
            {
                return (searchText, obj) => (obj as InputHistory).Word.Contains(searchText);
            }
        }

        /// <summary>
        /// 入力履歴に追加
        /// </summary>
        /// <param name="word"></param>
        public void AddWord(string word)
        {
            if (_inputHistory == null) return;

            //すでに登録済であれば追加しない
            int cnt = _inputHistory.Where(input => input.Word == word).Count();
            if (cnt > 0) return;

            _inputHistory.Add(new InputHistory() { Word = word, LastDate = DateTime.Now });
        }


    }

    /// <summary>
    /// 入力履歴クラス
    /// </summary>
    public class InputHistory
    {
        //入力された単語
        public string Word { get; set; }
        //登録日時
        public DateTime LastDate { get; set; }
    }


}
