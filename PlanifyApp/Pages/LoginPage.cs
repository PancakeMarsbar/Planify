using Microsoft.Identity.Client;
using Microsoft.Maui.Controls;

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Planify.Pages;

public class LoginPage : ContentPage
{
    //------------------------------------------------------------------------------------------------------
    //UI DECLARATION AND CONSTRUCTION
    //------------------------------------------------------------------------------------------------------

    //private readonly ListView _claimsList;
    private readonly Button _signInButton;

    private bool _isUserLoggedIn;

    //public IEnumerable<string> IdTokensClaims { get; set; } = new[] { "No Claims found in ID Tokens" };

    public LoginPage()
    {

        Title = "Sign_In";


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



        _signInButton = new Button
        {
            Text = "Sign In",
            HorizontalOptions = LayoutOptions.Center
        };
        _signInButton.Clicked += SignInButton_Clicked;

        // --- Page layout ---
        Content = new VerticalStackLayout
        {
            Children = {
                header1,
                header2,
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

    private async void SignInButton_Clicked(object sender, EventArgs e)
    {
        // Placeholder login flow
        bool loginSuccess = await PerformCustomLoginAsync();

        if (loginSuccess)
        {
            Preferences.Set("IsUserLoggedIn", true);
            await Shell.Current.GoToAsync("//ClaimsView");
        }
        else
        {
            await DisplayAlert("Login Failed", "Invalid credentials", "OK");
        }
    }

    private bool CheckIfUserIsLoggedIn()
    {
        // Placeholder logic: check secure storage, file, or variable
        return Preferences.Get("IsUserLoggedIn", false);
    }
    private Task<bool> PerformCustomLoginAsync()
    {
        // Placeholder async login (e.g., call your API)
        return Task.FromResult(true); // Simulate success
    }

}