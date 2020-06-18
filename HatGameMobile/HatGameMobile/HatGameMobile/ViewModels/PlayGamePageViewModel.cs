using HatGameMobile.Models;
using Plugin.CloudFirestore;
using Prism.Commands;
using Prism.Navigation;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace HatGameMobile.ViewModels
{
    public class PlayGamePageViewModel : ViewModelBase
    {
        private byte stage;
        private string currentWord;
        private int score;
        private int wordsInHat;
        private int penalty;
        private bool startCanExecute;
        private bool skipCanExecute;
        private bool doneCanExecute;
        private readonly ICollectionReference hatCollectionRef;
        private List<Word> doneWords;
        private readonly IObservable<Unit> startSeq;
        private readonly IObservable<Unit> doneSeq;
        private readonly Stopwatch timer;
        private byte Stage
        {
            get => stage;
            set => SetProperty(ref stage, value);
        }
        public string CurrentWord
        {
            get => currentWord;
            set => SetProperty(ref currentWord, value);
        }
        public int Score
        {
            get => score;
            set => SetProperty(ref score, value);
        }
        public int WordsInHat
        {
            get => wordsInHat;
            set => SetProperty(ref wordsInHat, value);
        }
        public bool StartCanExecute
        {
            get => startCanExecute;
            set => SetProperty(ref startCanExecute, value);
        }
        public bool SkipCanExecute
        {
            get => skipCanExecute;
            set => SetProperty(ref skipCanExecute, value);
        }
        public bool DoneCanExecute
        {
            get => doneCanExecute;
            set => SetProperty(ref doneCanExecute, value);
        }
        public DelegateCommand StartRoundCommand { get; }
        public DelegateCommand DoneCommand { get; }
        public DelegateCommand SkipCommand { get; }
        public PlayGamePageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Stage = 1;
            penalty = 0;
            doneWords = new List<Word>();
            StartRoundCommand = new DelegateCommand(OnStartRoundExecuted).ObservesCanExecute(() => StartCanExecute);
            SkipCommand = new DelegateCommand(OnSkipExecuted).ObservesCanExecute(() => SkipCanExecute);
            DoneCommand = new DelegateCommand(OnDoneExecuted).ObservesCanExecute(() => DoneCanExecute);
            hatCollectionRef = CrossCloudFirestore.Current.Instance.GetCollection("Hat");
            timer = new Stopwatch();

            Observable.FromAsync(async _ =>
            {
                var request = await hatCollectionRef.GetDocumentsAsync();
                return request;
            }).Select(r => r.Count).Subscribe(s => 
            {
                WordsInHat = s;
                if (s < 10)
                {
                    Title = "Шляпа пуста :(";
                    StartCanExecute = false;
                    SkipCanExecute = false;
                    DoneCanExecute = false;
                }
                else 
                {
                    Title = $"Идёт {Stage} этап";
                    StartCanExecute = true;
                    SkipCanExecute = false;
                    DoneCanExecute = false;
                }
            });

            startSeq = Observable.FromAsync(async _ => 
            {
                var request = await hatCollectionRef.GetDocumentsAsync();
                return request.ToObjects<Word>().ToList();
            }).Select(words => 
            {
                if (words.Count > 1)
                {
                    var rnd = new Random();
                    while (true)
                    {
                        var randIndex = rnd.Next(words.Count);
                        if (words[randIndex].Content != CurrentWord)
                        {
                            CurrentWord = words[randIndex].Content;
                            break;
                        }
                    }
                }
                else
                    CurrentWord = words.Select(w => w.Content).Single();
                return Unit.Default;
            });

            doneSeq = Observable.FromAsync(async _ => 
            {
                doneWords.Add(new Word { Content = CurrentWord });
                await hatCollectionRef.GetDocument(CurrentWord).DeleteDocumentAsync();
            });

            hatCollectionRef.AddSnapshotListener((snapshot, error) =>
            {
                WordsInHat = snapshot.Count;
                if (WordsInHat == 0) 
                {
                    Stage++;

                    if (Stage == 4)
                        CrossCloudFirestore.Current.Instance.DisableNetworkAsync();

                    Task.Run(async () =>
                    {
                        foreach (var word in doneWords)
                            await hatCollectionRef.GetDocument(word.Content).SetDataAsync(word);
                    }).ContinueWith(t => { StartCanExecute = true; });
                }
            });

            this.ObservableForProperty(p => p.Stage)
                .Select(p => p.Value)
                .Subscribe(s => 
                {
                    if (s <= 3)
                        Title = $"Идёт {s} этап";
                    else 
                    {
                        Title = "Игра завершена";
                        StartCanExecute = false;
                        SkipCanExecute = false;
                        DoneCanExecute = false;
                    }
                });

            this.ObservableForProperty(p => p.WordsInHat)
                .Select(p => p.Value)
                .Subscribe(s => 
                {
                    if (s == 0) 
                    {
                        CurrentWord = null;
                        SkipCanExecute = false;
                        DoneCanExecute = false;
                    }
                });
        }
        private void OnSkipExecuted()
        {
            if (timer.ElapsedMilliseconds > 5000)
                penalty++;

            startSeq.Subscribe(s => { timer.Restart(); });
        }
        private void OnStartRoundExecuted()
        {
            startSeq.Subscribe(s =>
            {
                timer.Start();
                StartCanExecute = false;
                SkipCanExecute = true;
                DoneCanExecute = true;
            });
        }
        private void OnDoneExecuted()
        {
            doneSeq.Subscribe(s => 
            {
                Score += 1 - penalty;
                penalty = 0;
                if (wordsInHat > 0)
                    startSeq.Subscribe(_ => { timer.Restart(); });
            });
        }
    }
}
