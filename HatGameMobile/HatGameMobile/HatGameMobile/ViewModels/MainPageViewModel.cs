using DynamicData;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Extensions;
using Prism.Commands;
using Prism.Navigation;
using System.Reactive.Linq;
using System;
using System.Diagnostics;
using System.Windows.Input;
using HatGameMobile.Models;

namespace HatGameMobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private readonly ICollectionReference hatCollectionRef;
        private readonly ICollectionReference sessionRef;
        private bool playCanExecute;

        public bool PlayCanExecute
        {
            get => playCanExecute;
            set => SetProperty(ref playCanExecute, value);
        }

        public ICommand NavigateToPlay { get; }
        public ICommand NavigateToCustom { get; }
        public ICommand NavigateToPreset { get; }

        public MainPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = App.RoomId;
            PlayCanExecute = false;

            NavigateToPlay = new DelegateCommand(OnNavigateToPlayExecuted).ObservesCanExecute(() => PlayCanExecute);
            NavigateToCustom = new DelegateCommand(OnNavigateToCustomExecuted);
            NavigateToPreset = new DelegateCommand(OnNavigateToPresetExecuted);

            hatCollectionRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                                   .GetDocument(App.RoomId)
                                                                   .GetCollection("Hat");
            sessionRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                             .GetDocument(App.RoomId)
                                                             .GetCollection("Session");

            sessionRef.ObserveModified()
                      .Subscribe(change => 
                      {
                          var status = change.Document.ToObject<Session>();
                          if (status.IsActive) 
                          {
                              var dispatcher = Prism.PrismApplicationBase.Current.Dispatcher;
                              dispatcher.BeginInvokeOnMainThread(() => OnNavigateToPlayExecuted());
                          }
                      });

            hatCollectionRef.AddSnapshotListener((snapshot, error) =>
            {
                if (snapshot.Count < 10)
                    PlayCanExecute = false;
                else
                    PlayCanExecute = true;
            });
        }
        private async void OnNavigateToPresetExecuted()
        {
            await NavigationService.NavigateAsync("PresetWordsPage");
        }
        private async void OnNavigateToCustomExecuted()
        {
            await NavigationService.NavigateAsync("CustomWordsPage");
        }
        private async void OnNavigateToPlayExecuted()
        {
            await NavigationService.NavigateAsync("/NavigationPage/PlayGamePage");
        }
    }
}
