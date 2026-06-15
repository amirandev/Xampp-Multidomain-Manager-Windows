/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using System.Collections.ObjectModel;
using XamppMultidomainManager.Models;
using XamppMultidomainManager.Services;

namespace XamppMultidomainManager.ViewModels;

public class DatabasesViewModel : BaseViewModel
{
    private readonly XamppService _xamppService;
    public ObservableCollection<DatabaseInfo> Databases { get; } = new();
    private bool _isLoading;
    private string _searchText = string.Empty;

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            SetProperty(ref _searchText, value);
            FilterDatabases();
        }
    }

    private List<DatabaseInfo> _allDatabases = new();

    public DatabasesViewModel(XamppService xamppService)
    {
        _xamppService = xamppService;
    }

    public void LoadDatabases()
    {
        IsLoading = true;
        try
        {
            var names = _xamppService.GetDatabases();
            _allDatabases = names.Select(name => new DatabaseInfo { Name = name }).ToList();
            FilterDatabases();
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void FilterDatabases()
    {
        Databases.Clear();
        var filtered = string.IsNullOrWhiteSpace(SearchText)
            ? _allDatabases
            : _allDatabases.Where(d => d.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));

        foreach (var db in filtered)
            Databases.Add(db);
    }

    public void Refresh()
    {
        LoadDatabases();
    }
}
