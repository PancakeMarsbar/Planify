using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Layouts;
using Planify.Models;
using Planify.Services;

namespace Planify.Pages
{
    public class AccountsPage : ContentPage
    {
        private List<UserAccount> Users = AppRepository.Instance.Users;

        public AccountsPage()
        {
            Title = "Accounts";

            var flex = new FlexLayout()
            {
                Direction = FlexDirection.Row,
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustify.Start,
                AlignItems = FlexAlignItems.Start,
                Margin = new Thickness(10),
            };
            


            foreach (var user in Users)
            {
                var name = new Label
                {
                    Text = user.Username,
                    FontSize = 14                    
                };

                var role = new Label
                {
                    Text = user.IsAdmin ? "Admin" : "User",
                    FontSize = 14
                };

                var image = new Image
                {
                    Source = "missingpicture.jpg",
                    WidthRequest = 60,
                    HeightRequest = 60

                };

               

                var infoStack = new VerticalStackLayout
                {
                    Spacing = 10,
                    Children = { name, role }
                };

                var personLayout = new HorizontalStackLayout
                {
                    Spacing = 10,
                    Children = { image, infoStack }
                };

                var frame = new Border
                {
                    Content = personLayout,
                    WidthRequest = 300,
                    Padding = 10,
                    Margin = 5,
                };

                flex.Children.Add(frame);
            }

            var CreateUserButton = new Button { Text = "Create New User " };
            CreateUserButton.Clicked += (s, e) => ShowPopup() ;



            Content = new VerticalStackLayout {
                new ScrollView { Content = flex, },
                CreateUserButton
            };
            }
        private async void ShowPopup()
        {
            var popup = new CreateUserPopup();
            var result = await this.ShowPopupAsync(popup);
            await AppRepository.Instance.SaveAsync();
        }

    }
}
