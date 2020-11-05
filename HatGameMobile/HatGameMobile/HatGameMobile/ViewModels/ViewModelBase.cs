using Plugin.CloudFirestore;
using Prism.AppModel;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HatGameMobile.ViewModels
{
    public class ViewModelBase : BindableBase, IInitialize, INavigationAware, IDestructible, IApplicationLifecycleAware
    {
        protected Task waitTask;
        protected INavigationService NavigationService { get; private set; }
        protected IPageDialogService DialogService { get; private set; }
        protected ICollectionReference RoomRef { get; set; }
        protected ICollectionReference HatRef { get; set; }
        protected ICollectionReference SessionRef { get; set; }
        protected ICollectionReference TeamsRef { get; set; }
        protected CancellationTokenSource Cts { get; set; }

        private string title;
        public string Title
        {
            get => title;
            set => SetProperty(ref title, value);
        }
        public ViewModelBase(INavigationService navigationService, IPageDialogService dialogService)
        {
            NavigationService = navigationService;
            DialogService = dialogService;

            RoomRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom");
            if (App.RoomId != null) 
            {
                HatRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                             .GetDocument(App.RoomId)
                                                             .GetCollection("Hat");
                SessionRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                                 .GetDocument(App.RoomId)
                                                                 .GetCollection("Session");
                TeamsRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                               .GetDocument(App.RoomId)
                                                               .GetCollection("Teams");
            }
        }
        public virtual void Initialize(INavigationParameters parameters) { }
        public virtual void OnNavigatedFrom(INavigationParameters parameters) { }
        public virtual void OnNavigatedTo(INavigationParameters parameters) { }
        public virtual void Destroy() { }
        protected async virtual Task RemoveFromFireStore(bool isHost) 
        {
            if (isHost)
            {
                if (SessionRef != null)
                    await SessionRef.GetDocument("CurrentSession").DeleteDocumentAsync();
                if (TeamsRef != null) 
                {
                    var teamsRequest = await TeamsRef.GetDocumentsAsync();
                    var teams = teamsRequest.Documents.Select(d => d.Id).ToList();
                    foreach (var t in teams)
                        await TeamsRef.GetDocument(t).DeleteDocumentAsync();
                }
                if (HatRef != null) 
                {
                    var hatRequest = await HatRef.GetDocumentsAsync();
                    var words = hatRequest.Documents.Select(d => d.Id).ToList();
                    foreach (var w in words)
                        await HatRef.GetDocument(w).DeleteDocumentAsync();
                }
                if (App.RoomId != null)
                    await RoomRef.GetDocument(App.RoomId).DeleteDocumentAsync();
            }
            else
            {
                await TeamsRef.GetDocument(App.TeamName).DeleteDocumentAsync();
            }
        }
        public void OnResume()
        {
            if (Cts != null)
                Cts.Cancel();
        }
        public void OnSleep()
        {
            Cts = new CancellationTokenSource();
            var token = Cts.Token;
            Task.Run(async () =>
            {
                await Task.Delay(600000, token);
                await RemoveFromFireStore(App.IsHost);
                var dispatcher = Prism.PrismApplicationBase.Current.Dispatcher;
                dispatcher.BeginInvokeOnMainThread(async () =>
                {
                    await NavigationService.NavigateAsync("/NavigationPage/WelcomePage");
                });
            });
        }
    }
}
