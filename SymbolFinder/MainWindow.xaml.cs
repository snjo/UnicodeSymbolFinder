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
    public ObservableCollection<UnicodeSymbol> SearchResults = [];
    public Dictionary<string, string>? HiddenSymbols = [];
    public Dictionary<string, string>? Favorites = [];
    public bool ShowHiddenSymbols { get; set; }
    public bool ShowFavoritesOnly { get; set; }
    public string hiddenSymbolsFilePath = @"data\hiddensymbols.txt";
    public string favoritesFilePath = @"data\favorites.txt";
    public string unicodeDataFilePath = @"data\UnicodeData.txt";


    /*
        Code_Point;
        Name;
        General_Category;
        Canonical_Combining_Class;
        Bidi_Class;
        Decomposition_Type_and_Decomposition_Mapping;
        Numeric_Type;
        Numeric_Value_for_Type_Digit;
        Numeric_Value_for_Type_Numeric;
        Bidi_Mirrored;
        Unicode_1_Name;
        ISO_Comment;
        Simple_Uppercase_Mapping;
        Simple_Lowercase_Mapping;
        Simple_Titlecase_Mapping
    */

    public MainWindow()
    {
        InitializeComponent();
        LoadUnicodeFile(unicodeDataFilePath);
        this.DataContext = this;
        ResultBox.Items.Clear();
        ResultBox.ItemsSource = SearchResults;
        LoadHiddenSymbolsFile(hiddenSymbolsFilePath);
        LoadFavoritesFile(favoritesFilePath);
    }

    private void ClickSearch(object sender, RoutedEventArgs e)
    {
        SearchSymbols(ShowHiddenSymbols, ShowFavoritesOnly);
    }

    private void SearchSymbols(bool showHidden, bool showFavoritesOnly)
    {
        string searchTerm = TextboxSearch.Text;

        SearchResults.Clear();

        int foundAmount = 0;
        int foundHiddenAmount = 0;
        

        foreach (UnicodeSymbol symbol in Symbols)
        {
            bool addEntry = true;
            if (symbol.Contains(searchTerm, true))
            {
                
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
                    SearchResults.Add(symbol);
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
        //Debug.WriteLine($"Found {foundAmount} among {Symbols.Count} matching {searchTerm}. {foundHiddenAmount} were hidden");
    }

    private void LoadUnicodeFile(string filename)
    {
        if (File.Exists(filename) == false)
        {
            Debug.WriteLine($"No such unicode text file: {filename}");
        }

        string[] unicodeLines = File.ReadAllLines(filename);

        DateTime before = DateTime.Now;

        int counter = 0; // test safeguard, implement timer if read takes too long
        foreach (string line in unicodeLines)
        {
            string[] values = line.Split(';');
            if (values.Length < 5)
            {
                Debug.WriteLine($"Too few values on line {counter}");
                continue;
            }
            
            Symbols.Add(new UnicodeSymbol(values));
            counter++;
        }

        TimeSpan processingTime = DateTime.Now - before;
        Debug.WriteLine($"Read all line in {(int)processingTime.TotalSeconds}s {processingTime.Milliseconds}");
    }

    private void ResultBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
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
            SearchSymbols(ShowHiddenSymbols, ShowFavoritesOnly);
        }
    }

    private void FavoriteSelectedSymbols()
    {
        if (Favorites == null)
        {
            Favorites = [];
        }
        bool updateFavoritesFile = false;
        foreach (object obj in ResultBox.SelectedItems)
        {
            if (obj is UnicodeSymbol symbol)
            {
                if (Favorites.ContainsKey(symbol.CodePoint) == false)
                {
                    Favorites.Add(symbol.CodePoint, symbol.Name);
                    symbol.Favorite = true;
                }
                updateFavoritesFile = true;
            }
        }
        if (updateFavoritesFile)
        {
            SaveFavoritesFile(favoritesFilePath);
        }
    }

    private void UnFavoriteSelectedSymbols()
    {
        if (Favorites == null)
        {
            Favorites = [];
        }
        bool updateFavoritesFile = false;
        foreach (object obj in ResultBox.SelectedItems)
        {
            if (obj is UnicodeSymbol symbol)
            {
                if (Favorites.ContainsKey(symbol.CodePoint) == true)
                {
                    Favorites.Remove(symbol.CodePoint);
                    symbol.Favorite = false;
                }
                updateFavoritesFile = true;
            }
        }
        if (updateFavoritesFile)
        {
            SaveFavoritesFile(favoritesFilePath);
        }
    }

    private void HideSelectedSymbols()
    {
        if (HiddenSymbols == null)
        {
            HiddenSymbols = [];
        }
        bool updateHiddenSymbolsFile = false;
        foreach (object obj in ResultBox.SelectedItems)
        {
            if (obj is UnicodeSymbol symbol)
            {
                //Debug.WriteLine($"Hide symbol {symbol.CodePoint} : {symbol.Name}");
                if (HiddenSymbols.ContainsKey(symbol.CodePoint) == false)
                {
                    HiddenSymbols.Add(symbol.CodePoint, symbol.Name);
                    symbol.Hidden = true;
                }
                updateHiddenSymbolsFile = true;
            }
        }
        if (updateHiddenSymbolsFile)
        {
            SaveHiddenSymbolsFile(hiddenSymbolsFilePath);
        }
    }

    private void UnHideSelectedSymbols()
    {
        if (HiddenSymbols == null)
        {
            HiddenSymbols = [];
        }
        bool updateHiddenSymbolsFile = false;
        foreach (object obj in ResultBox.SelectedItems)
        {
            if (obj is UnicodeSymbol symbol)
            {
                //Debug.WriteLine($"Unhide symbol {symbol.CodePoint} : {symbol.Name}");
                if (HiddenSymbols.ContainsKey(symbol.CodePoint) == true)
                {
                    HiddenSymbols.Remove(symbol.CodePoint);
                    symbol.Hidden = false;
                }
                updateHiddenSymbolsFile = true;
            }
        }
        if (updateHiddenSymbolsFile)
        {
            SaveHiddenSymbolsFile(hiddenSymbolsFilePath);
        }
    }

    private void SaveHiddenSymbolsFile(string file)
    {
        string jsonString = JsonSerializer.Serialize(HiddenSymbols);
        //string saveFilePath = @"data\hiddensymbols.txt";
        string fullPath = System.IO.Path.GetFullPath(file);
        Debug.WriteLine($"Save hidden symbols to path: {fullPath}");
        File.WriteAllText(fullPath, jsonString);
    }

    private void SaveFavoritesFile(string file)
    {
        string jsonString = JsonSerializer.Serialize(Favorites);
        string fullPath = System.IO.Path.GetFullPath(file);
        Debug.WriteLine($"Save favorites to path: {fullPath}");
        File.WriteAllText(fullPath, jsonString);
    }

    private void LoadHiddenSymbolsFile(string file)
    {
        string fullPath = System.IO.Path.GetFullPath(file);
        Debug.WriteLine($"Load hidden symbols file from path: {fullPath}");
        if (File.Exists(fullPath))
        {
            string jsonString = File.ReadAllText(fullPath);
            HiddenSymbols = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
        }
        
        if (HiddenSymbols == null)
        {
            Debug.WriteLine($"Failed to load Hidden Symbols list, creating empty Dictionary.");
            HiddenSymbols = [];
        }
        else
        {
            Debug.WriteLine($"Loaded Hidden Symbols list with {HiddenSymbols.Count} entries");
        }

        foreach (UnicodeSymbol symbol in Symbols)
        {
            if (HiddenSymbols.ContainsKey(symbol.CodePoint))
            {
                symbol.Hidden = true;
                //Debug.WriteLine($"Hiding symbol {symbol.CodePoint} : {symbol.Name}");
            }
        }
    }

    private void LoadFavoritesFile(string file)
    {
        string fullPath = System.IO.Path.GetFullPath(file);
        Debug.WriteLine($"Load favorites file from path: {fullPath}");
        if (File.Exists(fullPath))
        {
            string jsonString = File.ReadAllText(fullPath);
            Favorites = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);
        }
        
        if (Favorites == null)
        {
            Debug.WriteLine($"Failed to load Favorites list, creating empty Dictionary.");
            Favorites = [];
        }
        else
        {
            Debug.WriteLine($"Loaded Favorits list with {Favorites.Count} entries");
        }

        foreach (UnicodeSymbol symbol in Symbols)
        {
            if (Favorites.ContainsKey(symbol.CodePoint))
            {
                symbol.Favorite = true;
                //Debug.WriteLine($"Faving symbol {symbol.CodePoint} : {symbol.Name}");
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
    }

    private void ResultBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ResultBox.SelectedItem is UnicodeSymbol symbol)
        {
            Debug.WriteLine($"Showing info for symbol {symbol.Name}");
            TextboxSymbolName.Text = symbol.Name;
            TextboxSymbolGraphic.Text = symbol.Symbol;
            TextboxSymbolCodepoint.Text = symbol.CodePoint;
            TextblockSymbolCategory.Text = "Category: " + symbol.Category;
            TextblockISOcomment.Text = "ISO comment: " + symbol.ISOcomment;
            TextblockUnicode1Name.Text = "Unicode 1 name: " + symbol.Unicode_1_Name;
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
}