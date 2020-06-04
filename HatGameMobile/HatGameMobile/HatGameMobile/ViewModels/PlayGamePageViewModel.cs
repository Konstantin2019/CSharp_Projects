using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HatGameMobile.ViewModels
{
    public class PlayGamePageViewModel : ViewModelBase
    {
        private byte stage;
        
        public PlayGamePageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            stage = 1;
            Title = $"Идёт {stage} раунд";
        }
    }
}
