using System.Windows;
using BudgetFlow.ViewModels;

namespace BudgetFlow;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        // Tous les Binding de la fenêtre héritent de cette source de données.
        DataContext = new DashboardViewModel();
        StateChanged += (_, _) =>
        {
            MaximizeButton.Content = WindowState == WindowState.Maximized ? "\uE923" : "\uE922";
            // Laisser visibles les bords de la fenêtre lorsqu'elle est maximisée.
            MainWindowContent.Margin = WindowState == WindowState.Maximized
                ? new Thickness(8) : new Thickness(0);
        };
    }

    // Ces actions concernent uniquement la fenêtre : elles restent dans la vue.
    private void Minimize_Click(object sender, RoutedEventArgs e) => SystemCommands.MinimizeWindow(this);

    private void Maximize_Click(object sender, RoutedEventArgs e)
    {
        if (WindowState == WindowState.Maximized)
            SystemCommands.RestoreWindow(this);
        else
            SystemCommands.MaximizeWindow(this);
    }

    private void Close_Click(object sender, RoutedEventArgs e) => SystemCommands.CloseWindow(this);
}
