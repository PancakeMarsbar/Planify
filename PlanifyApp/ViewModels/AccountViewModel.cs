using System.Collections.ObjectModel;
using System.Windows.Input;
using Planify.Models;
using Planify.Pages.Popup;
using Planify.Services;
using CommunityToolkit.Maui.Extensions;


namespace Planify.ViewModels.V2
{
    public class AccountViewModel 
    {
        public ObservableCollection<UserAccount> Users { get; } = new();

        public ICommand DeleteUserCommand { get; }
        public ICommand ReloadCommand { get; }
        public ICommand CreateUserCommand { get; }
        public ICommand UpdateUserCommand { get; }

        public AccountViewModel()
        {
            ReloadCommand = new Command(LoadUsers);
            CreateUserCommand = new Command<UserAccount>(CreateUser);
            DeleteUserCommand = new Command<UserAccount>(DeleteUser);
            UpdateUserCommand = new Command<UserAccount>(UpdateUser);

            LoadUsers(); // initial load
        }

        private void LoadUsers()
        {
            Users.Clear();
            foreach (var user in AppRepository.Instance.Users)
                Users.Add(user);
        }

        private async void CreateUser(UserAccount newUser)
        {
            AppRepository.Instance.CreateUser(newUser);
            await AppRepository.Instance.SaveAsync();
            LoadUsers();
        }

        private async void UpdateUser(UserAccount User)
        {
            if (User.Username == AppRepository.Instance.CurrentUser)
            {
                await Application.Current.MainPage.DisplayAlert("Command Denied", "unable to edit yourself", "OK");
                return;
            }
            var result = await Application.Current.MainPage.ShowPopupAsync<UserAccount>(new UpdateUserPopup(User));
            if (result != null)
            {
                AppRepository.Instance.UpdateUser(User, result.Result);
                await AppRepository.Instance.SaveAsync();
                LoadUsers();
            }
        }

        private async void DeleteUser(UserAccount User)
        {
           
            if (User.Username == AppRepository.Instance.CurrentUser)
            {
                await Application.Current.MainPage.DisplayAlert("Command Denied", "unable to remove yourself", "OK");
                return;
            }

            AppRepository.Instance.RemoveUser(User);
            await AppRepository.Instance.SaveAsync();
            LoadUsers();
        }
    }
}
