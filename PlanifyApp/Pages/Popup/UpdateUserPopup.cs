using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Planify.Models;


namespace Planify.Pages.Popup;

public class UpdateUserPopup : Popup<UserAccount>
{

    private readonly Entry nameEntry;
    private readonly Entry passwordEntry;
    private readonly CheckBox isAdmin;
    public UpdateUserPopup(UserAccount oldUser)
    {
        WidthRequest = 230;   // desired width in device-independent units
        HeightRequest = 250;  // desired height
        var name = new Label { Text = "Username" };
        var password = new Label { Text = "Password" };
        var admin = new Label { Text = "admin" };

        nameEntry = new Entry() { Placeholder = oldUser.Username, Text = oldUser.Username };
        passwordEntry = new Entry() { Placeholder = "NewPassword", Text = oldUser.Password };
        isAdmin = new CheckBox() { IsChecked = oldUser.IsAdmin };

        var ConfirmButton = new Button() { Text = "Confirm" };
        ConfirmButton.Clicked += (s, e) =>
        {
            var newUser = new UserAccount
            {
                Username = nameEntry.Text,
                Password = passwordEntry.Text,
                IsAdmin = isAdmin.IsChecked
            };
            CloseAsync(newUser);
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