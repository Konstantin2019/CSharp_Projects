using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace HatGameMobile.ViewModels
{
    public class CreateRoomPageViewModel : ViewModelBase
    {
        private int roomID;
        private string roomName;
        private string roomPassword;
        private bool autoPaswwordGen;
        public int RoomID
        { 
            get => roomID; 
            set => SetProperty(ref roomID, value); 
        }
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
        public bool AutoPaswwordGen
        { 
            get => autoPaswwordGen; 
            set => SetProperty(ref autoPaswwordGen, value); 
        }
        public ICommand CreateGameRoom { get; }
        public CreateRoomPageViewModel(INavigationService navigationService)
            :base(navigationService)
        {
            Title = "Создание...";
            RoomID = 0;
            RoomName = "";
            RoomPassword = "";
            AutoPaswwordGen = false;
            CreateGameRoom = new DelegateCommand(OnCreateGameRoomExecuted);
        }
        private async void OnCreateGameRoomExecuted()
        {
            await NavigationService.NavigateAsync("MainPage");
        }
    }
}
