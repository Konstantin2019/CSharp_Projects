using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;

namespace HatGameMobile.ViewModels
{
    public class WelcomePageViewModel : ViewModelBase
    {
        public ICommand NavigateToCreateRoom { get; }
        public ICommand NavigateToJoinRoom { get; }
        public ICommand NavigateToQuitGame { get; }

        public WelcomePageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Игра **Шляпа**";

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
