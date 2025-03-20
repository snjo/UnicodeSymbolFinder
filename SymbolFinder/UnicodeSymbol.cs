﻿using System;
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
        public string Symbol { get; set; } // the actual unicode symbol character
        public string CodePoint { get; set; } // unicode hex code
        public string Category { get; set; } // unicode symbol Category
        public string ISOcomment { get; set; }
        public string Unicode_1_Name { get; set; }

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
            int codeNumber = Convert.ToInt32(CodePoint, 16);
            Category = values[(int)Importindex.General_Category];
            Unicode_1_Name = values[(int)Importindex.Unicode_1_Name];
            ISOcomment = values[(int)Importindex.ISO_Comment];


            // skip any symbols marked as surrogate, otherwise ConvertFromUtf32 causes an exception. String matching is sufficient.
            // A high surrogate is a character in the range U+D800 through U+DBFF. A low surrogate is a character in the range U+DC00 through U+DFFF.
            if (Name.Contains("surrogate", StringComparison.InvariantCultureIgnoreCase))
            {
                Symbol = "";
                hidden = true;
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
            if (hidden && !showHidden) return false;
            return (Name.Contains(searchTerm, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
