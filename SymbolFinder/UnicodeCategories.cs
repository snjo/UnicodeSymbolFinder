using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SymbolFinder
{
    internal class UnicodeCategories
    {
        public static string GetCategoryName(string shortName)
        {
            return GetCategory(shortName).Name;
        }

        public static (string Name, string Comment) GetCategory(string shortName)
        {
            //from https://www.unicode.org/reports/tr44/#General_Category_Values
            switch (shortName)
            {
                //Letter
                case "Lu": return ("Uppercase Letter", "an uppercase letter");
                case "Ll": return ("Lowercase Letter", "a lowercase letter");
                case "Lt": return ("Titlecase Letter", "a digraph encoded as a single character, with first part uppercase");
                case "Lm": return ("Modifier Letter", "a modifier letter");
                case "Lo": return ("Other Letter", "other letters, including syllables and ideographs");
                //Mark
                case "Mn": return ("Nonspacing Mark", "a nonspacing combining mark (zero advance width)");
                case "Mc": return ("Spacing Mark", "a spacing combining mark (positive advance width)");
                case "Me": return ("Enclosing Mark", "an enclosing combi.ning mark");
                //Number
                case "Nd": return ("Decimal Number", "a decimal digit");
                case "Nl": return ("Letter Number", "a letterlike numeric character");
                case "No": return ("Other Number", "a numeric character of other type");
                //Puncuation
                case "Pc": return ("Connector Punctuation", "a connecting punctuation mark, like a tie");
                case "Pd": return ("Dash Punctuation", "a dash or hyphen punctuation mark");
                case "Ps": return ("Open Punctuation", "an opening punctuation mark (of a pair)");
                case "Pe": return ("Close Punctuation", "a closing punctuation mark (of a pair)");
                case "Pi": return ("Initial Punctuation", "an initial quotation mark");
                case "Pf": return ("Final Punctuation", "a final quotation mark");
                case "Po": return ("Other Punctuation", "a punctuation mark of other type");
                //Symbol
                case "Sm": return ("Math Symbol", "a symbol of mathematical use");
                case "Sc": return ("Currency Symbol", "a currency sign");
                case "Sk": return ("Modifier Symbol", "a non-letterlike modifier symbol");
                case "So": return ("Other Symbol", "a symbol of other type");
                //Separator
                case "Zs": return ("Space Separator", "a space character (of various non-zero widths)");
                case "Zl": return ("Line Separator", "U+2028 LINE SEPARATOR only");
                case "Zp": return ("Paragraph Separator", "U+2029 PARAGRAPH SEPARATOR only");
                //Other
                case "Cc": return ("Control", "a C0 or C1 control code");
                case "Cf": return ("Format", "a format control character");
                case "Cs": return ("Surrogate", "a surrogate code point");
                case "Co": return ("Private Use", "a private-use character");
                case "Cn": return ("Unassigned", "a reserved unassigned code point or a noncharacter");
                default:
                    return ("", "");
            }
        }
    }
}
