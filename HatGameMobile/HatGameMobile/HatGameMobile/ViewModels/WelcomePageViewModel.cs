using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using System.Diagnostics;
using System.Windows.Input;

namespace HatGameMobile.ViewModels
{
    public class WelcomePageViewModel : ViewModelBase
    {
        public ICommand NavigateToCreateRoom { get; }
        public ICommand NavigateToJoinRoom { get; }
        public ICommand NavigateToQuitGame { get; }

        public WelcomePageViewModel(INavigationService navigationService, IPageDialogService dialogService)
            : base(navigationService, dialogService)
        {
            Title = "#HAT GAME#";

            NavigateToCreateRoom = new DelegateCommand(OnNavigateToCreateRoomExecuted);
            NavigateToJoinRoom = new DelegateCommand(OnNavigateToJoinRoomExecuted);
            NavigateToQuitGame = new DelegateCommand(OnNavigateToQuitGameExecuted);
        }
        private void OnNavigateToQuitGameExecuted()
        {
            Process.GetCurrentProcess().CloseMainWindow();
        }
        private async void OnNavigateToJoinRoomExecuted()
        {
            await NavigationService.NavigateAsync("JoinRoomPage");
        }
        private async void OnNavigateToCreateRoomExecuted()
        {
            await NavigationService.NavigateAsync("CreateRoomPage");
        }
    }
}
