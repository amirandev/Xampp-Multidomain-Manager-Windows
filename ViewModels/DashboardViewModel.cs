/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using XamppMultidomainManager.Services;

namespace XamppMultidomainManager.ViewModels;

public class DashboardViewModel : BaseViewModel
{
    private readonly XamppService _xamppService;
    private readonly DatabaseService _dbService;
    private readonly VirtualHostService _vhostService;
    private readonly HostsFileService _hostsService;
    private bool _isApacheRunning;
    private bool _isMySqlRunning;
    private string _apacheStatusText = "Stopped";
    private string _mySqlStatusText = "Stopped";
    private bool _isApacheBusy;
    private bool _isMySqlBusy;
    private string _statusMessage = string.Empty;
    private int _domainCount;
    private int _databaseCount;

    public bool IsApacheRunning
    {
        get => _isApacheRunning;
        set { SetProperty(ref _isApacheRunning, value); OnPropertyChanged(nameof(ApacheStatusColor)); }
    }

    public bool IsMySqlRunning
    {
        get => _isMySqlRunning;
        set { SetProperty(ref _isMySqlRunning, value); OnPropertyChanged(nameof(MySqlStatusColor)); }
    }

    public string ApacheStatusText
    {
        get => _apacheStatusText;
        set => SetProperty(ref _apacheStatusText, value);
    }

    public string MySqlStatusText
    {
        get => _mySqlStatusText;
        set => SetProperty(ref _mySqlStatusText, value);
    }

    public bool IsApacheBusy
    {
        get => _isApacheBusy;
        set => SetProperty(ref _isApacheBusy, value);
    }

    public bool IsMySqlBusy
    {
        get => _isMySqlBusy;
        set => SetProperty(ref _isMySqlBusy, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    public string ApacheStatusColor => IsApacheRunning ? "#2ecc71" : "#e74c3c";
    public string MySqlStatusColor => IsMySqlRunning ? "#2ecc71" : "#e74c3c";

    public int DomainCount
    {
        get => _domainCount;
        set => SetProperty(ref _domainCount, value);
    }

    public int DatabaseCount
    {
        get => _databaseCount;
        set => SetProperty(ref _databaseCount, value);
    }

    public DashboardViewModel(XamppService xamppService, DatabaseService dbService,
        VirtualHostService vhostService, HostsFileService hostsService)
    {
        _xamppService = xamppService;
        _dbService = dbService;
        _vhostService = vhostService;
        _hostsService = hostsService;
    }

    public void RefreshStatus()
    {
        IsApacheRunning = _xamppService.GetApacheStatus();
        IsMySqlRunning = _xamppService.GetMySqlStatus();
        ApacheStatusText = IsApacheRunning ? "Running" : "Stopped";
        MySqlStatusText = IsMySqlRunning ? "Running" : "Stopped";
        DomainCount = _dbService.GetAllHosts().Count;
        DatabaseCount = _xamppService.GetDatabases().Count;
    }

    public async Task ToggleApache()
    {
        IsApacheBusy = true;
        try
        {
            if (IsApacheRunning)
            {
                var (success, msg) = await _xamppService.StopApache();
                StatusMessage = msg;
            }
            else
            {
                await _vhostService.RewriteConfig();
                var (success, msg) = await _xamppService.StartApache();
                StatusMessage = msg;
            }
        }
        finally
        {
            RefreshStatus();
            IsApacheBusy = false;
        }
    }

    public async Task ToggleMySql()
    {
        IsMySqlBusy = true;
        try
        {
            if (IsMySqlRunning)
            {
                var (success, msg) = await _xamppService.StopMySql();
                StatusMessage = msg;
            }
            else
            {
                var (success, msg) = await _xamppService.StartMySql();
                StatusMessage = msg;
            }
        }
        finally
        {
            RefreshStatus();
            IsMySqlBusy = false;
        }
    }

    public async Task ApplyConfig()
    {
        IsApacheBusy = true;
        try
        {
            if (IsApacheRunning)
            {
                await _xamppService.StopApache();
                await Task.Delay(1000);
            }

            await _vhostService.RewriteConfig();
            var hosts = _dbService.GetAllHosts().Where(h => h.Enabled);
            foreach (var host in hosts)
            {
                if (!_hostsService.DomainExists(host.DomainName))
                {
                    try { _hostsService.AddDomain(host.DomainName); }
                    catch { StatusMessage = $"Run as Admin to add {host.DomainName} to hosts file"; }
                }
            }

            if (!IsApacheRunning)
            {
                await _xamppService.StartApache();
            }

            StatusMessage = "Configuration applied successfully";
        }
        finally
        {
            RefreshStatus();
            IsApacheBusy = false;
        }
    }
}
