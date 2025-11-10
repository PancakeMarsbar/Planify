using Microsoft.Maui.Controls;
using Planify.Services;

namespace Planify
{
    public class MainPage : ContentPage
    {
        private int _count;
        private readonly Button _counterBtn;
        public MainPage()
        {
            Title = "Home";

            

            var image = new Image
            {
                Source = "dotnet_bot.png",
                HeightRequest = 185,
                Aspect = Aspect.AspectFit
            };
            SemanticProperties.SetDescription(image, "dot net bot in a hovercraft number nine");

            var h1 = new Label
            {
                Text = "Hello, World!",
                Style = (Style)Application.Current.Resources["Headline"]
            };
            SemanticProperties.SetHeadingLevel(h1, SemanticHeadingLevel.Level1);

            var h2 = new Label
            {
                Text = "Welcome to \n.NET Multi-platform App UI",
                Style = (Style)Application.Current.Resources["SubHeadline"]
            };
            SemanticProperties.SetHeadingLevel(h2, SemanticHeadingLevel.Level2);
            SemanticProperties.SetDescription(h2, "Welcome to dot net Multi platform App U I");

            _counterBtn = new Button
            {
                Text = "Click me",
                HorizontalOptions = LayoutOptions.Fill
            };
            SemanticProperties.SetHint(_counterBtn, "Counts the number of times you click");
            _counterBtn.Clicked += OnCounterClicked;

            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Padding = new Thickness(30, 0),
                    Spacing = 25,
                    Children =
                    {
                        image,
                        h1,
                        h2,
                        _counterBtn
                    }
                }
            };
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            _count++;
            _counterBtn.Text = $"Clicked {_count} time{(_count == 1 ? "" : "s")}";
        }
    }
}
