using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Xamarin.Forms;

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
            Title = "Игра **Шляпа**";

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
