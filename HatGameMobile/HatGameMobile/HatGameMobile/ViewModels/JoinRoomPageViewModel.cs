using HatGameMobile.Models;
using Plugin.CloudFirestore;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HatGameMobile.ViewModels
{
    public class JoinRoomPageViewModel : ViewModelBase
    {
        private string roomName;
        private string roomPassword;
        private string teamName;
        private readonly ICollectionReference roomCollectionRef;
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
            roomCollectionRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom");
            JoinGameRoom = new DelegateCommand(async () => { await OnJoinGameRoomExecuted(); }, CanJoinGameRoomExecute)
                                                .ObservesProperty(() => RoomName)
                                                .ObservesProperty(() => RoomPassword)
                                                .ObservesProperty(() => TeamName);
        }
        private bool CanJoinGameRoomExecute()
        {
            if ((RoomName != null && RoomName.Length > 2)
                && (RoomPassword != null && RoomPassword.Length > 5)
                && (TeamName != null && TeamName.Length > 2))
                return true;
            else
                return false;
        }
        private async Task OnJoinGameRoomExecuted()
        {
            var inputData = new GameRoom
            {
                Name = RoomName,
                Password = RoomPassword
            };
            var request = await roomCollectionRef.GetDocumentsAsync();
            var ids = request.Documents.Select(d => d.Id).ToList();
            var rooms = request.ToObjects<GameRoom>().ToList();
            var collection = ids.Zip(rooms, (key, value) => new { id = key, room = value }).ToList();
            var id = collection.Where(s => s.room.Name == inputData.Name && s.room.Password == inputData.Password)
                               .Select(s => s.id)
                               .FirstOrDefault();
            if (id != null)
            {
                App.RoomId = id;
                var teamsRef = roomCollectionRef.GetDocument(id).GetCollection("Teams");
                await teamsRef.GetDocument(TeamName).SetDataAsync(new Team { Name = TeamName, Password = "" });
                await NavigationService.NavigateAsync("MainPage");
            }
        }
    }
}
