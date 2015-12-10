using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

//using BibleLite.Common;
//using Utility;
using Inri.Common;

namespace BibleStandard.Model
{
    /// <summary>  
    /// ViewerWindowのモデルクラス  
    /// </summary>  
    public class PictureViewerModel : INotifyPropertyChanged
    {
        #region コンストラクタ
        public PictureViewerModel()
        {
            // OnPropertyChangedでnullチェックするのがめんどいので  
            // 空の処理をあらかじめ１つ追加しておく。  
            PropertyChanged += (sender, e) => { };
        }
        #endregion

        #region INotifyPropertyChanged メンバ

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region プロパティ
        private List<ImageInfo> _images;
        /// <summary>  
        /// ビューワーで表示する画像の情報を取得または設定します。  
        /// </summary>  
        public List<ImageInfo> Images
        {
            get
            {
                return _images;
            }
            set
            {
                _images = value;
                OnPropertyChanged("Images");
            }
        }
        #endregion

        private string[] _supportExts = { ".jpg", ".bmp", ".png", ".tiff", ".gif" };
        /// <summary>
        /// アプリケーションでサポートするファイルの拡張子を取得する。
        /// </summary>
        public string[] SupportExts
        {
            get { return _supportExts; }
        }

        #region 公開メソッド
        //public void OpenDirectory()
        //{
        //    using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
        //    {
        //        if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
        //        {
        //            // OK以外は何もしない  
        //            return;
        //        }
        //        // Imagesプロパティを、選択された画像のリストに更新する  
        //        this.Images = ImageUtils.GetImages(
        //            dialog.SelectedPath, App.Current.SupportExts);
        //    }
        //}

        public void LoadImageList()
        {
            string path = System.IO.Path.Combine(StaticTools.GetAssembyPath(), "Picture");
            // Imagesプロパティを、選択された画像のリストに更新する  
            this.Images = ImageUtils.GetImages(
                path, SupportExts);

        }


        public void AddImage(string[] files)
        {
            this.Images = ImageUtils.GetImages(files, SupportExts);

        }

        #endregion  
    }  
}
