/**
 *  Powered By XROW.ASIA
 *  Real Life Soultions for IT World
 *  Contact: amoswaper@gmail.com
 */
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using XamppMultidomainManager.Pages;
using XamppMultidomainManager.Services;

namespace XamppMultidomainManager;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        TitleBarIcon.ImageSource = new BitmapImage(new Uri(IconHelper.GetIconPath("Assets/icons/16px.ico")));
        AppWindow.SetIcon(IconHelper.GetIconPath("Assets/icons/32px.ico"));
    }

    private void TitleBar_PaneToggleRequested(TitleBar sender, object args)
    {
        NavView.IsPaneOpen = !NavView.IsPaneOpen;
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        NavFrame.GoBack();
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            NavFrame.Navigate(typeof(SettingsPage));
        }
        else if (args.SelectedItem is NavigationViewItem item)
        {
            switch (item.Tag)
            {
                case "dashboard":
                    NavFrame.Navigate(typeof(DashboardPage));
                    break;
                case "domains":
                    NavFrame.Navigate(typeof(DomainsPage));
                    break;
                case "databases":
                    NavFrame.Navigate(typeof(DatabasesPage));
                    break;
                case "php":
                    NavFrame.Navigate(typeof(PhpIniPage));
                    break;
                case "about":
                    NavFrame.Navigate(typeof(AboutPage));
                    break;
                default:
                    break;
            }
        }
    }
}
