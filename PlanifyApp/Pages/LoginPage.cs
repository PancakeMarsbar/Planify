using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Identity.Client;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using Planify.PlanifyApp.Services;
using Planify.Services;

namespace Planify.Pages;



public class LoginPage : ContentPage
{
    //------------------------------------------------------------------------------------------------------
    //UI DECLARATION AND CONSTRUCTION
    //------------------------------------------------------------------------------------------------------

    //private readonly ListView _claimsList;
    private readonly Button _signInButton;

    private bool _isUserLoggedIn;

    private Entry username;

    private Entry password;



    //public IEnumerable<string> IdTokensClaims { get; set; } = new[] { "No Claims found in ID Tokens" };

    public LoginPage()
    {

        Title = "Sign_In";

        var signInCommand = new Command(SignInButton_Clicked);


        Shell.SetBackButtonBehavior(this,
                new BackButtonBehavior { IsVisible = false, IsEnabled = false });

        {
            Title = "Planify";

            var header1 = new Label
            {
                Text = "Welcome to Planify",
                FontSize = 30,
                FontAttributes = FontAttributes.Bold,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.Black,
                Margin = new Thickness(0, -50, 0, 0)
            };

            var header2 = new Label
            {
                Text = "Organize your plans effortlessly",
                FontSize = 18,
                HorizontalOptions = LayoutOptions.Center,
                TextColor = Colors.Gray,
                Padding = new Thickness(0, 0, 0, 30)
            };

            var imagePlaceholder = new Image
            {
                Source = "logo2.png", // add this image to your Resources/Images folder
                WidthRequest = 400,
                HeightRequest = 400,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, -50, 0, 0),
                Aspect = Aspect.AspectFit
            };

            username = new Entry
            {
                ReturnCommand = signInCommand,
                Placeholder = "Username",
                PlaceholderColor = Colors.Gray,
                TextColor = Colors.Black,
                BackgroundColor = Colors.White,
                WidthRequest = 250,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 5)
            };

            password = new Entry
            {
                ReturnCommand = signInCommand,
                Placeholder = "Password",
                PlaceholderColor = Colors.Gray,
                TextColor = Colors.Black,
                BackgroundColor = Colors.White,
                WidthRequest = 250,
                HorizontalOptions = LayoutOptions.Center,
                IsPassword = true,
                Margin = new Thickness(0, 5, 0, 15)
            };

            _signInButton = new Button
            {
                Text = "Sign In",
                Command = signInCommand,
                BackgroundColor = Color.FromArgb("#3B82F6"), // blue accent
                TextColor = Colors.White,
                CornerRadius = 10,
                WidthRequest = 250,
                HeightRequest = 45,
                HorizontalOptions = LayoutOptions.Center,
                Margin = new Thickness(0, 10, 0, 30)
            };

            var guestLoginLabel = new Label
            {
                Text = "Login as guest",
                TextColor = Colors.Blue,
                FontSize = 16,
                HorizontalOptions = LayoutOptions.Center
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += async (s, e) =>
            {
                // your guest logic
                //AppRepository.Instance.LoginAsGuest();
                //Application.Current.MainPage = new Claims();
                Application.Current.MainPage = new FloorPageGuest();
            };

            guestLoginLabel.GestureRecognizers.Add(tapGesture);

            Content = new ScrollView
            {
                Content = new VerticalStackLayout
                {
                    Spacing = 0,
                    Padding = new Thickness(0, 0),
                    HorizontalOptions = LayoutOptions.Center,
                    Children =
                    {
                        imagePlaceholder,
                        header1,
                        header2,
                        username,
                        password,
                        _signInButton,
                        guestLoginLabel
                    }
                }
            };

            BackgroundColor = Color.FromArgb("#F3F4F6");
        }

        // start logic
        // Simulated "fetch user" from cache
        _isUserLoggedIn = CheckIfUserIsLoggedIn();

        _ = Dispatcher.DispatchAsync(async () =>
        {
            if (!_isUserLoggedIn)
            {
                _signInButton.IsEnabled = true;
            }
            else
            {
                await Shell.Current.GoToAsync("claimsview");
            }
        });

    }

    // ---------------------------------------------------------------------
    // PAGE LOGIC AND EVENT HANDLERS
    // ---------------------------------------------------------------------


    protected override bool OnBackButtonPressed() => true;

    private async void SignInButton_Clicked()
    {

        
        // Placeholder login flow
        bool loginSuccess = await AppRepository.Instance.LoginAsync(username.Text, password.Text);

        if (loginSuccess)
        {
            Preferences.Set("IsUserLoggedIn", true);
            Application.Current.MainPage = new AppShell();
        }
        else
        {
            await DisplayAlert("Login Failed", "Invalid credentials " + username.Text + " and " + password.Text, "OK");
            //await DisplayAlert("Login Failed", "Invalid credentials", "OK");
        }
    }

    private bool CheckIfUserIsLoggedIn()
    {
        // Placeholder logic: check secure storage, file, or variable
        return Preferences.Get("IsUserLoggedIn", false);
    }
}