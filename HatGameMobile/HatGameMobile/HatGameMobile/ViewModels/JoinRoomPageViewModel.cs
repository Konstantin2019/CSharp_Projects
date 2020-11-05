using HatGameMobile.Models;
using Plugin.CloudFirestore;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
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
            : base(navigationService, dialogService)
        {
            Title = "Joining...";
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
            var roomRequest = await RoomRef.GetDocumentsAsync();
            var ids = roomRequest.Documents.Select(d => d.Id).ToList();
            var rooms = roomRequest.ToObjects<GameRoom>().ToList();
            var collection = ids.Zip(rooms, (key, value) => new { id = key, room = value }).ToList();
            var id = collection.Where(s => s.room.Name == inputData.Name && s.room.Password == inputData.Password)
                               .Select(s => s.id)
                               .FirstOrDefault();
            if (id != null)
            {
                SessionRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                                 .GetDocument(id)
                                                                 .GetCollection("Session");
                var sessionRequest = await SessionRef.GetDocumentsAsync();
                var session = sessionRequest.ToObjects<Session>().FirstOrDefault();
                if (!session.IsActive) 
                {
                    App.RoomId = id;
                    App.IsHost = false;
                    TeamsRef = RoomRef.GetDocument(id)
                                      .GetCollection("Teams");
                    var teamNamesRef = await TeamsRef.GetDocumentsAsync();
                    var teamNames = teamNamesRef.ToObjects<Team>().Select(t => t.Name).ToList();
                    if (!teamNames.Contains(TeamName))
                    {
                        App.TeamName = TeamName;
                        await TeamsRef.GetDocument(TeamName)
                                      .SetDataAsync(new Team { Name = TeamName, IsHost = false });

                        await NavigationService.NavigateAsync("/NavigationPage/MainPage");
                    }
                    else
                        await DialogService.DisplayAlertAsync("Information", "Team not found!", "OK");
                }
                else
                    await DialogService.DisplayAlertAsync("Information", "Game session started!", "OK");
            }
            else
                await DialogService.DisplayAlertAsync("Information", "Game room not found!", "OK");
        }
    }
}
