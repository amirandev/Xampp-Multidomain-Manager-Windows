/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using XamppMultidomainManager.Services;

namespace XamppMultidomainManager.Pages;

public sealed partial class DashboardPage : Page
{
    private readonly XamppService _xamppService = App.GetService<XamppService>();
    private readonly DatabaseService _dbService = App.GetService<DatabaseService>();
    private readonly VirtualHostService _vhostService = App.GetService<VirtualHostService>();
    private readonly HostsFileService _hostsService = App.GetService<HostsFileService>();

    public DashboardPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        RefreshStatus();
    }

    private void RefreshStatus()
    {
        var apacheRunning = _xamppService.GetApacheStatus();
        var mysqlRunning = _xamppService.GetMySqlStatus();

        ApacheStatusIndicator.Fill = new SolidColorBrush(
            apacheRunning ? Windows.UI.Color.FromArgb(255, 46, 204, 113) : Windows.UI.Color.FromArgb(255, 231, 76, 60));
        ApacheStatusText.Text = apacheRunning ? "Running" : "Stopped";
        SetButtonReady(ApacheBtn, ApacheBtnLabel, ApacheBtnIcon, ApacheBtnRing, apacheRunning ? "Stop Apache" : "Start Apache");

        MySqlStatusIndicator.Fill = new SolidColorBrush(
            mysqlRunning ? Windows.UI.Color.FromArgb(255, 46, 204, 113) : Windows.UI.Color.FromArgb(255, 231, 76, 60));
        MySqlStatusText.Text = mysqlRunning ? "Running" : "Stopped";
        SetButtonReady(MySqlBtn, MySqlBtnLabel, MySqlBtnIcon, MySqlBtnRing, mysqlRunning ? "Stop MySQL" : "Start MySQL");

        DomainCountText.Text = _dbService.GetAllHosts().Count.ToString();
        DatabaseCountText.Text = _xamppService.GetDatabases().Count.ToString();
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

    private async void ApacheBtn_Click(object sender, RoutedEventArgs e)
    {
        var wasRunning = _xamppService.GetApacheStatus();
        SetButtonLoading(ApacheBtn, ApacheBtnLabel, ApacheBtnIcon, ApacheBtnRing, wasRunning ? "Stopping..." : "Starting...");
        try
        {
            if (wasRunning)
            {
                var (success, msg) = await _xamppService.StopApache();
                StatusMessageText.Text = msg;
            }
            else
            {
                var (valid, msg) = await _vhostService.RewriteConfig();
                StatusMessageText.Text = msg;
                if (valid)
                {
                    var (success, msg2) = await _xamppService.StartApache();
                    StatusMessageText.Text = msg2;
                }
            }
        }
        finally
        {
            RefreshStatus();
        }
    }

    private async void MySqlBtn_Click(object sender, RoutedEventArgs e)
    {
        var wasRunning = _xamppService.GetMySqlStatus();
        SetButtonLoading(MySqlBtn, MySqlBtnLabel, MySqlBtnIcon, MySqlBtnRing, wasRunning ? "Stopping..." : "Starting...");
        try
        {
            if (wasRunning)
            {
                var (success, msg) = await _xamppService.StopMySql();
                StatusMessageText.Text = msg;
            }
            else
            {
                var (success, msg) = await _xamppService.StartMySql();
                StatusMessageText.Text = msg;
            }
        }
        finally
        {
            RefreshStatus();
        }
    }

    private async void ApplyConfig_Click(object sender, RoutedEventArgs e)
    {
        SetButtonLoading(ApplyConfigBtn, ApplyConfigBtnLabel, ApplyConfigBtnIcon, ApplyConfigBtnRing, "Applying...");
        try
        {
            if (_xamppService.GetApacheStatus())
            {
                SetButtonLoading(ApacheBtn, ApacheBtnLabel, ApacheBtnIcon, ApacheBtnRing, "Stopping...");
                await _xamppService.StopApache();
                await Task.Delay(1000);
            }

            var (valid, configMsg) = await _vhostService.RewriteConfig();
            StatusMessageText.Text = configMsg;
            if (!valid)
                return;

            var hosts = _dbService.GetAllHosts().Where(h => h.Enabled);
            foreach (var host in hosts)
            {
                if (!_hostsService.DomainExists(host.DomainName))
                {
                    try { _hostsService.AddDomain(host.DomainName); }
                    catch (Exception ex) { Logger.Log("DashboardPage.ApplyConfig.AddDomain", ex); StatusMessageText.Text = $"Run as Admin to add {host.DomainName} to hosts file"; }
                }
            }

            if (!_xamppService.GetApacheStatus())
            {
                SetButtonLoading(ApacheBtn, ApacheBtnLabel, ApacheBtnIcon, ApacheBtnRing, "Starting...");
                await _xamppService.StartApache();
            }

            StatusMessageText.Text = "Configuration applied successfully";
        }
        finally
        {
            RefreshStatus();
            SetButtonReady(ApplyConfigBtn, ApplyConfigBtnLabel, ApplyConfigBtnIcon, ApplyConfigBtnRing, "Apply Configuration");
        }
    }

    private void RefreshBtn_Click(object sender, RoutedEventArgs e)
    {
        RefreshStatus();
        StatusMessageText.Text = "Status refreshed";
    }
}
