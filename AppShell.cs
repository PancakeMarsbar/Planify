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

            Items.Add(new TabBar
            {
                Items =
                {
                    new ShellContent{ Title="Board",        Route="BoardPage",    ContentTemplate=new DataTemplate(()=> new BoardPage()) },
                    new ShellContent{ Title="Floors",       Route="FloorPage",    ContentTemplate=new DataTemplate(()=> new FloorPage()) },
                    new ShellContent{ Title="Settings",     Route="SettingsPage", ContentTemplate=new DataTemplate(()=> new SettingsPage()) },
                    new ShellContent{ Title="ClaimsView",   Route="ClaimsView",   ContentTemplate=new DataTemplate(()=> new ClaimsView()) },
                    new ShellContent{ Title="Accounts",     Route="AccountPage",  ContentTemplate=new DataTemplate(()=> new AccountsPage())},
                }
            });
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
