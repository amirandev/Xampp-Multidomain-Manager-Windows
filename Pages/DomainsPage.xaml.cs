/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using XamppMultidomainManager.Models;
using XamppMultidomainManager.Services;

namespace XamppMultidomainManager.Pages;

public sealed partial class DomainsPage : Page
{
    private readonly DatabaseService _dbService = App.GetService<DatabaseService>();
    private readonly HostsFileService _hostsService = App.GetService<HostsFileService>();
    private readonly XamppService _xamppService = App.GetService<XamppService>();
    private readonly VirtualHostService _vhostService = App.GetService<VirtualHostService>();
    private readonly SslService _sslService = App.GetService<SslService>();
    private VirtualHost? _editingHost;

    private static string XamppPath => @"C:\xampp";

    public DomainsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        LoadHosts();
    }

    private void SetPageLoading(bool loading)
    {
        if (loading)
        {
            PageLoadingRing.Visibility = Visibility.Visible;
            PageLoadingRing.IsActive = true;
            AddBtn.IsEnabled = false;
        }
        else
        {
            PageLoadingRing.IsActive = false;
            PageLoadingRing.Visibility = Visibility.Collapsed;
            AddBtn.IsEnabled = true;
        }
    }

    private static void SetButtonLoading(Button btn, TextBlock label, FontIcon icon, ProgressRing ring, string loadingText)
    {
        btn.IsEnabled = false;
        label.Text = loadingText;
        icon.Visibility = Visibility.Collapsed;
        ring.Visibility = Visibility.Visible;
        ring.IsActive = true;
    }

    private static void SetButtonReady(Button btn, TextBlock label, FontIcon icon, ProgressRing ring, string readyText)
    {
        btn.IsEnabled = true;
        label.Text = readyText;
        icon.Visibility = Visibility.Visible;
        ring.IsActive = false;
        ring.Visibility = Visibility.Collapsed;
    }

    private void LoadHosts()
    {
        var hosts = _dbService.GetAllHosts();
        if (hosts.Count == 0)
        {
            var existing = _vhostService.ParseExistingVhostsConfig();
            if (existing.Count > 0)
            {
                _dbService.ImportHosts(existing);
                hosts = _dbService.GetAllHosts();
            }
        }
        HostsList.ItemsSource = hosts;
        EmptyState.Visibility = hosts.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static string AutoRoot(string domain) =>
        $@"{XamppPath}\htdocs\websites\{domain}\public";

    private void AddBtn_Click(object sender, RoutedEventArgs e)
    {
        _editingHost = null;
        FormTitle.Text = "Add New Domain";
        DomainInput.Text = string.Empty;
        RootInput.Text = string.Empty;
        AliasInput.Text = string.Empty;
        DomainInput.IsReadOnly = false;
        FormPanel.Visibility = Visibility.Visible;
    }

    private void DomainInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (_editingHost == null)
        {
            var domain = DomainInput.Text.Trim();
            if (!string.IsNullOrWhiteSpace(domain))
            {
                var clean = domain.Replace(" ", "").ToLower();
                RootInput.Text = AutoRoot(clean);
            }
            else
            {
                RootInput.Text = string.Empty;
            }
        }
    }

    private async void SaveBtn_Click(object sender, RoutedEventArgs e)
    {
        SetButtonLoading(SaveBtn, SaveBtnLabel, SaveBtnIcon, SaveBtnRing, "Saving...");
        try
        {
            var domain = DomainInput.Text.Trim().ToLower();
            var root = RootInput.Text.Trim();

            if (string.IsNullOrWhiteSpace(domain) || string.IsNullOrWhiteSpace(root))
                return;

            if (_editingHost == null)
            {
                var host = new VirtualHost
                {
                    DomainName = domain,
                    DocumentRoot = root,
                    ServerAlias = string.IsNullOrWhiteSpace(AliasInput.Text) ? null : AliasInput.Text.Trim(),
                    Enabled = true
                };
                _dbService.AddHost(host);

                try
                {
                    if (!_hostsService.DomainExists(host.DomainName))
                        _hostsService.AddDomain(host.DomainName);
                }
                catch (Exception ex) { Logger.Log("DomainsPage.Save.AddDomain", ex); }

                await _sslService.GenerateCertificate(host.DomainName);
                CreateBoilerplate(root, domain);
            }
            else
            {
                var oldDomain = _editingHost.DomainName;

                _editingHost.DomainName = domain;
                _editingHost.DocumentRoot = AutoRoot(domain);
                _editingHost.ServerAlias = string.IsNullOrWhiteSpace(AliasInput.Text) ? null : AliasInput.Text.Trim();
                _editingHost.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                _dbService.UpdateHost(_editingHost);

                try
                {
                    if (oldDomain != domain)
                    {
                        _hostsService.RemoveDomain(oldDomain);
                        if (!_hostsService.DomainExists(domain))
                            _hostsService.AddDomain(domain);

                        _sslService.DeleteCertificate(oldDomain);
                        await _sslService.GenerateCertificate(domain);
                    }
                }
                catch (Exception ex) { Logger.Log("DomainsPage.Save.RenameHosts", ex); }

                if (!Directory.Exists(root))
                    CreateBoilerplate(root, domain);
            }

            var (valid, msg) = await _vhostService.RewriteConfig();
            if (!valid)
            {
                var dialog = new ContentDialog
                {
                    Title = "Configuration Error",
                    Content = msg,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            var apacheWasRunning = _xamppService.GetApacheStatus();
            if (apacheWasRunning)
            {
                await _xamppService.StopApache();
                await Task.Delay(1500);
            }
            await _xamppService.StartApache();
        }
        finally
        {
            FormPanel.Visibility = Visibility.Collapsed;
            _editingHost = null;
            SetButtonReady(SaveBtn, SaveBtnLabel, SaveBtnIcon, SaveBtnRing, "Save");
            LoadHosts();
        }
    }

    private static void CreateBoilerplate(string root, string domain)
    {
        try
        {
            if (!Directory.Exists(root))
                Directory.CreateDirectory(root);

            var indexPath = Path.Combine(root, "index.php");
            if (!File.Exists(indexPath))
            {
                var title = domain.Split('.')[0];
                File.WriteAllText(indexPath, $$"""
                    <!DOCTYPE html>
                    <html lang="en">
                    <head>
                        <meta charset="UTF-8">
                        <meta name="viewport" content="width=device-width, initial-scale=1.0">
                        <title>{{title}}</title>
                        <style>
                            * { margin: 0; padding: 0; box-sizing: border-box; }
                            body { font-family: 'Segoe UI', system-ui, -apple-system, sans-serif; background: #0f0f1a; color: #e0e0e0; display: flex; align-items: center; justify-content: center; min-height: 100vh; }
                            .container { text-align: center; padding: 2rem; }
                            h1 { font-size: 3rem; font-weight: 300; margin-bottom: 0.5rem; color: #fff; }
                            h1 span { color: #6c5ce7; }
                            p { color: #888; font-size: 1.1rem; }
                            .domain { margin-top: 2rem; padding: 0.75rem 1.5rem; background: #1a1a2e; border-radius: 12px; display: inline-block; font-size: 0.9rem; color: #6c5ce7; }
                        </style>
                    </head>
                    <body>
                        <div class="container">
                            <h1>Welcome to <span>{{title}}</span></h1>
                            <p>Your site is ready. Start building.</p>
                            <div class="domain">{{domain}}</div>
                        </div>
                    </body>
                    </html>
                    """);
            }
        }
        catch (Exception ex) { Logger.Log("DomainsPage.CreateBoilerplate", ex); }
    }

    private void OpenDomainBtn_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is VirtualHost host)
        {
            var url = $"https://{host.DomainName}";
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); }
            catch (Exception ex) { Logger.Log("DomainsPage.OpenDomain", ex); }
        }
    }

    private void OpenFolderBtn_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is VirtualHost host)
        {
            try
            {
                if (Directory.Exists(host.DocumentRoot))
                    Process.Start("explorer.exe", host.DocumentRoot);
            }
            catch (Exception ex) { Logger.Log("DomainsPage.OpenFolder", ex); }
        }
    }

    private async void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if ((sender as ToggleSwitch)?.DataContext is VirtualHost host)
        {
            SetPageLoading(true);
            try
            {
                _dbService.UpdateHost(host);

                if (host.Enabled)
                {
                    if (!_hostsService.DomainExists(host.DomainName))
                        _hostsService.AddDomain(host.DomainName);
                }
                else
                {
                    _hostsService.RemoveDomain(host.DomainName);
                }

                var (valid, msg) = await _vhostService.RewriteConfig();
                if (!valid)
                {
                    SetPageLoading(false);
                    return;
                }
                if (_xamppService.GetApacheStatus())
                {
                    await _xamppService.StopApache();
                    await Task.Delay(1500);
                }
                await _xamppService.StartApache();
            }
            catch (Exception ex) { Logger.Log("DomainsPage.ToggleSwitch", ex); }
            finally
            {
                SetPageLoading(false);
            }
        }
    }

    private void CancelBtn_Click(object sender, RoutedEventArgs e)
    {
        FormPanel.Visibility = Visibility.Collapsed;
        _editingHost = null;
    }

    private void RenameBtn_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is VirtualHost host)
        {
            _editingHost = host;
            FormTitle.Text = "Rename Domain";
            DomainInput.Text = host.DomainName;
            DomainInput.IsReadOnly = false;
            RootInput.Text = AutoRoot(host.DomainName);
            AliasInput.Text = host.ServerAlias ?? string.Empty;
            FormPanel.Visibility = Visibility.Visible;
        }
    }

    private async void DeleteBtn_Click(object sender, RoutedEventArgs e)
    {
        if ((sender as FrameworkElement)?.DataContext is VirtualHost host)
        {
            var dialog = new ContentDialog
            {
                Title = "Delete Domain",
                Content = $"Delete '{host.DomainName}'?\n\nThis will remove:\n• SQLite record\n• Apache vhost config\n• Windows hosts file entry\n• SSL certificate",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await dialog.ShowAsync();
            if (result != ContentDialogResult.Primary)
                return;

            SetPageLoading(true);

            _dbService.DeleteHost(host.Id);

            try { _hostsService.RemoveDomain(host.DomainName); }
            catch (Exception ex) { Logger.Log("DomainsPage.Delete.RemoveDomain", ex); }

            _sslService.DeleteCertificate(host.DomainName);

            var (valid, msg) = await _vhostService.RewriteConfig();
            if (!valid)
            {
                SetPageLoading(false);
                LoadHosts();
                return;
            }

            try
            {
                if (_xamppService.GetApacheStatus())
                {
                    await _xamppService.StopApache();
                    await Task.Delay(1500);
                }
                await _xamppService.StartApache();
            }
            catch (Exception ex) { Logger.Log("DomainsPage.Delete.RestartApache", ex); }

            SetPageLoading(false);
            LoadHosts();
        }
    }

    private async void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        SetButtonLoading(RefreshBtn, RefreshBtnLabel, RefreshBtnIcon, RefreshBtnRing, "Refreshing...");
        try
        {
            var (valid, msg) = await _vhostService.RewriteConfig();
            if (!valid)
            {
                var dialog = new ContentDialog
                {
                    Title = "Configuration Error",
                    Content = msg,
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
                return;
            }

            var apacheWasRunning = _xamppService.GetApacheStatus();
            if (apacheWasRunning)
            {
                await _xamppService.StopApache();
                await Task.Delay(1500);
            }
            await _xamppService.StartApache();
        }
        catch (Exception ex) { Logger.Log("DomainsPage.RefreshConfig", ex); }
        finally
        {
            SetButtonReady(RefreshBtn, RefreshBtnLabel, RefreshBtnIcon, RefreshBtnRing, "Refresh Config");
        }
    }
}
