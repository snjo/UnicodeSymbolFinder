using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFinder
{
    public class UnicodeSymbol
    {
        public string Name { get; set; } // the display name of the character
        public char Symbol { get; set; } // the actual unicode symbol character
        public string CodePoint { get; set; } // unicode hex code
        public bool hidden = false;

        //public UnicodeSymbol(string name, string symbol, int number)
        //{
        //    throw new NotImplementedException();
        //    Name = name;
        //    Symbol = symbol;
        //    CodePoint = number;
        //}

        public UnicodeSymbol(string[] values)
        {
            if (values.Length < (int)Importindex.END)
            {
                throw new IndexOutOfRangeException("Unicode values array is shorter than expected");
            }
            Name = values[(int)Importindex.Name];
            CodePoint = values[(int)Importindex.Code_Point];
            long codeNumber = Convert.ToInt64(CodePoint, 16);

            Symbol = (char)codeNumber;
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
            if (hidden && !showHidden) return false;
            return (Name.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
