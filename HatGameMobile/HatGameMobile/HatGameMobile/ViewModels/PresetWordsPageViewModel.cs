using HatGameMobile.Models;
using ImTools;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Extensions;
using Prism.Commands;
using Prism.Navigation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HatGameMobile.ViewModels
{
    public class PresetWordsPageViewModel : ViewModelBase
    {
        private string selectedNumber;
        private int intSelectedNumber;
        private string selectedComplexity;
        private int counter;
        private bool canExecute;
        private string currentWord;
        private ICollectionReference dbCollectionRef;
        private readonly ICollectionReference hatCollectionRef;
        private readonly Dictionary<string, string> comlexityDict;
        private readonly ReactiveCommand<Unit, Unit> addReactive;
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
        public string SelectedComplexity
        {
            get => selectedComplexity;
            set => SetProperty(ref selectedComplexity, value);
        }
        public int Counter
        {
            get => counter;
            set => SetProperty(ref counter, value);
        }
        public bool CanExecute
        {
            get => canExecute;
            set => SetProperty(ref canExecute, value);
        }
        public DelegateCommand AddPresetWordCommand { get; }
        public PresetWordsPageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Title = "Добавь готовые слова";
            SelectedNumber = "5 слов";
            SelectedComplexity = "Легко";
            CanExecute = true;
            comlexityDict = new Dictionary<string, string>
            {
                { "Легко", "EasyHat" },
                { "Средне", "MediumHat" },
                { "Сложно", "HardHat" }
            };
            hatCollectionRef = CrossCloudFirestore.Current.Instance.GetCollection("Hat");
            dbCollectionRef = CrossCloudFirestore.Current.Instance.GetCollection(comlexityDict[SelectedComplexity]);
            AddPresetWordCommand = new DelegateCommand(OnAddPresetWordExecuted).ObservesCanExecute(() => CanExecute);
            addReactive = ReactiveCommand.CreateFromTask(_ => AddPresetWordTask());
            hatCollectionRef.ObserveAdded()
                            .Subscribe(documentChanged =>
                            {
                                if (documentChanged.Document.Id == currentWord)
                                    Counter++;
                            });
            this.ObservableForProperty(p => p.IntSelectedNumber)
                .Select(p => p.Value)
                .Subscribe(s => 
                {
                    if (s > Counter) 
                    {
                        Title = "Добавь готовые слова";
                        CanExecute = true;
                    }
                });
        }
        private void OnAddPresetWordExecuted()
        {
            addReactive.Execute().Subscribe(s => { if (Counter == IntSelectedNumber) Title = "Успешно выполнено"; });
            addReactive.IsExecuting.Subscribe(IsExecuting => { if (IsExecuting) Title = "В процессе..."; });
            addReactive.CanExecute.Subscribe(CanExecute => { this.CanExecute = false; });
        }
        private async Task AddPresetWordTask()
        {
            dbCollectionRef = CrossCloudFirestore.Current.Instance.GetCollection(comlexityDict[SelectedComplexity]);
            var dbCollection = await dbCollectionRef.GetDocumentsAsync();
            var words = dbCollection.ToObjects<Word>().ToList();
            if (words != null)
            {
                var rnd = new Random();
                while (Counter < IntSelectedNumber)
                {
                    var randIndex = rnd.Next(words.Count);
                    var word = words[randIndex];
                    currentWord = word.Content;
                    await hatCollectionRef.GetDocument(currentWord).SetDataAsync(word);
                }
            }
        }
    }
}
