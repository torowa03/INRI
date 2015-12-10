using System;
using System.Windows.Media;
using FirstFloor.ModernUI.Presentation;

using Inri.Common;

///設定の記憶機能を追加
namespace BibleStandard
{
    [Serializable]
    public class AppearanceSettings
    {
        public static AppearanceSettings Instance { get; set; }

        public static AppearanceSettings Load(string path)
        {
            AppearanceSettings ret = null;

            try
            {
                ret = path.ReadXml<AppearanceSettings>();
            }
            catch (Exception ex)
            {
                ret = new AppearanceSettings();

                CommonApp.ExceptionLog(ex);
            }

            Instance = ret;

            return ret;
        }

        public static void Save(string path)
        {
            try
            {
                Instance.WriteXml(path);
            }
            catch (Exception ex)
            {
                CommonApp.ExceptionLog(ex);
            }
        }

        /// <summary>
        /// The <see cref="ThemeSource" /> property's name.
        /// </summary>
        public const string ThemeSourcePropertyName = "ThemeSource";

        private string _themeSource = AppearanceManager.Current.ThemeSource.OriginalString;

        /// <summary>
        /// Sets and gets the ThemeSource property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public string ThemeSource
        {
            get
            {
                return _themeSource;
            }

            set
            {
                if (_themeSource == value)
                {
                    return;
                }

                _themeSource = value;
                //RaisePropertyChanged(ThemeSourcePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="FontSize" /> property's name.
        /// </summary>
        public const string FontSizePropertyName = "FontSize";

        private FontSize _fontSize = AppearanceManager.Current.FontSize;

        /// <summary>
        /// Sets and gets the FontSize property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public FontSize FontSize
        {
            get
            {
                return _fontSize;
            }

            set
            {
                if (_fontSize == value)
                {
                    return;
                }

                _fontSize = value;
                //RaisePropertyChanged(FontSizePropertyName);
            }
        }

        /// <summary>
        /// The <see cref="AccentColor" /> property's name.
        /// </summary>
        public const string AccentColorPropertyName = "AccentColor";

        private Color _accentColor = AppearanceManager.Current.AccentColor;

        /// <summary>
        /// Sets and gets the AccentColor property.
        /// Changes to that property's value raise the PropertyChanged event. 
        /// </summary>
        public Color AccentColor
        {
            get
            {
                return _accentColor;
            }

            set
            {
                if (_accentColor == value)
                {
                    return;
                }

                _accentColor = value;
                //RaisePropertyChanged(AccentColorPropertyName);
            }
        }


    }
}