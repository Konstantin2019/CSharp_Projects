using HatGameMobile.Models;
using HatGameMobile.Models.Services;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Extensions;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HatGameMobile.ViewModels
{
    public class CreateRoomPageViewModel : ViewModelBase
    {
        private string roomName;
        private string roomPassword;
        private bool autoPaswwordGen;
        private string teamName;
        private bool success;
        private GameRoom room;
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
        public bool AutoPasswordGen
        { 
            get => autoPaswwordGen; 
            set => SetProperty(ref autoPaswwordGen, value); 
        }
        public string TeamName
        {
            get => teamName;
            set => SetProperty(ref teamName, value);
        }
        public ICommand CreateGameRoom { get; }
        public CreateRoomPageViewModel(INavigationService navigationService, IPageDialogService dialogService)
            :base(navigationService, dialogService)
        {
            Title = "Creating...";
            success = false;

            CreateGameRoom = new DelegateCommand(async () => { await OnCreateGameRoomExecuted(); }, CanCreateGameRoomExecute)
                                                .ObservesProperty(() => RoomName)
                                                .ObservesProperty(() => RoomPassword)
                                                .ObservesProperty(() => TeamName);
            this.ObservableForProperty(p => p.AutoPasswordGen)
                .Select(p => p.Value)
                .Subscribe(s => { if (s) RoomPassword = Helper.GeneratePassword(8); });

            RoomRef.ObserveAdded()
                   .Subscribe(r =>
                    {
                        if (room != null && r.Document.Id == room.Id)
                            success = true;
                    });
        }

        private bool CanCreateGameRoomExecute()
        {
            if ((RoomName != null && RoomName.Length > 2) 
                && (RoomPassword != null && RoomPassword.Length > 5)
                && (TeamName != null && TeamName.Length > 2))
                return true;
            else
                return false;
        }
        private async Task OnCreateGameRoomExecuted()
        {
            room = new GameRoom
            {
                Name = RoomName,
                Password = RoomPassword, 
            };

            while (!success)
            {
                room.Id = room.Name + "_" + Helper.GeneratePassword(5);
                await RoomRef.GetDocument(room.Id).SetDataAsync(room);
            }

            App.RoomId = room.Id;
            App.IsHost = true;
            App.TeamName = TeamName;

            TeamsRef = RoomRef.GetDocument(room.Id)
                              .GetCollection("Teams");

            await TeamsRef.GetDocument(TeamName)
                          .SetDataAsync(new Team { Name = TeamName, IsHost = true });

            SessionRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                             .GetDocument(room.Id)
                                                             .GetCollection("Session");

            await SessionRef.GetDocument("CurrentSession")
                            .SetDataAsync(new Session { IsActive = false, TimerStarted = false, NumberOfReadyTeams = 0 });

            var parameters = new NavigationParameters($"Title=GameRoom&Name={room.Id}&Password={room.Password}");
            await NavigationService.NavigateAsync("/NavigationPage/MainPage", parameters);
        }
    }
}
