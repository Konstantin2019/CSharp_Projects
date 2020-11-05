using HatGameMobile.Models;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Extensions;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
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
        private List<Word> addedWords;
        private ICollectionReference presetDBRef;
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
        public PresetWordsPageViewModel(INavigationService navigationService, IPageDialogService dialogService)
            : base(navigationService, dialogService)
        {
            Title = "Add preset words";
            SelectedNumber = "5 words";
            SelectedComplexity = "Easy";
            CanExecute = true;

            comlexityDict = new Dictionary<string, string>
            {
                { "Easy", "EasyHat" },
                { "Medium", "MediumHat" },
                { "Hard", "HardHat" }
            };
            addedWords = new List<Word>();

            presetDBRef = CrossCloudFirestore.Current.Instance.GetCollection(comlexityDict[SelectedComplexity]);

            AddPresetWordCommand = new DelegateCommand(OnAddPresetWordExecuted).ObservesCanExecute(() => CanExecute);
            addReactive = ReactiveCommand.CreateFromTask(_ => AddPresetWordTask());

            HatRef.ObserveAdded()
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
                        Title = "Add preset words";
                        CanExecute = true;
                    }
                });
        }
        private void OnAddPresetWordExecuted()
        {
            addReactive.Execute().Subscribe(s => { if (Counter == IntSelectedNumber) Title = "Successfully done"; });
            addReactive.IsExecuting.Subscribe(IsExecuting => { if (IsExecuting) Title = "In process..."; });
            addReactive.CanExecute.Subscribe(CanExecute => { this.CanExecute = false; });
        }
        private async Task AddPresetWordTask()
        {
            presetDBRef = CrossCloudFirestore.Current.Instance.GetCollection(comlexityDict[SelectedComplexity]);
            var presetCollection = await presetDBRef.GetDocumentsAsync();
            var presetWords = presetCollection.ToObjects<Word>().ToList();
            if (presetWords != null)
            {
                var rnd = new Random();
                while (Counter < IntSelectedNumber)
                {
                    var randIndex = rnd.Next(presetWords.Count);
                    var word = presetWords[randIndex];
                    currentWord = word.Content;
                    await HatRef.GetDocument(currentWord)
                                .SetDataAsync(word);
                    addedWords.Add(word);
                }
            }
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
