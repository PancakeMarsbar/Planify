using Microsoft.Maui.Controls;
using Planify.Pages;
using Planify.Services;
using CommunityToolkit.Maui.Views;

namespace Planify
{
    public class AppShell : Shell
    {
        public AppShell()
        {
            var repo = AppRepository.Instance;

            FlyoutBehavior = FlyoutBehavior.Flyout;

            Title = "Planify";

            var tabBar = new TabBar();

            // Common items for all users
            tabBar.Items.Add(new ShellContent { Title = "Board", Route = "BoardPage", ContentTemplate = new DataTemplate(() => new BoardPage()) });
            tabBar.Items.Add(new ShellContent { Title = "Floors", Route = "FloorPage", ContentTemplate = new DataTemplate(() => new FloorPage()) });
            //tabBar.Items.Add(new ShellContent { Title = "ClaimsView", Route = "ClaimsView", ContentTemplate = new DataTemplate(() => new Claims()) });
            
            // Admin-only items
            if (repo.IsAdmin)
            {
                tabBar.Items.Add(new ShellContent { Title = "Accounts", Route = "AccountPage", ContentTemplate = new DataTemplate(() => new AccountsPage()) });
            }

            Items.Add(tabBar);


            // Create the header layout
            var profileImage = new Image
            {
                WidthRequest = 50,
                HeightRequest = 50,
                Aspect = Aspect.AspectFill,
                Source = "missingpicture.jpg"   // ImageSource
            };

            var userName = new Label
            {
                Text = repo.CurrentUser,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16
            };

            var userRole = new Label
            {
                Text = repo.IsAdmin ? "Admin" : "User",
                FontSize = 14
            };

            var logoutButton = new Button
            {
                Text = "Sign Out",
                Padding = new Thickness(0),
                FontSize = 14
            };
            logoutButton.Clicked += (sender, e) => { repo.Logout();  };

            var header = new Grid
            {
                Padding = 20,
                RowDefinitions =
            {
                new RowDefinition(),  // name
                new RowDefinition(),  // role
                new RowDefinition()   // logout
            },
                ColumnDefinitions =
            {
                new ColumnDefinition(){ Width = 60 },
                new ColumnDefinition(){ Width = GridLength.Star }
            }
            };

            header.Add(profileImage, 0, 0);
            Grid.SetRowSpan(profileImage, 3);

            header.Add(userName, 1, 0);
            header.Add(userRole, 1, 1);
            header.Add(logoutButton, 1, 2);

            // Assign to Shell
            this.FlyoutHeader = header;
            

            Items.Add(new FlyoutItem { Title = "Board", Route = "BoardPage", Items = { new ShellContent { ContentTemplate = new DataTemplate(() => new BoardPage() )} } });
            Items.Add(new FlyoutItem { Title = "Floors", Route = "FloorPage", Items = { new ShellContent { ContentTemplate = new DataTemplate(() => new FloorPage() )} } });
            //Items.Add(new FlyoutItem { Title = "ClaimsView", Route = "ClaimsView", Items = { new ShellContent { ContentTemplate = new DataTemplate(() => new Claims() )} } });
            
            // Admin-only items
            if (repo.IsAdmin)
            {
                Items.Add(new FlyoutItem { Title = "Accounts", Route = "AccountPage", Items = { new ShellContent { ContentTemplate = new DataTemplate(() => new AccountsPage()) } } });
            }



        }
        protected override async void OnNavigating(ShellNavigatingEventArgs args)
        {
            base.OnNavigating(args);
                
            // Block access if somehow navigating manually to admin page
            if (!AppRepository.Instance.IsAdmin && args.Target.Location.OriginalString.Contains("AccountPage"))
            {
                args.Cancel();
                await Application.Current.MainPage.DisplayAlert("Access Denied", "Admin only page", "OK");
                await Shell.Current.GoToAsync("//SettingsPage");
            }
        }
    }
}
