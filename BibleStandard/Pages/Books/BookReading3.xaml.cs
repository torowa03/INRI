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

using Data;
//using Utility;
using DBUtility;
using Newtonsoft.Json;
using BibleStandard.Parts;
using Inri.Common;
using Inri.Controls;

namespace BibleStandard.Pages.Books
{
    /// <summary>
    /// Interaction logic for BookReading.xaml
    /// </summary>
    public partial class BookReading3 : UserControl
    {
        private const string FOLDER = "Save";
        private const string FILENAME = "BWork3.xml";

        List<BibleBook> _bookList = new List<BibleBook>();
        private BookReadingResume _setting = new BookReadingResume();

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BookReading3()
        {
            InitializeComponent();

            btnCopy.Click += btnCopy_Click;

            _bookList = DBAccess.ReadBook();
            InitContentItemSource();

            Load();
        }

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
            Inri.Common.Serialize.WriteXml<BookReadingResume>(_setting, _getResumeFilePath());

        }

        /// <summary>
        /// 状態の復元
        /// </summary>
        public void Load()
        {
            string fn = _getResumeFilePath();
            if (System.IO.File.Exists(fn) == false) return;
            _setting = Inri.Common.Serialize.ReadXml<BookReadingResume>(fn);

            lstBooks.SelectedIndex = _setting.BookIndex;
            lstBooks.ScrollIntoView(lstBooks.SelectedItem);
            lstChapters.SelectedIndex = _setting.ChapterIndex;
            lstChapters.ScrollIntoView(lstChapters.SelectedItem);
        }

        /// <summary>
        /// 書物リストボックス
        /// </summary>
        private void InitContentItemSource()
        {
            List<KeyValuePair<string, string>> contentItemList = new List<KeyValuePair<string, string>>();
            foreach (BibleBook bb in _bookList)
            {
                string json = JsonConvert.SerializeObject(bb);
                contentItemList.Add(new KeyValuePair<string, string>(bb.BookName, json));
            }

            lstBooks.ItemsSource = contentItemList;
            lstBooks.DisplayMemberPath = "Key";
            lstBooks.SelectedValuePath = "Value";

        }

        /// <summary>
        /// 章リストボックス
        /// </summary>
        /// <param name="selectBook"></param>
        private void InitChapterItemSource(BibleBook selectBook)
        {
            int selected = lstChapters.SelectedIndex;

            List<KeyValuePair<string, string>> chapterItemList = new List<KeyValuePair<string, string>>();
            List<BibleChapter> lst = DBAccess.ReadChapter(selectBook.BookName);
            foreach (BibleChapter cp in lst)
            {
                chapterItemList.Add(new KeyValuePair<string, string>(cp.Chapter, cp.Url));
            }

            lstChapters.ItemsSource = chapterItemList;
            lstChapters.DisplayMemberPath = "Key";
            lstChapters.SelectedValuePath = "Value";

            //選択している章をそのまま
            if(lstChapters.Items.Count >= selected)
            {
                lstChapters.SelectedIndex = selected;
            }else
            {
                lstChapters.SelectedIndex = lstChapters.Items.Count - 1;
            }
        }

        /// <summary>
        /// 書巻の選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstBooks_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstBooks.SelectedIndex == -1) return;

            string json = lstBooks.SelectedValue.ToString();
            BibleBook selectBook = JsonConvert.DeserializeObject<BibleBook>(json);

            InitChapterItemSource(selectBook);
        }

        /// <summary>
        /// 節を結合して１つの文字列にする
        /// </summary>
        /// <param name="lst"></param>
        /// <returns></returns>
        private string _joinScripture(List<BibleParagraph> lst)
        {
            StringBuilder sb = new StringBuilder();
            foreach (BibleParagraph pg in lst)
            {
                sb.Append(string.Format("{0:D3} {1}", pg.Section, pg.Paragraph));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 節を追加
        /// </summary>
        /// <param name="lst"></param>
        private void _renderScripture(List<BibleParagraph> lst)
        {
            spArticle.Children.Clear();
            foreach (BibleParagraph pg in lst)
            {
                ScriptureBox sb = new ScriptureBox();
                sb.No = pg.Section;
                sb.Scripture = pg.Paragraph.Replace("\r","");
                spArticle.Children.Add(sb);
                sb.Height = sb.ScriptureHeight();
                
            }

        }

        /// <summary>
        /// 章選択
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstChapters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lstChapters.SelectedIndex == -1) return;

            string bookjson = lstBooks.SelectedValue.ToString();
            BibleBook selectBook = JsonConvert.DeserializeObject<BibleBook>(bookjson);

            string chapterjson = lstChapters.SelectedValue.ToString();
            KeyValuePair<string, string> kv = (KeyValuePair<string, string>)lstChapters.SelectedItem;
            string chapter = kv.Key;

            string url = lstChapters.SelectedValue.ToString();

            List<BibleParagraph> lst = DBAccess.ReadParagraph(selectBook.BookName, chapter);
            //string buf = _joinScripture(lst);
            //txtArticle.Text = buf;
            _renderScripture(lst);

            string title = string.Format("{0} {1}章", selectBook.BookName, chapter);
            _bookTitle.Content = title;

            _scrollViewR.ScrollToTop();

            //_setting.FotSizeIndex = cbFontSize.SelectedIndex;
            _setting.BookIndex = lstBooks.SelectedIndex;
            _setting.ChapterIndex = lstChapters.SelectedIndex;
            
            Save();
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
            List<string> numbers = new List<string>();
            selected.AppendLine(_bookTitle.Content.ToString() + " #range#"); //書名 #range#
            foreach (var obj in spArticle.Children)
            {
                if (obj.GetType() == typeof(ScriptureBox))
                {
                    ScriptureBox sb = obj as ScriptureBox;
                    if (sb.Selected == true)
                    {
                        string script = string.Format("{0} {1}", sb.No, sb.Scripture);
                        selected.AppendLine(script);
                        numbers.Add(sb.No);
                    }
                }
            }
            string text = selected.ToString();
            string scripts = string.Format("{0}節", string.Join(",", numbers.ToArray()));  //書名 1,2,3節
            text = text.Replace("#range#", scripts);
            Clipboard.SetText(text);
        }

        /// <summary>
        /// 全選択・全解除
        /// </summary>
        private bool _allSelected = false;
        private bool AllSelected
        {
            get
            {
                return _allSelected;
            }
            set
            {
                _allSelected = value;
                foreach (var obj in spArticle.Children)
                {
                    if (obj.GetType() == typeof(ScriptureBox))
                    {
                        ScriptureBox sb = obj as ScriptureBox;
                        sb.Selected = _allSelected;
                    }
                }
            }
        }

        /// <summary>
        /// キーイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            //クリップボードにコピー
            if(e.Key == Key.C && Keyboard.Modifiers == ModifierKeys.Control)
            {
                __selectedToClipboard();
            }

            //全選択・全解除
            if(e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
            {
                AllSelected = !AllSelected;

            }
        }
    }
}
