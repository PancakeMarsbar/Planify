using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Planify.Services;


namespace Planify.Pages;

public class CreateUserPopup : Popup
{
	public CreateUserPopup()
	{
		WidthRequest = 230;   // desired width in device-independent units
		HeightRequest = 250;  // desired height
		var name = new Label { Text = "Username" };
		var password = new Label { Text = "Password" };
		var admin = new Label { Text = "admin" };

		var nameEntry = new Entry() { Placeholder = "Enter Name" };
		var passwordEntry = new Entry() { Placeholder = "Password" };
		var isAdmin = new CheckBox();

		var ConfirmButton = new Button() { Text = "Confirm" };
		ConfirmButton.Clicked += (s, e) =>
		{
			AppRepository.Instance.CreateUser(
				new Models.UserAccount
				{
					Username = nameEntry.Text,
					Password = passwordEntry.Text,
					IsAdmin = isAdmin.IsChecked
				}
				);
			CloseAsync();
		};

		var CancelButton = new Button() { Text = "Cancel" };
		CancelButton.Clicked += (s, e) => CloseAsync();


		Content = new VerticalStackLayout
		{
			Padding = 20,
			Spacing = 10,
			Children = {
			new Label { Text = "Enter information"},
			HStack (name, nameEntry ),
			HStack (password, passwordEntry),
			HStack (admin, isAdmin),
			HStack (ConfirmButton, CancelButton)
			}
		};
	}
    private HorizontalStackLayout HStack(View first, View second, double spacing = 5)
    {
        return new HorizontalStackLayout
        {
            Spacing = spacing,
            Children = { first, second }
        };
    }
}