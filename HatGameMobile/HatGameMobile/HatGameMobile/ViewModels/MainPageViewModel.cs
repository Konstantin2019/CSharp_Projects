using Prism.Commands;
using Prism.Navigation;
using System.Diagnostics;
using System.Windows.Input;

namespace HatGameMobile.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        public ICommand NavigateToPlay { get; }
        public ICommand NavigateToCustom { get; }
        public ICommand NavigateToPreset { get; }
        public ICommand NavigateToExit { get; }

        public MainPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "--TO DO--";

            NavigateToPlay = new DelegateCommand(OnNavigateToPlayExecuted);
            NavigateToCustom = new DelegateCommand(OnNavigateToCustomExecuted);
            NavigateToPreset = new DelegateCommand(OnNavigateToPresetExecuted);
            NavigateToExit = new DelegateCommand(OnNavigateToExitExecuted);
        }

        private void OnNavigateToExitExecuted()
        {
            Process.GetCurrentProcess().CloseMainWindow();
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
            await NavigationService.NavigateAsync("PlayGamePage");
        }
    }
}
