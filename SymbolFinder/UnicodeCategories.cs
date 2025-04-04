using System.ComponentModel;

namespace SymbolFinder
{
    public class UnicodeCategory(string shortName, string longName, string description, bool enabled = true) : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public string ShortName { get; } = shortName;

        public string LongName { get; } = longName;

        public string Description { get; } = description;
        private bool _enabled { get; set; } = enabled;
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Enabled)));
            }
        }
    }

    public class UnicodeCategories
    {


        public Dictionary<string, UnicodeCategory> Categories = [];
        public static readonly UnicodeCategories Instance = new();

        public static string GetCategoryName(string shortName)
        {
            if (Instance.Categories.TryGetValue(shortName, out UnicodeCategory? value))
            {
                return value.LongName;
            }
            else
            {
                return "";
            }
        }

        public static UnicodeCategory? GetCategory(string shortName)
        {
            if (Instance.Categories.TryGetValue(shortName, out UnicodeCategory? value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public UnicodeCategory Lu = new("Lu", "Uppercase Letter", "an uppercase letter");

        public UnicodeCategories()
        {
            //from https://www.unicode.org/reports/tr44/#General_Category_Values

            //Letter
            Categories.Add("Lu", new UnicodeCategory("Lu", "Uppercase Letter", "an uppercase letter"));
            Categories.Add("Ll", new UnicodeCategory("Ll", "Lowercase Letter", "a lowercase letter"));
            Categories.Add("Lt", new UnicodeCategory("Lt", "Titlecase Letter", "a digraph encoded as a single character, with first part uppercase"));
            Categories.Add("Lm", new UnicodeCategory("Lm", "Modifier Letter", "a modified letter"));
            Categories.Add("Lo", new UnicodeCategory("Lo", "Other Letter", "other letters, including syllables and ideographs"));

            //Mark
            Categories.Add("Mn", new UnicodeCategory("Mn", "Nonspacing Mark", "a nonspacing combining mark (zero advance width)"));
            Categories.Add("Mc", new UnicodeCategory("Mc", "Spacing Mark", "a spacing combining mark (positive advance width)"));
            Categories.Add("Me", new UnicodeCategory("Me", "Enclosing Mark", "an enclosing combining mark"));

            //Number
            Categories.Add("Nd", new UnicodeCategory("Nd", "Decimal Numbe", "a decimal digit"));
            Categories.Add("Nl", new UnicodeCategory("Nl", "Letter Number", "a letterlike numeric character"));
            Categories.Add("No", new UnicodeCategory("No", "Other Numbe", "a numeric character of other type"));

            //Punctuation
            Categories.Add("Pc", new UnicodeCategory("Pc", "Connector Punctuation", "a connecting punctuation mark, like a tie"));
            Categories.Add("Pd", new UnicodeCategory("Pd", "Dash Punctuation", "a dash or hyphen punctuation mark"));
            Categories.Add("Ps", new UnicodeCategory("Ps", "Open Punctuation", "an opening punctuation mark (of a pair)"));
            Categories.Add("Pe", new UnicodeCategory("Pe", "Close Punctuation", "a closing punctuation mark (of a pair)"));
            Categories.Add("Pi", new UnicodeCategory("Pi", "Initial Punctuation", "an initial quotation mark"));
            Categories.Add("Pf", new UnicodeCategory("Pf", "Final Punctuation", "a final quotation mark"));
            Categories.Add("Po", new UnicodeCategory("Po", "Other Punctuation", "a punctuation mark of other type"));

            //Symbol
            Categories.Add("Sm", new UnicodeCategory("Sm", "Math Symbol", "a symbol of mathematical use"));
            Categories.Add("Sc", new UnicodeCategory("Sc", "Currency Symbol", "a currency sign"));
            Categories.Add("Sk", new UnicodeCategory("Sk", "Modifier Symbol", "a non-letterlike modifier symbol"));
            Categories.Add("So", new UnicodeCategory("So", "Other Symbol", "a symbol of other type"));

            //Separator
            Categories.Add("Zs", new UnicodeCategory("Zs", "Space Separator", "a space character (of various non-zero widths)"));
            Categories.Add("Zl", new UnicodeCategory("Zl", "Line Separator", "U+2028 LINE SEPARATOR only"));
            Categories.Add("Zp", new UnicodeCategory("Zp", "Paragraph Separator", "U+2029 PARAGRAPH SEPARATOR only"));

            //Other                                  
            Categories.Add("Cc", new UnicodeCategory("Cc", "Control", "a C0 or C1 control code"));
            Categories.Add("Cf", new UnicodeCategory("Cf", "Format", "a format control character"));
            Categories.Add("Cs", new UnicodeCategory("Cs", "Surrogate", "a surrogate code point"));
            Categories.Add("Co", new UnicodeCategory("Co", "Private Use", "a private-use character"));
            Categories.Add("Cn", new UnicodeCategory("Co", "Unassigned", "a reserved unassigned code point or a noncharacter"));
        }

    }
}
