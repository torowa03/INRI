using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows;

using System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace Inri.Common
{
    /// <summary>  
    /// 画像ビューワの便利メソッド集  
    /// </summary>  
    public static class ImageUtils
    {
        public static BitmapSource CopyScreen()
        {
            using (var screenBmp = new Bitmap(
                (int)SystemParameters.PrimaryScreenWidth,
                (int)SystemParameters.PrimaryScreenHeight,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb))
            {
                using (var bmpGraphics = Graphics.FromImage(screenBmp))
                {
                    bmpGraphics.CopyFromScreen(0, 0, 0, 0, screenBmp.Size);
                    return Imaging.CreateBitmapSourceFromHBitmap(
                        screenBmp.GetHbitmap(),
                        IntPtr.Zero,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());
                }
            }
        }



        /// <summary>  
        /// 指定したフォルダから、指定した拡張子のファイルを  
        /// ImageInfo型のリストにして返す。  
        /// </summary>  
        /// <param name="directory">ファイルを探すディレクトリへのパス</param>  
        /// <param name="supportExts">探す拡張子</param>  
        /// <returns>引数で指定した条件に合致する画像の情報</returns>  
        public static List<ImageInfo> GetImages(string directory, string[] supportExts)
        {
            if (!Directory.Exists(directory))
            {
                // ディレクトリが無い時は空のリストを返す  
                return new List<ImageInfo>();
            }
            var dirInfo = new DirectoryInfo(directory);
            // ディレクトリからファイルを拡張子で絞り込んで返す  
            return dirInfo.GetFiles().
                Where(f => supportExts.Contains(f.Extension)).
                Select(f => new ImageInfo { Path = f.FullName }).
                ToList();
        }

        public static List<ImageInfo> GetImages(string[] files, string[] supportExts)
        {
            // ディレクトリからファイルを拡張子で絞り込んで返す  
            return files.
                Select(f => new ImageInfo { Path = f}).
                ToList();
        }


    }

    /// <summary>  
    /// 画像に関する情報を持たせるクラス  
    /// </summary>  
    public class ImageInfo
    {
        /// <summary>  
        /// 画像ファイルへのパス  
        /// </summary>  
        public string Path { get; set; }

        /// <summary>
        /// 画像名
        /// </summary>
        public string Name
        {
            get
            {
                return System.IO.Path.GetFileNameWithoutExtension(Path);
            }
        }

    }  
}
