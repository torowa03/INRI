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
    /// ScriptureBox.xaml の相互作用ロジック
    /// </summary>
    public partial class ScriptureBox : UserControl
    {
        /// <summary>
        /// 番号
        /// </summary>
        public string No
        {
            set
            {
                lblNo.Content = value;
            }
            get
            {
                return lblNo.Content.ToString();
            }
        }

        /// <summary>
        /// 文章
        /// </summary>
        public string Scripture
        {
            set
            {
                txtScripture.Text = value;
            }
            get
            {
                return txtScripture.Text;
            }
        }

        ///// <summary>
        ///// 背景色
        ///// </summary>
        //public Brush Background
        //{
        //    set
        //    {
        //        this.Background = value;
        //    }
        //    get
        //    {
        //        return this.Background;
        //    }
        //}

        /// <summary>
        /// ハイライトカラー
        /// </summary>
        public Brush HighlightBackground
        {
            set
            {
                txtScripture.HighlightBackground = value;
            }
            get
            {
                return txtScripture.HighlightBackground;
            }

        }

        /// <summary>
        /// ハイライトを行うか
        /// </summary>
        public bool IsHighlight
        {
            set
            {
                txtScripture.IsHighlight = value;
            }
            get
            {
                return txtScripture.IsHighlight;
            }
        }

        /// <summary>
        /// 大文字でマッチング
        /// </summary>
        public bool IsMatchCase
        {
            set
            {
                txtScripture.IsMatchCase = value;
            }
            get
            {
                return txtScripture.IsMatchCase;
            }
        }

        /// <summary>
        /// 検索ワード
        /// </summary>
        public string SearchText
        {
            set
            {
                txtScripture.SearchText = value;
            }
            get
            {
                return txtScripture.SearchText;
            }
        }

        /// <summary>
        /// 選択可能
        /// </summary>
        private bool _selectMode = true;
        public bool SelectMode
        {
            get
            {
                return _selectMode;
            }
            set
            {
                _selectMode = value;
                if(_selectMode == true)
                {
                    txtScripture.MouseEnter += txtScripture_MouseEnter;
                    txtScripture.MouseLeave += txtScripture_MouseLeave;
                }else
                {
                    txtScripture.MouseEnter -= txtScripture_MouseEnter;
                    txtScripture.MouseLeave -= txtScripture_MouseLeave;
                }
            }
        }

        /// <summary>
        /// 選択状態
        /// </summary>
        private bool _isSelected = false;
        public bool Selected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                __setBackColor(_isSelected);
            }
        }

        private void __setBackColor(bool sel)
        {
            if (sel == true)
            {
                txtScripture.Background = new SolidColorBrush(Colors.Cyan);
            }
            else
            {
                txtScripture.Background = _defaultBackColor;
            }
        }


        /// <summary>
        /// コントロールの高さ
        /// </summary>
        /// <returns></returns>
        public double ScriptureHeight()
        {
            return txtScripture.Height;
        }

        /// <summary>
        /// 高さ自動調整
        /// </summary>
        public void AdjastHeight()
        {
            //this.Height = txtScripture.Height;
        }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public ScriptureBox()
        {
            InitializeComponent();

            _defaultBackColor = txtScripture.Background;
        }

        private Brush _backColor;
        private Brush _defaultBackColor;

        /// <summary>
        /// マウスオーバーイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtScripture_MouseEnter(object sender, MouseEventArgs e)
        {
            //if (SelectMode == false) return;

            _backColor = txtScripture.Background;
            txtScripture.Background = new SolidColorBrush(Colors.Cyan);
        }

        /// <summary>
        /// マウスリーブイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtScripture_MouseLeave(object sender, MouseEventArgs e)
        {
            //if (SelectMode == false) return;

            __setBackColor(_isSelected);
        }

        /// <summary>
        /// マウスダブルクリックイベント
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtScripture_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (SelectMode == false) return;

            //ダブルクリックのイベントがないのでカウントで判断する
            if(e.ClickCount == 2)
            {
                Selected = !Selected;
            }
        }
    }
}
