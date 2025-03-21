using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFinder
{
    public class UnicodeSymbol : INotifyPropertyChanged
    {
        public string Name { get; set; } // the display name of the character
        public string Symbol { get; set; } // the actual unicode symbol character
        public string CodePoint { get; set; } // unicode hex code
        public string Category { get; set; } // unicode symbol Category
        public string ISOcomment { get; set; }
        public string Unicode_1_Name { get; set; }
        private MainWindow Parent;

        private string _personalComment = "";
        public string PersonalComment
        {
            get
            {
                return _personalComment;
            }
            set
            {
                _personalComment = value.Replace(';',','); // prevent semicolons, used as separator in the symbols file
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("PersonalComment"));
                }
            }
        }

        public double FontSize
        { 
            get
            { 
                return Parent.ResultFontSize.FontSize;
            }
            set
            {
                Parent.ResultFontSize.FontSize = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("FontSize"));
                }
            }
        }

        //This is all that the interface requires
        public event PropertyChangedEventHandler? PropertyChanged;

        private bool _hidden = false;
        public bool Hidden {
            get
            {
                return _hidden;
            }
            set
            {
                _hidden = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Hidden"));
                }
            }
        }

        private bool _favorite = false;
        public bool Favorite
        {
            get
            {
                return _favorite;
            }
            set
            {
                _favorite = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Favorite"));
                }
            }
        }

        public UnicodeSymbol(MainWindow parent, string codepoint, string name, string category, string unicode_1_name, string personalcomment, bool favorite, bool hidden) // import from custom symbol file
        {
            Parent = parent;
            CodePoint = codepoint;
            Name = name;
            Category = category;
            Unicode_1_Name = unicode_1_name;
            ISOcomment = "";
            PersonalComment = personalcomment;
            Favorite = favorite;
            Hidden = hidden;

            int codeNumber = Convert.ToInt32(CodePoint, 16);
            if (Name.Contains("surrogate", StringComparison.InvariantCultureIgnoreCase))
            {
                Symbol = "";
                Hidden = true;
            }
            else
            {
                Symbol = char.ConvertFromUtf32(codeNumber);
            }
        }

        public UnicodeSymbol(MainWindow parent, string[] values) // import from UnicodeData.txt from unicode.org
        {
            Parent = parent;
            if (values.Length < (int)Importindex.END)
            {
                throw new IndexOutOfRangeException("Unicode values array is shorter than expected");
            }
            Name = values[(int)Importindex.Name];
            CodePoint = values[(int)Importindex.Code_Point];
            int codeNumber = Convert.ToInt32(CodePoint, 16);
            Category = values[(int)Importindex.General_Category];
            Unicode_1_Name = values[(int)Importindex.Unicode_1_Name];
            ISOcomment = values[(int)Importindex.ISO_Comment];
            PersonalComment = "";

            // skip any symbols marked as surrogate, otherwise ConvertFromUtf32 causes an exception. String matching is sufficient.
            // A high surrogate is a character in the range U+D800 through U+DBFF. A low surrogate is a character in the range U+DC00 through U+DFFF.
            if (Name.Contains("surrogate", StringComparison.InvariantCultureIgnoreCase))
            {
                Symbol = "";
                Hidden = true;
            }
            else
            {
                Symbol = char.ConvertFromUtf32(codeNumber);
            }

            //Debug.WriteLine($"Name: {Name}   Codepoint: {CodePoint}   Symbol: {Symbol}.");
        }

        private UnicodeSymbol()
        {
            throw new NotImplementedException();
        }

        public enum Importindex
        {
            Code_Point = 0,
            Name = 1,
            General_Category = 2,
            Canonical_Combining_Class = 3,
            Bidi_Class = 4,
            Decomposition_Type_and_Decomposition_Mapping = 5,
            Numeric_Type = 6,
            Numeric_Value_for_Type_Digit = 7,
            Numeric_Value_for_Type_Numeric = 8,
            Bidi_Mirrored = 9,
            Unicode_1_Name = 10,
            ISO_Comment = 11,
            Simple_Uppercase_Mapping = 12,
            Simple_Lowercase_Mapping = 13,
            Simple_Titlecase_Mapping = 14,
            END
        }

        public bool Contains(string searchTerm, bool showHidden = false)
        {
            if (Hidden && !showHidden) return false;
            return (Name.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase) || PersonalComment.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase) || CodePoint.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
