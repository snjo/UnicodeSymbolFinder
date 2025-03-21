using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFinder
{
    public class FontData : INotifyPropertyChanged
    {
        private double _fontSize;
        public event PropertyChangedEventHandler? PropertyChanged;

        public FontData(double fontSize)
        {
            FontSize = fontSize;
        }

        private FontData()
        {
            FontSize = 12;
        }

        public double FontSize
        {
            get { return _fontSize; }
            set
            {
                _fontSize = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("FontSize"));
                }
            }
        }
    }
}
