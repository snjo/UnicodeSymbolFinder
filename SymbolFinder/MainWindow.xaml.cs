using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.IO;
using System.Text;
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
        LoadUnicodeFile(@"data\UnicodeData.txt");
        this.DataContext = this;
        ResultBox.ItemsSource = SearchResults;
    }

    private void ClickSearch(object sender, RoutedEventArgs e)
    {
        SearchSymbols();
    }

    private void SearchSymbols()
    {
        string searchTerm = TextboxSearch.Text;

        SearchResults.Clear();

        foreach (UnicodeSymbol symbol in Symbols)
        {
            StringBuilder sb = new();
            if (symbol.Contains(searchTerm))
            {
                SearchResults.Add(symbol);
            }
        }
        Debug.WriteLine($"Found {SearchResults.Count} among {Symbols.Count} matching {searchTerm}");
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
        }
        else
        {
            Debug.WriteLine($"Selected symbol was null");
        }
        CopySymbolToClipboard(symbol);
    }

    private void CopySymbolToClipboard(UnicodeSymbol? symbol)
    {
        if (symbol == null) return;
        Clipboard.SetText(symbol.Symbol.ToString());
    }

    private void TextboxSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchSymbols();
        }
    }
}