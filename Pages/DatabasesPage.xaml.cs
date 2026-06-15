/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using XamppMultidomainManager.Services;

namespace XamppMultidomainManager.Pages;

public sealed partial class DatabasesPage : Page
{
    private readonly XamppService _xampp = App.GetService<XamppService>();
    private string[] _allDatabases = Array.Empty<string>();

    public DatabasesPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        LoadDatabases();
    }

    private void LoadDatabases()
    {
        LoadingIndicator.IsActive = true;
        try
        {
            _allDatabases = _xampp.GetDatabases().ToArray();
            SearchInput_TextChanged(null, null);
        }
        finally { LoadingIndicator.IsActive = false; }
    }

    private void SearchInput_TextChanged(object? sender, TextChangedEventArgs? e)
    {
        var s = SearchInput.Text?.Trim().ToLower() ?? "";
        var f = string.IsNullOrWhiteSpace(s)
            ? _allDatabases
            : _allDatabases.Where(d => d.ToLower().Contains(s)).ToArray();
        DatabasesGrid.ItemsSource = f.Select(n => new DbItem { Name = n }).ToArray();
        EmptyState.Visibility = f.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    // ── Add Database ───────────────────────────────────────────────
    private void AddDbBtn_Click(object sender, RoutedEventArgs e)
    {
        AddDbPanel.Visibility = Visibility.Visible;
        DbNameInput.Text = "";
        DbStatusText.Text = "";
    }

    private async void CreateDbBtn_Click(object sender, RoutedEventArgs e)
    {
        var dbName = DbNameInput.Text.Trim().Replace(" ", "_").ToLower();
        var dbUser = DbUserInput.Text.Trim().Replace(" ", "_").ToLower();
        var dbPass = DbPassInput.Password;

        if (string.IsNullOrWhiteSpace(dbName)) { DbStatusText.Text = "Enter a database name"; return; }
        if (string.IsNullOrWhiteSpace(dbUser)) { DbStatusText.Text = "Enter a database user"; return; }
        if (string.IsNullOrWhiteSpace(dbPass)) { DbStatusText.Text = "Enter a database password"; return; }

        CreateDbBtn.IsEnabled = false;
        try
        {
            var (ok, msg) = await _xampp.CreateDatabase(dbName);
            if (!ok) { DbStatusText.Text = msg; return; }

            var (userOk, userMsg) = await _xampp.CreateDatabaseUser(dbUser, dbPass, dbName);
            if (!userOk) { DbStatusText.Text = userMsg; return; }

            DbStatusText.Text = $"Created database '{dbName}' with user '{dbUser}'";
            AddDbPanel.Visibility = Visibility.Collapsed;
            DbNameInput.Text = "";
            DbUserInput.Text = "";
            DbPassInput.Password = "";
            LoadDatabases();
        }
        finally { CreateDbBtn.IsEnabled = true; }
    }

    private void CancelDbBtn_Click(object sender, RoutedEventArgs e) =>
        AddDbPanel.Visibility = Visibility.Collapsed;

    // ── phpMyAdmin (per-database) ──────────────────────────────────
    private void PhpMyAdminDbBtn_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is DbItem item)
        {
            try
            {
                var url = $"https://localhost/phpmyadmin/index.php?route=/database/structure&db={item.Name}";
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch { StatusBar.Text = "Could not open browser"; }
        }
    }

    // ── phpMyAdmin (global) ────────────────────────────────────────
    private void PhpMyAdminBtn_Click(object sender, RoutedEventArgs e)
    {
        try { Process.Start(new ProcessStartInfo("https://localhost/phpmyadmin") { UseShellExecute = true }); }
        catch { StatusBar.Text = "Could not open browser"; }
    }

    // ── Delete Database ────────────────────────────────────────────
    private async void DeleteDbBtn_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is DbItem item)
        {
            var dlg = new ContentDialog
            {
                Title = "Delete Database",
                Content = $"Delete '{item.Name}'?\nThis cannot be undone.",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = XamlRoot
            };
            if (await dlg.ShowAsync() != ContentDialogResult.Primary) return;
            var (ok, msg) = await _xampp.DeleteDatabase(item.Name);
            StatusBar.Text = msg;
            LoadDatabases();
        }
    }

    // ── User Management ────────────────────────────────────────────
    private async void UsersBtn_Click(object sender, RoutedEventArgs e)
    {
        var users = _xampp.GetUsers();
        var listBox = new ListBox { Height = 200 };
        foreach (var u in users) listBox.Items.Add(u);
        if (users.Count == 0) listBox.Items.Add("(no users found)");

        var nameBox = new TextBox { Header = "Username", PlaceholderText = "newuser" };
        var passBox = new PasswordBox { Header = "Password" };

        var panel = new StackPanel { Spacing = 12 };
        panel.Children.Add(new TextBlock { Text = "Existing Users:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        panel.Children.Add(listBox);
        panel.Children.Add(new TextBlock { Text = "Create New User:", FontWeight = Microsoft.UI.Text.FontWeights.SemiBold });
        panel.Children.Add(nameBox);
        panel.Children.Add(passBox);

        var dlg = new ContentDialog
        {
            Title = "Database Users",
            Content = new ScrollViewer { Content = panel, MaxHeight = 500 },
            PrimaryButtonText = "Create User",
            SecondaryButtonText = "Delete Selected",
            CloseButtonText = "Close",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = XamlRoot
        };

        var result = await dlg.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            var u = nameBox.Text.Trim();
            var p = passBox.Password;
            if (!string.IsNullOrWhiteSpace(u) && !string.IsNullOrWhiteSpace(p))
            {
                var (ok, msg) = await _xampp.CreateUser(u, p);
                StatusBar.Text = msg;
            }
        }
        else if (result == ContentDialogResult.Secondary && listBox.SelectedItem is string sel)
        {
            var (ok, msg) = await _xampp.DeleteUser(sel);
            StatusBar.Text = msg;
        }
    }
}

public class DbItem
{
    public string Name { get; set; } = "";
}
