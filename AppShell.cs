using Microsoft.Maui.Controls;
using Planify.Pages;
using Planify.Services;

namespace Planify
{
    public class AppShell : Shell
    {
        public AppShell()
        {
            Title = "Planify";

            var tabBar = new TabBar();

            // Common items for all users
            tabBar.Items.Add(new ShellContent { Title = "Board", Route = "BoardPage", ContentTemplate = new DataTemplate(() => new BoardPage()) });
            tabBar.Items.Add(new ShellContent { Title = "Floors", Route = "FloorPage", ContentTemplate = new DataTemplate(() => new FloorPage()) });
            tabBar.Items.Add(new ShellContent { Title = "ClaimsView", Route = "ClaimsView", ContentTemplate = new DataTemplate(() => new ClaimsView()) });
            tabBar.Items.Add(new ShellContent { Title = "Accounts", Route = "AccountPage", ContentTemplate = new DataTemplate(() => new AccountsPage()) });

            // Admin-only items
            if (AppRepository.Instance.IsAdmin)
            {
                tabBar.Items.Add(new ShellContent { Title = "Settings", Route = "SettingsPage", ContentTemplate = new DataTemplate(() => new SettingsPage()) });
            }

            Items.Add(tabBar);
        }
        protected override async void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);

            // Block access if somehow navigating manually to admin page
            if (!AppRepository.Instance.IsAdmin && args.Target.Location.OriginalString.Contains("SettingsPage"))
            {
                args.Cancel();
                await Application.Current.MainPage.DisplayAlert("Access Denied", "Admin only page", "OK");
            }
        }
    }
}
