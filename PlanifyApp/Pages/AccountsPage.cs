using System;
using System.Collections.Generic;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Layouts;
using Planify.Models;
using Planify.Services;

namespace Planify.Pages
{
    public class AccountsPage : ContentPage
    {
        private readonly FlexLayout flex;

        public AccountsPage()
        {
            Title = "Accounts";

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

            foreach (var user in AppRepository.Instance.Users)
            {
                var name = new Label { Text = user.Username, FontSize = 14 };
                var role = new Label { Text = user.IsAdmin ? "Admin" : "User", FontSize = 14 };
                var image = new Image { Source = "missingpicture.jpg", WidthRequest = 60, HeightRequest = 60 };

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
                    Margin = 5
                };

                flex.Children.Add(frame);
            }
        }

        private async Task ShowPopup()
        {
            var popup = new CreateUserPopup();
            var result = await this.ShowPopupAsync(popup);
            await AppRepository.Instance.SaveAsync();
            BuildUserCards(); // reload UI after popup closes
        }
    }
}
