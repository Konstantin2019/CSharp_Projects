using Prism.Commands;
using Prism.Navigation;
using System.Windows.Input;
using Prism.AppModel;
using Prism.Services;

namespace HatGameMobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase, IApplicationLifecycleAware
    {
        private bool playCanExecute;

        public bool PlayCanExecute
        {
            get => playCanExecute;
            set => SetProperty(ref playCanExecute, value);
        }

        public ICommand NavigateToPlay { get; }
        public ICommand NavigateToCustom { get; }
        public ICommand NavigateToPreset { get; }

        public MainPageViewModel(INavigationService navigationService, IPageDialogService dialogService)
            : base(navigationService, dialogService)
        {
            Title = App.RoomId;
            PlayCanExecute = false;

            NavigateToPlay = new DelegateCommand(OnNavigateToPlayExecuted).ObservesCanExecute(() => PlayCanExecute);
            NavigateToCustom = new DelegateCommand(OnNavigateToCustomExecuted);
            NavigateToPreset = new DelegateCommand(OnNavigateToPresetExecuted);

            HatRef.AddSnapshotListener((snapshot, error) =>
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
