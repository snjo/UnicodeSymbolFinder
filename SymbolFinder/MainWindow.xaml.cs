using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SymbolFinder;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{

    List<UnicodeSymbol> Symbols = [];
    public UnicodeCategories unicodeCategories = new UnicodeCategories();

    public ObservableCollection<UnicodeSymbol> SearchResults = [];
    public ObservableCollection<UnicodeCategory> CategoryList = [];
    public Dictionary<string, string>? HiddenSymbols = [];
    public Dictionary<string, string>? Favorites = [];
    public bool ShowHiddenSymbols { get; set; }
    public bool ShowFavoritesOnly { get; set; }
    public FontData ResultFontSize = new(15);
    public string hiddenSymbolsFilePath = @"data\hiddensymbols.txt";
    public string favoritesFilePath = @"data\favorites.txt";
    public string unicodeDataFilePath = @"data\UnicodeData.txt"; // source file from the Unicode.org
    public string unicodeSymbolsFilePath = @"data\symbols.txt"; // the generated personal library file
    string HexPrefix = "0x";

    //font selector objects
    public string selectedFont { get; set; } = "Comic Sans";

    public static readonly DependencyProperty MyFontFamilyProperty =
        DependencyProperty.Register("MyFontFamily",
        typeof(FontFamily), typeof(MainWindow), new UIPropertyMetadata(null));


    // periodic timer
    System.Windows.Threading.DispatcherTimer saveTimer = new System.Windows.Threading.DispatcherTimer();

    

    public MainWindow()
    {
        InitializeComponent();
        if (File.Exists(unicodeSymbolsFilePath))
        {
            LoadSymbolsFile();
        }
        else
        {
            LoadUnicodeFile(unicodeDataFilePath);
        }
        
        this.DataContext = this;
        ResultBox.Items.Clear();
        ResultBox.ItemsSource = SearchResults;
        ListviewCategories.ItemsSource = CategoryList;
        saveTimer.Tick += new EventHandler(SaveTimer_Tick);
        saveTimer.Interval = new TimeSpan(0, 0, 30);
        saveTimer.Start();

        CategoryList = [];
        foreach (var category in UnicodeCategories.Instance.Categories)
        {
            CategoryList.Add(category.Value);
            //Debug.WriteLine($"Added {category.Value.LongName}");
        }
        ListviewCategories.ItemsSource = CategoryList;

        //create font list
        foreach (FontFamily fontFamily in Fonts.SystemFontFamilies)
        {
            // FontFamily.Source contains the font family name.
            //comboBoxFonts.Items.Add(fontFamily.Source);
            Debug.WriteLine($"font: {fontFamily.Source}");
            if (fontFamily.Source == ("Segoe UI Emoji") || fontFamily.Source == ("Segoe UI") || fontFamily.Source == "Arial")
            {
                Debug.WriteLine($"Found font preference {fontFamily.Source}");
                comboBoxFonts.SelectedItem = fontFamily;
                // continue until end, luckily Segoe UI Emoji is last in the list, so it wins
            }
        }




        //do a search to fill the list on launch
        SearchResults = SearchSymbols(Symbols, TextboxSearch.Text, false, false);
        ResultBox.ItemsSource = SearchResults;
        Debug.WriteLine($"Start Results: {SearchResults.Count}");
    }

    private void SaveTimer_Tick(object? sender, EventArgs e)
    {
        SaveSymolsFile();
    }

    private void ClickSearch(object sender, RoutedEventArgs e)
    {
        SearchSymbolsWithConditions(ShowHiddenSymbols, ShowFavoritesOnly);
    }

    private void SearchSymbolsWithConditions(bool showHidden, bool showFavoritesOnly)
    {
        string searchTerm = TextboxSearch.Text;
        
        List<UnicodeSymbol> results = [];

        // escape clauses to default to regular search
        if (searchTerm.Length == 0)
        {
            SearchResults = SearchSymbols(Symbols, TextboxSearch.Text, showHidden, showFavoritesOnly);
            ResultBox.ItemsSource = SearchResults;
            return;
        }
        if (searchTerm.Contains('+') == false && searchTerm.Contains('-') == false)
        {
            SearchResults = SearchSymbols(Symbols, TextboxSearch.Text, showHidden, showFavoritesOnly);
            ResultBox.ItemsSource = SearchResults;
            return;
        }


        List<string> includeTerms = [];
        List<string> excludeTerms = [];

        int i = 0;
        string currentTerm = string.Empty;
        bool inclusive = true;
        while (i < searchTerm.Length)
        {
            char c = searchTerm[i];

            if (c == '+' || c == '-')
            {
                if (inclusive && currentTerm.Length > 0)
                {
                    includeTerms.Add(currentTerm);
                    Debug.WriteLine($"  + Add INCLUDE term {currentTerm}");
                    currentTerm = string.Empty;
                }
                else if (currentTerm.Length > 0)
                {
                    excludeTerms.Add(currentTerm);
                    Debug.WriteLine($"  - Add EXCLUDE term {currentTerm}");
                    currentTerm = string.Empty;
                }
            }
            
            if (c == '+')
            {
                inclusive = true;
            }
            else if (c == '-')
            {
                inclusive = false;
            }
            else
            {
                currentTerm += c;
            }

            i++;
        }
        // add the final term
        if (inclusive && currentTerm.Length > 0)
        {
            includeTerms.Add(currentTerm);
            Debug.WriteLine($"  + Add INCLUDE term {currentTerm}");
        }
        else if (currentTerm.Length > 0)
        {
            excludeTerms.Add(currentTerm);
            Debug.WriteLine($"  - Add EXCLUDE term {currentTerm}");
        }

        // there must something to search for, so if all terms are exclude, exclude from the full list
        if (includeTerms.Count == 0)
        {
            includeTerms.Add("");
        }
        
        foreach (UnicodeSymbol symbol in Symbols)
        {
            bool includeSymbol = true;
            foreach (string term in includeTerms)
            {
                if (symbol.Contains(term, true) == false)
                {
                    includeSymbol = false;
                    break;
                }
            }

            foreach (string term in excludeTerms)
            {
                if (symbol.Contains(term, true) == true)
                {
                    includeSymbol = false;
                    break;
                }
            }

            if (includeSymbol)
            {
                results.Add(symbol);
            }
        }

        SearchResults.Clear();
        SearchResults = SearchSymbols(results, "", showHidden, showFavoritesOnly);
        ResultBox.ItemsSource = SearchResults;
    }

    private ObservableCollection<UnicodeSymbol> SearchSymbols(List<UnicodeSymbol> symbolList, string searchTerm, bool showHidden, bool showFavoritesOnly)
    {
        ObservableCollection<UnicodeSymbol> results = [];

        // ints to display the search result label next to the search box
        int foundAmount = 0;
        int foundHiddenAmount = 0;

        foreach (UnicodeSymbol symbol in symbolList)
        {
            // all symbols default to being shown in the search results if the search term hits, but can be hidden by criteria below
            bool addEntry = true;
            if (symbol.Contains(searchTerm, true))
            {
                // only show symbols that are enabled in the categeries list, or more precisely, don't show them if they're off.
                // that way, if a symbol has an unknown category it will always be shown as a failsafe
                if (UnicodeCategories.Instance.Categories[symbol.Category].Enabled == false)
                {
                    addEntry = false;
                }

                if (symbol.Hidden)
                {
                    foundHiddenAmount++;
                    if (showHidden == false)
                    {
                        addEntry = false;
                        //Debug.WriteLine($"Skipping hidden symbol {symbol.Name}");
                    }
                }

                if (symbol.Favorite == false && showFavoritesOnly)
                {
                    addEntry = false;
                    //Debug.WriteLine($"Skipping unfaved symbol {symbol.Name} in fave only mode");
                }


                if (addEntry)
                {
                    foundAmount++;
                    results.Add(symbol);
                    //Debug.WriteLine($"Adding entry {symbol.Name}");
                }
            }
        }

        if (showHidden)
        {
            TextblockSearchCount.Text = $"Found {foundAmount} (including {foundHiddenAmount} hidden symbols)";
            
        }
        else
        {
            TextblockSearchCount.Text = $"Found {foundAmount} ({foundHiddenAmount} symbols hidden)";
        }
        return results;
    }

    private HashSet<string> CreateSymbolHashes()
    {
        HashSet<string> values = new();
        foreach (UnicodeSymbol symbol in Symbols)
        {
            values.Add(symbol.CodePoint);
        }
        return values;
    }

    private void LoadUnicodeFile(string filename)
    {
        // load the raw UnicodeData.txt file from unicode.org, and fill the symbol dictionary
        // only used if there's no symbols.txt already generated

        if (File.Exists(filename) == false)
        {
            Debug.WriteLine($"No such unicode text file: {filename}");
        }

        string[] unicodeLines = File.ReadAllLines(filename);

        DateTime before = DateTime.Now;

        int counterNew = 0; // test safeguard, implement timer if read takes too long
        int counterDuplicate = 0;

        // create a hashSet to very quickly look for duplicate symbols, much faster than comparing lists.
        HashSet<string> symbolHashSet = CreateSymbolHashes();

        foreach (string line in unicodeLines)
        {
            string[] values = line.Split(';');
            if (values.Length < 5)
            {
                Debug.WriteLine($"Too few values on line {counterNew}");
                continue;
            }
            
            if (symbolHashSet.Contains(values[0]) == false)
            {
                Symbols.Add(new UnicodeSymbol(this, values));
                SaveRequested = true;
                counterNew++;
            }
            else
            {
                counterDuplicate++;
            }
        }

        Debug.WriteLine($"Added {counterNew} symbols, skipped {counterDuplicate} duplicate entries");

        TimeSpan processingTime = DateTime.Now - before;
        Debug.WriteLine($"Read all line in {(int)processingTime.TotalSeconds}s {processingTime.Milliseconds}");
    }

    private void ResultBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        CopySelectedSymbolToClipboard();
    }

    private void ButtonCopySymbol_Click(object sender, RoutedEventArgs e)
    {
        CopySelectedSymbolToClipboard();
    }

    private void ButtonCopyAllSelectedSymbols_Click(object sender, RoutedEventArgs e)
    {
        CopyAllSelectedSymbols();
    }

    private void CopyAllSelectedSymbols()
    {
        if (ResultBox.SelectedItems.Count > 0)
        {
            StringBuilder sb = new();
            foreach (var item in ResultBox.SelectedItems)
            {
                if (item is UnicodeSymbol symbol)
                {
                    sb.Append(symbol.Symbol);
                }
            }
            if (sb.Length > 0)
            {
                try
                {
                    Clipboard.SetText(sb.ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error copying to clipboard\n{ex.Message}");
                }
            }
        }
    }

    private void ButtonCopyAllSelectedList_Click(object sender, RoutedEventArgs e)
    {
        CopyAllSelectedAsList();
    }

    private void CopyAllSelectedAsList()
    {
        if (ResultBox.SelectedItems.Count > 0)
        {
            StringBuilder sb = new();
            foreach (var item in ResultBox.SelectedItems)
            {
                if (item is UnicodeSymbol symbol)
                {
                    sb.Append(HexPrefix + symbol.CodePoint + "\t");
                    sb.Append(symbol.Symbol + "\t");
                    sb.Append(symbol.Name);
                    if (symbol.PersonalComment.Length > 0)
                    {
                        sb.Append(" - " + symbol.PersonalComment);
                    }
                    sb.AppendLine();
                }
            }
            if (sb.Length > 0)
            {
                try
                {
                    Clipboard.SetText(sb.ToString());
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error copying to clipboard\n{ex.Message}");
                }
            }
        }
    }

    private void CopySelectedSymbolToClipboard()
    {
        UnicodeSymbol? symbol = (UnicodeSymbol)ResultBox.SelectedItem;
        if (symbol != null)
        {
            Debug.WriteLine($"Copy {symbol.Name}");
            CopySymbolToClipboard(symbol);
        }
        else
        {
            Debug.WriteLine($"Selected symbol was null");
        }
    }

    private static void CopySymbolToClipboard(UnicodeSymbol? symbol)
    {
        if (symbol == null) return;
        string clipboardText = symbol.Symbol.ToString();
        if (clipboardText.Length > 0)
        {
            try
            {
                Clipboard.SetText(symbol.Symbol.ToString());
            }
            catch (Exception e)
            {
                MessageBox.Show("Couldn't set clipboard text\n\n" + e.Message);
            }
        }
        else
        {
            Clipboard.Clear();
        }
    }

    private void TextboxSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchSymbolsWithConditions(ShowHiddenSymbols, ShowFavoritesOnly);
        }
    }

    private void FavoriteSelectedSymbols()
    {
        if (Favorites == null)
        {
            Debug.WriteLine($"Favorites was null, creating empty list");
            Favorites = [];
        }
        foreach (object obj in ResultBox.SelectedItems)
        {
            if (obj is UnicodeSymbol symbol)
            {
                symbol.Favorite = true;
                if (Favorites.ContainsKey(symbol.CodePoint) == false)
                {
                    Favorites.Add(symbol.CodePoint, symbol.Name);
                }
                SaveRequested = true;
            }
        }
    }

    private void UnFavoriteSelectedSymbols()
    {
        if (Favorites == null)
        {
            Debug.WriteLine($"Favorites was null, creating empty list");
            Favorites = [];
        }
        foreach (object obj in ResultBox.SelectedItems)
        {
            if (obj is UnicodeSymbol symbol)
            {
                symbol.Favorite = false;
                if (Favorites.ContainsKey(symbol.CodePoint) == true)
                {
                    Favorites.Remove(symbol.CodePoint);
                }
                SaveRequested = true;
            }
        }
    }

    private void HideSelectedSymbols()
    {
        if (HiddenSymbols == null)
        {
            HiddenSymbols = [];
        }
        foreach (object obj in ResultBox.SelectedItems)
        {
            if (obj is UnicodeSymbol symbol)
            {
                symbol.Hidden = true;
                if (HiddenSymbols.ContainsKey(symbol.CodePoint) == false)
                {
                    HiddenSymbols.Add(symbol.CodePoint, symbol.Name);
                    
                }
                SaveRequested = true;
            }
        }
    }

    private void UnHideSelectedSymbols()
    {
        if (HiddenSymbols == null)
        {
            HiddenSymbols = [];
        }
        foreach (object obj in ResultBox.SelectedItems)
        {
            if (obj is UnicodeSymbol symbol)
            {
                symbol.Hidden = false;
                if (HiddenSymbols.ContainsKey(symbol.CodePoint) == true)
                {
                    HiddenSymbols.Remove(symbol.CodePoint);   
                }
                SaveRequested = true;
            }
        }
    }


    private void ResultBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.H)
        {
            HideSelectedSymbols();
        }
        if (e.Key == Key.U)
        {
            UnHideSelectedSymbols();
        }
        if (e.Key == Key.F)
        {
            FavoriteSelectedSymbols();
        }
        if (e.Key == Key.R)
        {
            UnFavoriteSelectedSymbols();
        }
        if (e.Key == Key.C)
        {
            CopyAllSelectedSymbols();
        }
        if (e.Key == Key.L)
        {
            CopyAllSelectedAsList();
        }
    }

    UnicodeSymbol? currentSymbol = null;
    private void ResultBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ResultBox.SelectedItem is UnicodeSymbol symbol)
        {
            currentSymbol = symbol;
            //Debug.WriteLine($"Showing info for symbol {symbol.Name}");
            TextboxSymbolName.Text = symbol.Name;
            TextboxSymbolGraphic.Text = symbol.Symbol;
            TextboxSymbolCodepoint.Text = HexPrefix + symbol.CodePoint;
            TextblockSymbolCategory.Text = "Category: " + UnicodeCategories.GetCategoryName(symbol.Category);
            TextblockISOcomment.Text = "ISO comment: " + symbol.ISOcomment;
            TextblockUnicode1Name.Text = "Unicode 1 name: " + symbol.Unicode_1_Name;
            TextboxPersonalComment.Text = symbol.PersonalComment;
        }    
    }

    private void ButtonHideSelected_Click(object sender, RoutedEventArgs e)
    {
        HideSelectedSymbols();
    }

    private void ButtonUnHideSelected_Click(object sender, RoutedEventArgs e)
    {
        UnHideSelectedSymbols();
    }

    private void ButtonFavoriteSelected_Click(object sender, RoutedEventArgs e)
    {
        FavoriteSelectedSymbols();
    }

    private void ButtonUnFavoriteSelected_Click(object sender, RoutedEventArgs e)
    {
        UnFavoriteSelectedSymbols();
    }

    bool SaveRequested = false;
    private void TextboxPersonalComment_TextChanged(object sender, TextChangedEventArgs e)
    {
        SaveRequested = true;
        if (currentSymbol != null)
        {
            currentSymbol.PersonalComment = TextboxPersonalComment.Text;
        }
        if (ResultBox.SelectedItems.Count > 1) 
        { 
            foreach (var item in ResultBox.SelectedItems)
            {
                if (item is UnicodeSymbol symbol)
                {
                    symbol.PersonalComment = TextboxPersonalComment.Text;
                }
            }
        }
    }

    private void LoadSymbolsFile()
    {
        string fullPath = System.IO.Path.GetFullPath(unicodeSymbolsFilePath);
        Debug.WriteLine($"Load symbols from file: {fullPath}");

        Symbols.Clear();
        if (File.Exists(fullPath))
        {
            string[] lines = File.ReadAllLines(fullPath);
            int counter = 0;
            foreach (string line in lines)
            {
                
                string[] entries = line.Split(';');
                if (entries.Length >= 7)
                {
                    Symbols.Add(new UnicodeSymbol(this, entries[0], entries[1], entries[2], entries[3], entries[4], bool.Parse(entries[5]), bool.Parse(entries[6])));
                }
                else
                {
                    Debug.WriteLine($"Too few entries on line {counter} when loading symbols");
                }
                counter++;
            }
            Debug.WriteLine($"Added {counter} symbols from file");
        }
    }

    private void SaveSymolsFile(bool forceSave = false)
    {
        if (SaveRequested || forceSave)
        {
            Debug.WriteLine($"Saving symbols file");
            SaveRequested = false;
            string fullPath = System.IO.Path.GetFullPath(unicodeSymbolsFilePath);
            StringBuilder sb = new StringBuilder();
            foreach (UnicodeSymbol symbol in Symbols)
            {
                // ( codepoint,  name,  category,  unicode_1_name,  personalcomment, bool favorite, bool hidden) 
                sb.Append(symbol.CodePoint + ";");
                sb.Append(symbol.Name + ";");
                sb.Append(symbol.Category + ";");
                sb.Append(symbol.Unicode_1_Name + ";");
                sb.Append(symbol.PersonalComment + ";");
                sb.Append(symbol.Favorite + ";");
                sb.Append(symbol.Hidden + ";");
                sb.AppendLine();
            }
            try
            {
                File.WriteAllText(fullPath, sb.ToString());
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Couldn't save symbols file, exception:");
                Debug.WriteLine(e.Message);
            }
        }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
    {
        Debug.WriteLine($"Closing application, requesting save of symbols file");
        SaveSymolsFile();
    }

    private void ButtonFontPlus_Click(object sender, RoutedEventArgs e)
    {
        //Debug.WriteLine($"Setting font size {ResultFontSize.FontSize}");
        ResultFontSize.FontSize += 5;
        TextblockFontSize.Text = "Font size: " + ResultFontSize.FontSize.ToString();
        ResultBox.Items.Refresh();
    }

    private void ButtonFontMinus_Click(object sender, RoutedEventArgs e)
    {
        //Debug.WriteLine($"Setting font size {ResultFontSize.FontSize}");
        ResultFontSize.FontSize -= 5;
        if (ResultFontSize.FontSize < 10)
        {
            ResultFontSize.FontSize = 10;
        }
        TextblockFontSize.Text = "Font size: " + ResultFontSize.FontSize.ToString();
        ResultBox.Items.Refresh();
    }

    private void ButtonSelectAllCategories_Click(object sender, RoutedEventArgs e)
    {
        foreach (UnicodeCategory cat in CategoryList)
        {
            cat.Enabled = true;
        }
    }

    private void ButtonSelectNoCategories_Click(object sender, RoutedEventArgs e)
    {
        foreach (UnicodeCategory cat in CategoryList)
        {
            cat.Enabled = false;
        }
    }

    private void MenuExit_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }

    private void MenuUpdateUnicodeData_Click(object sender, RoutedEventArgs e)
    {
        LoadUnicodeFile(unicodeDataFilePath);
    }

    public static void OpenLink(string url)
    {
        Process.Start(new ProcessStartInfo() { FileName = url, UseShellExecute = true });
    }

    private void MenuHelpDocumentation_Click(object sender, RoutedEventArgs e)
    {
        OpenLink("https://github.com/snjo/UnicodeSymbolFinder/blob/master/README.md");
    }

    private void MenuAbout_Click(object sender, RoutedEventArgs e)
    {
        MessageBox.Show("Unicode Symbol Finder\n\nby Andreas Aakvik Gogstad\n2025");
    }

    private void MenuSourceCode_Click(object sender, RoutedEventArgs e)
    {
        OpenLink("https://github.com/snjo/UnicodeSymbolFinder");
    }
}