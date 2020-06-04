using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HatGameMobile.ViewModels
{
    public class PresetWordsPageViewModel : ViewModelBase
    {
        public PresetWordsPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Добавь готовые слова";
        }
    }
}
