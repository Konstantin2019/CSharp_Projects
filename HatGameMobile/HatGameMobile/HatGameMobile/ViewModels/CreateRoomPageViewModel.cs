﻿using HatGameMobile.Models;
using Plugin.CloudFirestore;
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

            CreateGameRoom = new DelegateCommand(async () => { await OnCreateGameRoomExecuted(); }, CanCreateGameRoomExecute)
                                                .ObservesProperty(() => RoomName)
                                                .ObservesProperty(() => RoomPassword)
                                                .ObservesProperty(() => TeamName);
            this.ObservableForProperty(p => p.AutoPasswordGen)
                .Select(p => p.Value)
                .Subscribe(s => { if (s) RoomPassword = GeneratePassword(8); });
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
            var room = new GameRoom
            {
                Name = RoomName,
                Password = RoomPassword
            };
            var id = await GenerateId();
            App.RoomId = id;
            App.IsHost = true;
            App.TeamName = TeamName;
            await RoomRef.GetDocument(id)
                         .SetDataAsync(room);
            TeamsRef = RoomRef.GetDocument(id)
                              .GetCollection("Teams");
            await TeamsRef.GetDocument(TeamName)
                          .SetDataAsync(new Team { Name = TeamName, IsHost = true });
            SessionRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                             .GetDocument(id)
                                                             .GetCollection("Session");
            await SessionRef.GetDocument("CurrentSession")
                            .SetDataAsync(new Session { IsActive = false, TimerStarted = false, NumberOfReadyTeams = 0 });
            await NavigationService.NavigateAsync("/NavigationPage/MainPage");
        }
        private string GeneratePassword(int length)
        {
            var chars = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            return new string(chars.OrderBy(o => Guid.NewGuid()).Take(length).ToArray());
        }
        private async Task<string> GenerateId() 
        {
            string code = null;
            var request = await RoomRef.GetDocumentsAsync();
            var ids = request.Documents.Select(d => d.Id).ToList();
            var notUnique = true;
            while (notUnique)
            {
                code = GeneratePassword(5);
                notUnique = ids.Contains(code);
            }
            return RoomName + "_" + code;
        } 
    }
}
