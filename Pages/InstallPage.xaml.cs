using System.Diagnostics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace XamppMultidomainManager.Pages;

public sealed partial class InstallPage : Page
{
    private static string InstallDir => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "XamppMultidomainManager");

    public InstallPage()
    {
        InitializeComponent();
    }

    private async void InstallBtn_Click(object sender, RoutedEventArgs e)
    {
        InstallBtn.IsEnabled = false;
        InstallBtnLabel.Text = "Installing...";
        InstallBtnIcon.Visibility = Visibility.Collapsed;
        InstallBtnRing.Visibility = Visibility.Visible;
        InstallBtnRing.IsActive = true;

        try
        {
            var exePath = Environment.ProcessPath;
            if (string.IsNullOrEmpty(exePath))
            {
                StatusText.Text = "Could not determine executable path.";
                return;
            }

            var targetDir = InstallDir;
            var targetExe = Path.Combine(targetDir, "XamppMultidomainManager.exe");

            if (Directory.Exists(targetDir))
                Directory.Delete(targetDir, true);
            Directory.CreateDirectory(targetDir);

            File.Copy(exePath, targetExe, true);

            var shortcutDir = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu) +
                "\\Programs";
            var shortcut = Path.Combine(shortcutDir, "Xampp Multidomain Manager.lnk");

            var ps = $"$WS = New-Object -ComObject WScript.Shell; " +
                $"$SC = $WS.CreateShortcut('{shortcut.Replace("'", "''")}'); " +
                $"$SC.TargetPath = '{targetExe.Replace("'", "''")}'; " +
                $"$SC.WorkingDirectory = '{targetDir.Replace("'", "''")}'; " +
                $"$SC.Description = 'Xampp Multidomain Manager — Powered By XROW.ASIA'; " +
                $"$SC.Save()";

            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell",
                    Arguments = $"-Command \"{ps}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            proc.Start();
            proc.WaitForExit();

            StatusText.Text = "Installation complete! Starting application...";
            StatusText.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Windows.UI.Color.FromArgb(255, 46, 204, 113));
            await Task.Delay(1000);

            Process.Start(new ProcessStartInfo(targetExe) { UseShellExecute = true });
            Application.Current.Exit();
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Installation failed: {ex.Message}";
            InstallBtn.IsEnabled = true;
            InstallBtnLabel.Text = "Retry";
            InstallBtnIcon.Visibility = Visibility.Visible;
            InstallBtnRing.Visibility = Visibility.Collapsed;
            InstallBtnRing.IsActive = false;
        }
    }

    public static bool IsInstalled()
    {
        var exePath = Environment.ProcessPath;
        if (string.IsNullOrEmpty(exePath))
            return false;
        return exePath.StartsWith(InstallDir, StringComparison.OrdinalIgnoreCase);
    }
}
