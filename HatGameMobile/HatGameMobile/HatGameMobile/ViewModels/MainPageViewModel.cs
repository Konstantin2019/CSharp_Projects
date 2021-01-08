using Prism.Commands;
using Prism.Navigation;
using System.Windows.Input;
using Prism.Services;
using Plugin.Share;
using Plugin.Share.Abstractions;

namespace HatGameMobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private bool playCanExecute;
        private INavigationParameters parameters;

        public bool PlayCanExecute
        {
            get => playCanExecute;
            set => SetProperty(ref playCanExecute, value);
        }
        public ICommand NavigateToPlay { get; }
        public ICommand NavigateToCustom { get; }
        public ICommand NavigateToPreset { get; }
        public ICommand Share { get; }

        public MainPageViewModel(INavigationService navigationService,
                                 IPageDialogService dialogService)
            : base(navigationService, dialogService)
        {
            Title = App.RoomId;
            PlayCanExecute = false;

            NavigateToPlay = new DelegateCommand(OnNavigateToPlayExecuted).ObservesCanExecute(() => PlayCanExecute);
            NavigateToCustom = new DelegateCommand(OnNavigateToCustomExecuted);
            NavigateToPreset = new DelegateCommand(OnNavigateToPresetExecuted);
            Share = new DelegateCommand(OnShareExecuted);

            HatRef.AddSnapshotListener((snapshot, error) =>
            {
                if (snapshot.Count < 10)
                    PlayCanExecute = false;
                else
                    PlayCanExecute = true;
            });
        }
        private async void OnShareExecuted()
        {
            await CrossShare.Current.Share(new ShareMessage
            {
                Title = parameters.GetValue<string>("Title"),
                Text = $"RoomName : { parameters.GetValue<string>("Name") }, " +
                       $"Password : { parameters.GetValue<string>("Password") }"
            });
        }
        public override void OnNavigatedTo(INavigationParameters parameters)
        {
            this.parameters = parameters;
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
