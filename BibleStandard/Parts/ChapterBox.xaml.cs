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

using Inri.Common;
using Inri.Controls;

namespace BibleStandard.Parts
{
    /// <summary>
    /// ChapterBox.xaml の相互作用ロジック
    /// </summary>
    public partial class ChapterBox : UserControl
    {
        /// <summary>
        /// 聖書の書名
        /// </summary>
        public string Title
        {
            set
            {
                expander1.Header = value;
            }
            get
            {
                return expander1.Header.ToString();
            }
        }

        /// <summary>
        /// クリア
        /// </summary>
        public void Clear()
        {
            spChapter.Children.Clear();
        }

        /// <summary>
        /// 章を追加
        /// </summary>
        /// <param name="chapter">章</param>
        public void AddChapter(string chapter)
        {
            Label lb = new Label();
            lb.Content = chapter;
            lb.FontSize = 22;
            lb.FontWeight = FontWeights.Bold;

            spChapter.Children.Add(lb);
            lb.Width = spChapter.Width;
        }

        /// <summary>
        /// 検索キーワードをハイライト
        /// </summary>
        public bool IsKeyword { get; set; }

        /// <summary>
        /// 節を追加
        /// </summary>
        /// <param name="no"></param>
        /// <param name="sctipture"></param>
        /// <param name="searchWord"></param>
        public void AddScripture(string no, string sctipture, string searchWord)
        {
            ScriptureBox sb = new ScriptureBox();
            sb.No = no;
            sb.Scripture = sctipture.Replace("\r", "");
            sb.HighlightBackground = new SolidColorBrush() { Color = Colors.Cyan };
            if(this.IsKeyword == true)
            {
                sb.IsHighlight = true;
                sb.IsMatchCase = false;
                sb.SearchText = searchWord;
                sb.Width = spChapter.Width;
                sb.SelectMode = false;
            }

            //TextBlock sb = new TextBlock();
            //sb.Width = spChapter.Width;
            //sb.Text = string.Format("{0}:{1}",no,sctipture.Replace("\r", ""));

            spChapter.Children.Add(sb);
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ChapterBox()
        {
            InitializeComponent();

            //IsKeyword = false;

        }

        /// <summary>
        /// 選択された節をクリップボードにコピー
        /// </summary>
        public string GetText()
        {
            StringBuilder selected = new StringBuilder();
            Dictionary<string, string> reference = new Dictionary<string, string>();
            List<string> numbers = new List<string>();
            string scripts=string.Empty;
            selected.AppendLine(Title);
            int chapter = 0;
            foreach (var obj in spChapter.Children)
            {
                if(obj.GetType() == typeof(Label))
                {
                    if (chapter > 0)
                    {
                        scripts = string.Format("{0}節", string.Join(",", numbers.ToArray()));
                        reference.Add(string.Format("#range{0}#", chapter), scripts);
                        numbers.Clear();
                    }

                    Label lb = obj as Label;

                    chapter++;
                    selected.AppendLine(lb.Content.ToString() + string.Format(" #range{0}#",chapter));
                }

                if (obj.GetType() == typeof(ScriptureBox))
                {
                    ScriptureBox sb = obj as ScriptureBox;

                    string script = string.Format("{0} {1}", sb.No, sb.Scripture);
                    selected.AppendLine(script);
                    numbers.Add(sb.No);
                }
            }

            scripts = string.Format("{0}節", string.Join(",", numbers.ToArray()));
            reference.Add(string.Format("#range{0}#", chapter), scripts);

            string text = selected.ToString();
            foreach (string key in reference.Keys)
            {
                text = text.Replace(key, reference[key]);
            }

            return text;
        }

    }
}
