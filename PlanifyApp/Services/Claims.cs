using Microsoft.Identity.Client;
using Microsoft.Maui.Controls;

using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Planify.Services;
using Planify.Pages;

namespace Planify.PlanifyApp.Services;

public class Claims : ContentPage
{
	//------------------------------------------------------------------------------------------------------
	//UI DECLARATION AND CONSTRUCTION
	//------------------------------------------------------------------------------------------------------

	private readonly ListView _claimsList;
	private readonly Button _signOutButton;

	public IEnumerable<string> IdTokensClaims { get; set; } = new[] { "No Claims found in ID Tokens" };

	public Claims()
	{

		Title = "ClaimsView";

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

		var subtitle = new Label
		{
			Text = "Claims found in ID token",
			FontSize = 18,
			Padding = new Thickness(0, 20, 0, 0),
			VerticalOptions = LayoutOptions.Center,
			HorizontalOptions = LayoutOptions.Center
		};

		// --- layout elemets ---
		_claimsList = new ListView
		{
			//binds to the view model
			ItemsSource = null, // replace whit BindingContex later
			ItemTemplate = new DataTemplate(() =>
			{
				var label = new Label
				{
					HorizontalOptions = LayoutOptions.Center
				};
				label.SetBinding(Label.TextProperty, ".");
				return new ViewCell
				{
					View = new Grid
					{
						Padding = new Thickness(0),
						Children = { label }
					}
				};
			})
		};


		_signOutButton = new Button
		{
			Text = "Sign Out",
			HorizontalOptions = LayoutOptions.Center
		};
		_signOutButton.Clicked += SignOutButton_Clicked;

		// --- Page layout ---
		Content = new VerticalStackLayout
		{
			Children = {
				header1,
				header2,
				subtitle,
				_claimsList,
				_signOutButton

			}
		};

        // start logic
        _ = LoadLocalClaimsAsync();

    }

    // ---------------------------------------------------------------------
    // PAGE LOGIC AND EVENT HANDLERS
    // ---------------------------------------------------------------------

	
    private async Task LoadLocalClaimsAsync()
	{
		// Replace this with you own local logic
		// Example: laod claims from local JSON, DB, or Static list
		await Task.Delay(300); // simulate small async work

		IdTokensClaims = new[]
		{
			"user; " + AppRepository.Instance.CurrentUser,
			"role; " + (AppRepository.Instance.IsAdmin ? "admin" : "User")
		};

		_claimsList.ItemsSource = IdTokensClaims;
	}

	protected override bool OnBackButtonPressed() => true;

	private async void SignOutButton_Clicked(object sender, EventArgs e)
    {
		// Replace whit your own local sign-out or state clear logic
		AppRepository.Instance.Logout();
        Application.Current.MainPage = new LoginPage();
        //await Shell.Current.GoToAsync("//LoginPage");
    }
}