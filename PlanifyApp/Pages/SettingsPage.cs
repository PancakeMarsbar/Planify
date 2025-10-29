using Microsoft.Maui.Controls;

namespace Planify.Pages
{
    public class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            Title = "Settings";
            Content = new VerticalStackLayout
            {
                Padding = 16,
                Children =
                {
                    new Label{ Text="Indstillinger (MVP)", Style=(Style)Application.Current.Resources["H1"] },
                    new Label{ Text="• ADMIN/USER roller (hardcoded i MVP)\n• CSV-eksport (kommer)\n• Kolonner/Tags konfiguration (kommer)", LineBreakMode=LineBreakMode.WordWrap }
                }
            };
        }
    }
}
