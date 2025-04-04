using SymbolFinder.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;


namespace SymbolFinder
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
    {
        private readonly MainWindow parent;
        public bool OptionShowHidden { get; set; }
        public bool OptionShowFontList { get; set; }
        public string OptionCodePointPrefix { get; set; }
        private string _optionFontFamily = "";
        public string OptionFontFamily
        {
            get
            {
                return _optionFontFamily;
            }
            set
            {
                ValidFont = parent.FontExists(value);
                UpdateFontTextBoxStyle();
                _optionFontFamily = value;
            }
        }
        public string OptionFontSize { get; set; }
        
        public bool ValidFont { get; set; }


        private void UpdateFontTextBoxStyle()
        {
            if (ValidFont)
            {
                TextBoxFontFamily.Foreground = Brushes.Black;
            }
            else
            {
                TextBoxFontFamily.Foreground = Brushes.Red;
            }
        }

        public Options(MainWindow _parent)
        {
            InitializeComponent();
            parent = _parent;
            this.DataContext = this;
            OptionFontFamily = Settings.Default.startingFont;
            OptionFontSize = Settings.Default.fontSize.ToString();
            OptionShowFontList = Settings.Default.showFontCompatibility;
            OptionShowHidden = Settings.Default.showHidden;
            OptionCodePointPrefix = Settings.Default.CodePointPrefix;
        }

        private void SaveSettings()
        {
            if (ValidFont)
            {
                Settings.Default.startingFont = OptionFontFamily;
            }
            if (double.TryParse(OptionFontSize, out double fontSize))
            {
                Settings.Default.fontSize = fontSize;
            }
            Settings.Default.showFontCompatibility = OptionShowFontList;
            Settings.Default.showHidden = OptionShowHidden;
            Settings.Default.CodePointPrefix = OptionCodePointPrefix;
            Settings.Default.Save();
        }

        private void ButtonOptionsOK_Click(object sender, RoutedEventArgs e)
        {
            SaveSettings();
            Close();
        }

        private void ButtonOptionsCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBoxFontFamily_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ValidFont = parent.FontExists(TextBoxFontFamily.Text);
            UpdateFontTextBoxStyle();
        }
    }
}
