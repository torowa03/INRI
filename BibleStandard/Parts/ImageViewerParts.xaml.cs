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

using BibleStandard.Model;
using System.ComponentModel;

namespace BibleStandard.Parts
{
    /// <summary>
    /// ImageViewerParts.xaml の相互作用ロジック
    /// </summary>
    public partial class ImageViewerParts : UserControl
    {

        public ImageViewerParts()
        {
            InitializeComponent();

            //デザインモード時の処理
            if ((bool)(DesignerProperties.IsInDesignModeProperty.GetMetadata(typeof(DependencyObject)).DefaultValue))
            {
                return;
            }

            Model.LoadImageList();
        }

        public PictureViewerModel Model
        {
            get { return DataContext as PictureViewerModel; }
        }

        //private void btnOpen_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Model.OpenDirectory();
        //}


    }

    public class ZoomImage : Image
    {
        private TransformGroup _transformGroup = new TransformGroup();
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.None)
            {
                ChangeScale(e.GetPosition(this), e.Delta);
            }
            base.OnMouseWheel(e);
        }
        private void ChangeScale(Point center, int delta)
        {
            double scale = (0 < delta) ? 1.1 : (1.0 / 1.1);
            _transformGroup.Children.Add(new ScaleTransform(scale, scale, center.X, center.Y));
            RenderTransform = _transformGroup;
        }

    }




}
