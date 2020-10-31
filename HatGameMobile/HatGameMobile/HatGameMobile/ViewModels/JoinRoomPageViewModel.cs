using HatGameMobile.Models;
using Plugin.CloudFirestore;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
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
        private IPageDialogService dialog;
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

        public JoinRoomPageViewModel(INavigationService navigationService, IPageDialogService dialogService)
            : base(navigationService)
        {
            dialog = dialogService;
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
            var roomRequest = await roomCollectionRef.GetDocumentsAsync();
            var ids = roomRequest.Documents.Select(d => d.Id).ToList();
            var rooms = roomRequest.ToObjects<GameRoom>().ToList();
            var collection = ids.Zip(rooms, (key, value) => new { id = key, room = value }).ToList();
            var id = collection.Where(s => s.room.Name == inputData.Name && s.room.Password == inputData.Password)
                               .Select(s => s.id)
                               .FirstOrDefault();
            if (id != null)
            {
                var sessionRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                                     .GetDocument(id)
                                                                     .GetCollection("Session");
                var sessionRequest = await sessionRef.GetDocumentsAsync();
                var session = sessionRequest.ToObjects<Session>().FirstOrDefault();
                if (!session.IsActive) 
                {
                    App.RoomId = id;
                    var teamsRef = roomCollectionRef.GetDocument(id).GetCollection("Teams");
                    var teamNamesRef = await teamsRef.GetDocumentsAsync();
                    var teamNames = teamNamesRef.ToObjects<Team>().Select(t => t.Name).ToList();
                    if (!teamNames.Contains(TeamName))
                    {
                        App.TeamName = TeamName;
                        await teamsRef.GetDocument(TeamName).SetDataAsync(new Team { Name = TeamName, IsHost = false });
                        await NavigationService.NavigateAsync("/NavigationPage/MainPage");
                    }
                    else
                        await dialog.DisplayAlertAsync("Информация", "Такое имя команды уже существует!", "OK");
                }
                else
                    await dialog.DisplayAlertAsync("Информация", "Игровая сессия уже запущена!", "OK");
            }
            else
                await dialog.DisplayAlertAsync("Информация", "Комната не найдена!", "OK");
        }
    }
}
