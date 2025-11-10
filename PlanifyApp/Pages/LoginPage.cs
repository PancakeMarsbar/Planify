using Microsoft.Identity.Client;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

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

        var header1 = new Label
        {
            Text = "local auth sample",
            FontSize = 26,
            HorizontalOptions = LayoutOptions.Center
        };

        var header2 = new Label
        {
            Text = "MAUI sample",
            FontSize = 26,
            Padding = new Thickness(0, 0, 0, 20),
            HorizontalOptions = LayoutOptions.Center
        };

        username = new Entry
        {
            ReturnCommand = signInCommand,
            Placeholder = "Usernames",
            PlaceholderColor = Color.FromRgba(128, 128, 128, 128),
            HorizontalOptions = LayoutOptions.Center
        };

        password = new Entry
        {
            ReturnCommand = signInCommand,
            Placeholder = "Password",
            PlaceholderColor = Color.FromRgba(128, 128, 128, 128),
            HorizontalOptions = LayoutOptions.Center,
            IsPassword = true
        };

        _signInButton = new Button
        {
            Text = "Sign In",
            HorizontalOptions = LayoutOptions.Center,
            Command = signInCommand
        };
        

        // --- Page layout ---
        Content = new VerticalStackLayout
        {
            Children = {
                header1,
                header2,
                username,
                password,
                _signInButton
            }
        };

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