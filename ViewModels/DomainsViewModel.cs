/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using System.Collections.ObjectModel;
using XamppMultidomainManager.Models;
using XamppMultidomainManager.Services;

namespace XamppMultidomainManager.ViewModels;

public class DomainsViewModel : BaseViewModel
{
    private readonly DatabaseService _dbService;
    private readonly HostsFileService _hostsService;
    public ObservableCollection<VirtualHost> Hosts { get; } = new();

    private string _newDomain = string.Empty;
    private string _newRoot = string.Empty;
    private string _newAlias = string.Empty;
    private VirtualHost? _selectedHost;
    private bool _isEditMode;
    private bool _isAdding;

    public string NewDomain
    {
        get => _newDomain;
        set => SetProperty(ref _newDomain, value);
    }

    public string NewRoot
    {
        get => _newRoot;
        set => SetProperty(ref _newRoot, value);
    }

    public string NewAlias
    {
        get => _newAlias;
        set => SetProperty(ref _newAlias, value);
    }

    public VirtualHost? SelectedHost
    {
        get => _selectedHost;
        set => SetProperty(ref _selectedHost, value);
    }

    public bool IsEditMode
    {
        get => _isEditMode;
        set => SetProperty(ref _isEditMode, value);
    }

    public bool IsAdding
    {
        get => _isAdding;
        set => SetProperty(ref _isAdding, value);
    }

    public DomainsViewModel(DatabaseService dbService, HostsFileService hostsService)
    {
        _dbService = dbService;
        _hostsService = hostsService;
    }

    public void LoadHosts()
    {
        Hosts.Clear();
        foreach (var host in _dbService.GetAllHosts())
            Hosts.Add(host);
    }

    public void AddHost()
    {
        if (string.IsNullOrWhiteSpace(NewDomain) || string.IsNullOrWhiteSpace(NewRoot))
            return;

        var host = new VirtualHost
        {
            DomainName = NewDomain.Trim(),
            DocumentRoot = NewRoot.Trim(),
            ServerAlias = string.IsNullOrWhiteSpace(NewAlias) ? null : NewAlias.Trim(),
            Enabled = true
        };

        _dbService.AddHost(host);

        try
        {
            if (!_hostsService.DomainExists(host.DomainName))
                _hostsService.AddDomain(host.DomainName);
        }
        catch { }

        NewDomain = string.Empty;
        NewRoot = string.Empty;
        NewAlias = string.Empty;
        IsAdding = false;
        LoadHosts();
    }

    public void ToggleHost(VirtualHost host)
    {
        host.Enabled = !host.Enabled;
        _dbService.UpdateHost(host);

        try
        {
            if (host.Enabled)
            {
                if (!_hostsService.DomainExists(host.DomainName))
                    _hostsService.AddDomain(host.DomainName);
            }
            else
            {
                _hostsService.RemoveDomain(host.DomainName);
            }
        }
        catch { }

        LoadHosts();
    }

    public void DeleteHost(VirtualHost host)
    {
        _dbService.DeleteHost(host.Id);
        try { _hostsService.RemoveDomain(host.DomainName); } catch { }
        LoadHosts();
    }

    public void EditHost(VirtualHost host)
    {
        SelectedHost = host;
        NewDomain = host.DomainName;
        NewRoot = host.DocumentRoot;
        NewAlias = host.ServerAlias ?? string.Empty;
        IsEditMode = true;
    }

    public void SaveEdit()
    {
        if (SelectedHost == null) return;

        SelectedHost.DomainName = NewDomain.Trim();
        SelectedHost.DocumentRoot = NewRoot.Trim();
        SelectedHost.ServerAlias = string.IsNullOrWhiteSpace(NewAlias) ? null : NewAlias.Trim();
        SelectedHost.UpdatedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _dbService.UpdateHost(SelectedHost);

        NewDomain = string.Empty;
        NewRoot = string.Empty;
        NewAlias = string.Empty;
        IsEditMode = false;
        SelectedHost = null;
        LoadHosts();
    }

    public void CancelEdit()
    {
        NewDomain = string.Empty;
        NewRoot = string.Empty;
        NewAlias = string.Empty;
        IsEditMode = false;
        SelectedHost = null;
        IsAdding = false;
    }
}
