using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HatGameMobile.ViewModels
{
    public class CustomWordsPageViewModel : ViewModelBase
    {
        public CustomWordsPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Добавь свои слова";
        }
    }
}
