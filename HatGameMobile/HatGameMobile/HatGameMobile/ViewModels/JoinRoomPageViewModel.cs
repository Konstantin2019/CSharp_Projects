using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace HatGameMobile.ViewModels
{
    public class JoinRoomPageViewModel : ViewModelBase
    {
        private string roomName;
        private string roomPassword;
        private string teamName;
        public string RoomName
        {
            get => roomName;
            set => SetProperty(ref roomName, value);
        }
        public string RoomPassword
        {
            get => roomPassword;
            set => SetProperty(ref roomPassword, value);
        }
        public string TeamName
        {
            get => teamName;
            set => SetProperty(ref teamName, value);
        }
        public ICommand JoinGameRoom { get; }

        public JoinRoomPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Присоединение...";
            RoomName = "";
            RoomPassword = "";
            TeamName = "";
            JoinGameRoom = new DelegateCommand(OnJoinGameRoomExecuted);
        }
        private async void OnJoinGameRoomExecuted()
        {
            await NavigationService.NavigateAsync("MainPage");
        }
    }
}
