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
using Timer = System.Timers.Timer;

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
        private double roundTime;
        private readonly ICollectionReference hatCollectionRef;
        private List<Word> doneWords;
        private readonly IObservable<Unit> startSeq;
        private readonly IObservable<Unit> doneSeq;
        private readonly Stopwatch faultTimer;
        private readonly Timer roundTimer;
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
        public double RoundTime 
        {
            get => roundTime;
            set => SetProperty(ref roundTime, value);
        }
        public DelegateCommand StartRoundCommand { get; }
        public DelegateCommand DoneCommand { get; }
        public DelegateCommand SkipCommand { get; }
        public PlayGamePageViewModel(INavigationService navigationService)
            : base(navigationService)
        {
            Stage = 1;
            RoundTime = 1;
            penalty = 0;
            doneWords = new List<Word>();

            StartRoundCommand = new DelegateCommand(OnStartRoundExecuted).ObservesCanExecute(() => StartCanExecute);
            SkipCommand = new DelegateCommand(OnSkipExecuted).ObservesCanExecute(() => SkipCanExecute);
            DoneCommand = new DelegateCommand(OnDoneExecuted).ObservesCanExecute(() => DoneCanExecute);

            hatCollectionRef = CrossCloudFirestore.Current.Instance.GetCollection("Hat");

            faultTimer = new Stopwatch();

            roundTimer = new Timer(1000);
            roundTimer.Elapsed += RoundTimer_Elapsed;
            roundTimer.AutoReset = true;

            Observable.FromAsync(async _ =>
            {
                var request = await hatCollectionRef.GetDocumentsAsync();
                return request;
            }).Select(r => r.Count).Subscribe(s => 
            {
                WordsInHat = s;
                if (s < 10)
                {
                    Title = "Недостаточно слов :(";
                    DisableMode();
                }
                else 
                {
                    Title = $"Идёт {Stage} этап";
                    StopMode();
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
                    DisableMode();
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
                        DisableMode();
                    }
                });
        }

        private void RoundTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RoundTime -= roundTimer.Interval / 60000;
            if (RoundTime < 0) 
            {
                roundTimer.Stop();
                StopMode();
            }
        }

        private void OnSkipExecuted()
        {
            if (faultTimer.ElapsedMilliseconds > 5000)
                penalty++;

            startSeq.Subscribe(s => { faultTimer.Restart(); });
        }
        private void OnStartRoundExecuted()
        {
            roundTimer.Start();

            startSeq.Subscribe(s =>
            {
                faultTimer.Start();
                PlayMode();
            });
        }
        private void OnDoneExecuted()
        {
            doneSeq.Subscribe(s => 
            {
                Score += 1 - penalty;
                penalty = 0;
                if (wordsInHat > 0)
                    startSeq.Subscribe(_ => { faultTimer.Restart(); });
            });
        }
        private void PlayMode() 
        {
            StartCanExecute = false;
            SkipCanExecute = true;
            DoneCanExecute = true;
        }
        private void StopMode()
        {
            CurrentWord = null;
            StartCanExecute = true;
            SkipCanExecute = false;
            DoneCanExecute = false;
        }
        private void DisableMode()
        {
            CurrentWord = null;
            StartCanExecute = false;
            SkipCanExecute = false;
            DoneCanExecute = false;
        }
    }
}
