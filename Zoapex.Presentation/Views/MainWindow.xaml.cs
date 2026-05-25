using System.Windows;

namespace Zoapex.Presentation.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        mainFrame.Navigate(new CatalogView());
    }

    private void BtnCatalog_Click(object sender, RoutedEventArgs e)
        => mainFrame.Navigate(new CatalogView());

    private void BtnCart_Click(object sender, RoutedEventArgs e)
        => mainFrame.Navigate(new CartView());
}
