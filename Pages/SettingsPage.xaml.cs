/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using Microsoft.UI.Xaml.Controls;
using System.IO;

namespace XamppMultidomainManager.Pages;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        XamppStatusText.Text = Directory.Exists(@"C:\xampp")
            ? "XAMPP installation found at C:\xampp"
            : "XAMPP not found at C:\xampp";
    }
}
