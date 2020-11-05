using HatGameMobile.Models;
using Plugin.CloudFirestore.Extensions;
using Prism.Commands;
using Prism.Navigation;
using System.Reactive.Linq;
using System;
using System.Threading.Tasks;
using Prism.Services;
using System.Collections.Generic;

namespace HatGameMobile.ViewModels
{
    public class CustomWordsPageViewModel : ViewModelBase
    {
        private string selectedNumber;
        private int intSelectedNumber;
        private string newWord;
        private int counter;
        private List<Word> addedWords;

        public int IntSelectedNumber
        {
            get => intSelectedNumber;
            set => SetProperty(ref intSelectedNumber, value);
        }
        public string SelectedNumber
        {
            get => selectedNumber;
            set
            {
                SetProperty(ref selectedNumber, value);
                IntSelectedNumber = int.Parse(value.Split(' ')[0]);
            }
        }
        public string NewWord
        {
            get => newWord;
            set => SetProperty(ref newWord, value);
        }
        public int Counter
        {
            get => counter;
            set => SetProperty(ref counter, value);
        }
        public DelegateCommand AddCustomWordCommand { get; }
        public CustomWordsPageViewModel(INavigationService navigationService, IPageDialogService dialogService)
            : base(navigationService, dialogService)
        {
            Title = "Add custom words...";
            SelectedNumber = "5 words";
            addedWords = new List<Word>();

            AddCustomWordCommand = new DelegateCommand(async () => { await OnAddCustomWordExecuted(); }, CanAddCustomWordExecute)
                                                      .ObservesProperty(() => IntSelectedNumber)
                                                      .ObservesProperty(() => Counter);

            HatRef.ObserveAdded()
                  .Subscribe(documentChanged => 
                  {
                      if (documentChanged.Document.Id == NewWord)
                          Counter++;
                  });
        }
        private bool CanAddCustomWordExecute()
        {
            if (Counter < IntSelectedNumber)
            {
                Title = "Add custom words...";
                return true;
            }
            else 
            {
                Title = "Successfully done";
                NewWord = null;
                return false;
            }
        }
        private async Task OnAddCustomWordExecuted()
        {
            var customWord = new Word { Content = NewWord };
            if (customWord.Content != null) 
            {
                await HatRef.GetDocument(customWord.Content)
                            .SetDataAsync(customWord);
                addedWords.Add(customWord);
            }
            NewWord = null;
        }
        protected override async Task RemoveFromFireStore(bool isHost)
        {
            if (!App.IsHost) 
            {
                foreach (var word in addedWords)
                {
                    await HatRef.GetDocument(word.Content)
                                .DeleteDocumentAsync();
                }
            }

            await base.RemoveFromFireStore(isHost);
        }
    }
}
