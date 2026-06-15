/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using Microsoft.UI.Xaml.Controls;
using System.IO;

namespace XamppMultidomainManager.Pages;

public sealed partial class PhpIniPage : Page
{
    private readonly string _phpIniPath = @"C:\xampp\php\php.ini";

    public PhpIniPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        LoadFile();
    }

    private void LoadFile()
    {
        try
        {
            Editor.Text = File.ReadAllText(_phpIniPath);
            StatusBar.Text = $"Loaded: {_phpIniPath}";
        }
        catch (FileNotFoundException)
        {
            Editor.Text = string.Empty;
            StatusBar.Text = $"File not found: {_phpIniPath}";
        }
        catch (DirectoryNotFoundException)
        {
            Editor.Text = string.Empty;
            StatusBar.Text = $"Directory not found: {Path.GetDirectoryName(_phpIniPath)}";
        }
        catch (IOException ex)
        {
            Editor.Text = string.Empty;
            StatusBar.Text = $"Error reading file: {ex.Message}";
        }
    }

    private void ReloadBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        LoadFile();
    }

    private void SaveBtn_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        try
        {
            File.WriteAllText(_phpIniPath, Editor.Text);
            StatusBar.Text = "php.ini saved successfully.";
        }
        catch (IOException ex)
        {
            StatusBar.Text = $"Error saving file: {ex.Message}";
        }
        catch (UnauthorizedAccessException)
        {
            StatusBar.Text = "Access denied. Run as Administrator to save php.ini.";
        }
    }
}
