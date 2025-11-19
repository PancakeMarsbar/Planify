using System;
using System.Collections.Generic;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;

using Microsoft.Maui.Layouts;
using Planify.Models;
using Planify.Services;
using Planify.ViewModels.V2;
using Planify.Pages.Popup;

namespace Planify.Pages
{

    public class AccountsPage : ContentPage
    {
        private readonly FlexLayout flex;
        private readonly AccountViewModel viewModel;

        public AccountsPage()
        {
            Title = "Accounts";
            viewModel = new AccountViewModel();

            flex = new FlexLayout
            {
                Direction = FlexDirection.Row,
                Wrap = FlexWrap.Wrap,
                JustifyContent = FlexJustify.Start,
                AlignItems = FlexAlignItems.Start,
                Margin = new Thickness(10),
            };

            var createUserButton = new Button { Text = "Create New User" };
            createUserButton.Clicked += async (s, e) => await ShowPopup();

            Content = new VerticalStackLayout
                {
                    new ScrollView { Content = flex },
                    createUserButton
                };

            BuildUserCards();
        }

        private void BuildUserCards()
        {
            flex.Children.Clear();

            foreach (var user in viewModel.Users)
            {
                var name = new Label { Text = user.Username, FontSize = 14 };
                var role = new Label { Text = user.IsAdmin ? "Admin" : "User", FontSize = 14 };
                var image = new Image { Source = "missingpicture.jpg", WidthRequest = 60, HeightRequest = 60 };

                var editUserButton = new Button
                {
                    Text = "..."
                }; editUserButton.Clicked += async (s, e) =>
                {
                    var options = new System.Collections.Generic.List<string>
                {
                    "Edit User",          // <- ét prompt
                    "Remove User"
                };
                    var choice = await DisplayActionSheet("User Controll", "Close", null, options.ToArray());
                    switch (choice)
                    {
                        case "Edit User":
                            {
                                await EditUser(user); 
                            }
                            break;
                        case "Remove User":
                            {
                                await RemoveUser(user);  
                            }
                            break;
                    }
                   
                };

                var infoStack = new VerticalStackLayout
                {
                    Spacing = 10,
                    Children = { name, role }
                };

                var personLayout = new HorizontalStackLayout
                {
                    Spacing = 10,
                    Children = { image, infoStack, editUserButton }
                };

                var frame = new Border
                {
                    Content = personLayout,
                    WidthRequest = 175,
                    Padding = 10,
                    Margin = 5
                };

                flex.Children.Add(frame);
            }
        }

        private async Task ShowPopup()
        {
            var result = await this.ShowPopupAsync<UserAccount>(new CreateUserPopup());
            if (result.Result != null)
            {
                var user = result;
                viewModel.CreateUserCommand.Execute(result.Result);
            }

        }

        private async Task EditUser(UserAccount user)
        {
            viewModel.UpdateUserCommand.Execute(user);    
        }

        private async Task RemoveUser(UserAccount user)
        {
            viewModel.DeleteUserCommand.Execute(user);
        }
    }
}
