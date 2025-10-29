using Microsoft.Maui.Controls;

namespace Planify.Pages
{
    public class MainPage : ContentPage
    {
        public MainPage()
        {
            Title = "Planify";
            Content = new VerticalStackLayout
            {
                Padding = 16,
                Children =
                {
                    new Label{ Text="Velkommen til Planify", Style=(Style)Application.Current.Resources["H1"] },
                    new Label{ Text="Faner: Board, Floors, Settings", Style=(Style)Application.Current.Resources["Small"] }
                }
            };
        }
    }
}
