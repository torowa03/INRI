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
using DBUtility;
using BibleStandard.Parts;

namespace BibleStandard.Pages
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl
    {
        public Home()
        {
            InitializeComponent();

            BibleParagraph para = DBAccess.RandomScripture();
            Label lb = new Label();
            lb.Content = string.Format("{0} {1}章", para.BookName, para.Chapter);
            lb.FontSize = 30;
            lb.FontWeight = FontWeights.Bold;
            spMain.Children.Add(lb);

            ScriptureBox sb = new ScriptureBox();
            sb.No = para.Section;
            sb.Scripture = para.Paragraph;
            sb.SelectMode = false;
            spMain.Children.Add(sb);

        }
    }
}
