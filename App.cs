using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Handlers.Items;
using Microsoft.Maui.Graphics;
using Planify.Pages;

namespace Planify
{
    public class App : Application
    {
        public App()
        {
            Resources = new ResourceDictionary
            {
                ["H1"] = new Style(typeof(Label))
                {
                    Setters =
                    {
                        new Setter{Property=Label.FontSizeProperty, Value=28d},
                        new Setter{Property=Label.FontAttributesProperty, Value=FontAttributes.Bold}
                    }
                },
                ["Small"] = new Style(typeof(Label))
                {
                    Setters =
                    {
                        new Setter{Property=Label.FontSizeProperty, Value=12d},
                        new Setter{Property=Label.TextColorProperty, Value=Colors.Gray}
                    }
                }
            };
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            bool startLoggedIn = false; // change to true later if needed and whant auto log in
            if (startLoggedIn)
            {
                return new Window(new AppShell());    // Shell-based app
            } else {
                return new Window(new LoginPage());   // ContentPage login
            }
        }
    }
}
