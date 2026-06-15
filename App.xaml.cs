/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using XamppMultidomainManager.Pages;
using XamppMultidomainManager.Services;

namespace XamppMultidomainManager;

public partial class App : Application
{
    private Window? _window;

    public static DatabaseService DatabaseService { get; } = new();
    public static XamppService XamppService { get; } = new();
    public static HostsFileService HostsFileService { get; } = new();
    public static SslService SslService { get; } = new(XamppService);
    public static VirtualHostService VirtualHostService { get; } = new(DatabaseService, XamppService, SslService);

    public static T GetService<T>() where T : class
    {
        if (typeof(T) == typeof(DatabaseService)) return (T)(object)DatabaseService;
        if (typeof(T) == typeof(XamppService)) return (T)(object)XamppService;
        if (typeof(T) == typeof(HostsFileService)) return (T)(object)HostsFileService;
        if (typeof(T) == typeof(SslService)) return (T)(object)SslService;
        if (typeof(T) == typeof(VirtualHostService)) return (T)(object)VirtualHostService;
        throw new InvalidOperationException($"Service {typeof(T).Name} not registered");
    }

    public App()
    {
        Environment.SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY", AppContext.BaseDirectory);
        InitializeComponent();
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
        AppDomain.CurrentDomain.UnhandledException += (_, e) => Logger.Log(e.ExceptionObject as Exception ?? new Exception("Unknown unhandled error"));
        UnhandledException += (_, e) => { Logger.Log(e.Exception); e.Handled = true; };
    }

    private static void OnProcessExit(object? sender, EventArgs e)
    {
        StopServices();
    }

    private static void StopServices()
    {
        var xamppPath = @"C:\xampp";

        // Gracefully stop Apache via httpd -k shutdown
        try
        {
            var httpd = Path.Combine(xamppPath, "apache", "bin", "httpd.exe");
            if (File.Exists(httpd))
            {
                using var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = httpd,
                        Arguments = "-k shutdown",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p.Start();
                p.WaitForExit(10000);
            }
        }
        catch { }

        // Gracefully stop MySQL via mysqladmin
        try
        {
            var mysqladmin = Path.Combine(xamppPath, "mysql", "bin", "mysqladmin.exe");
            if (File.Exists(mysqladmin))
            {
                using var p = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = mysqladmin,
                        Arguments = "-u root shutdown",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                p.Start();
                p.WaitForExit(10000);
            }
        }
        catch { }
    }

    private static void KillProcesses(params string[] names)
    {
        foreach (var name in names)
        {
            try
            {
                foreach (var proc in Process.GetProcessesByName(name))
                {
                    try
                    {
                        if (!proc.HasExited)
                            proc.Kill();
                    }
                    catch { }
                }
            }
            catch { }
        }
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        if (!InstallPage.IsInstalled())
        {
            _window = new Window
            {
                Content = new InstallPage(),
                Title = "Xampp Multidomain Manager — Install"
            };
            _window.Activate();
            return;
        }

        // Graceful stop first
        StopServices();

        // Force-kill any remaining orphaned processes
        KillProcesses("httpd", "mysqld");

        _window = new MainWindow();
        _window.Activate();

        // Auto-start services after UI is ready
        _ = Task.Run(async () =>
        {
            await Task.Delay(500);
            try
            {
                if (!XamppService.GetApacheStatus())
                    await XamppService.StartApache();
            }
            catch { }

            try
            {
                if (!XamppService.GetMySqlStatus())
                    await XamppService.StartMySql();
            }
            catch { }
        });
    }
}
