using HatGameMobile.Models;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Extensions;
using Prism.Commands;
using Prism.Navigation;
using System.Reactive.Linq;
using System;
using System.Threading.Tasks;

namespace HatGameMobile.ViewModels
{
    public class CustomWordsPageViewModel : ViewModelBase
    {
        private string selectedNumber;
        private int intSelectedNumber;
        private string newWord;
        private int counter;
        private readonly ICollectionReference hatCollectionRef;

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
        public CustomWordsPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Добавь свои слова";
            SelectedNumber = "5 слов";

            hatCollectionRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                                   .GetDocument(App.RoomId)
                                                                   .GetCollection("Hat");

            AddCustomWordCommand = new DelegateCommand(async () => { await OnAddCustomWordExecuted(); }, CanAddCustomWordExecute)
                                                      .ObservesProperty(() => IntSelectedNumber)
                                                      .ObservesProperty(() => Counter);

            hatCollectionRef.ObserveAdded()
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
                Title = "Добавь свои слова";
                return true;
            }
            else 
            {
                Title = "Успешно выполнено";
                NewWord = null;
                return false;
            }
        }
        private async Task OnAddCustomWordExecuted()
        {
            var word = new Word { Content = NewWord };
            if (word.Content != null)
                await hatCollectionRef.GetDocument(word.Content).SetDataAsync(word);
            NewWord = null;
        }
    }
}
