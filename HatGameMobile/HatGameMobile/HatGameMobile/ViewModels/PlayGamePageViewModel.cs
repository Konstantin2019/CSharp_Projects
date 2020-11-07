using DynamicData;
using HatGameMobile.Models;
using Plugin.CloudFirestore;
using Plugin.CloudFirestore.Extensions;
using Prism.Commands;
using Prism.Navigation;
using Prism.Services;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;
using Timer = System.Timers.Timer;

namespace HatGameMobile.ViewModels
{
    public class PlayGamePageViewModel : ViewModelBase
    {
        private bool sessionIsActive;
        private bool roundTimerIsStarted;
        private int numberOfReadyTeams;
        private byte stage;
        private string currentWord;
        private int currentIndex;
        private int score;
        private int wordsInHat;
        private int penalty;
        private bool startCanExecute;
        private bool skipCanExecute;
        private bool guessedCanExecute;
        private double roundTime;
        private string currentTeamName;
        private IListenerRegistration hatListener;
        private List<Word> guessedWords;
        private List<Team> teams;
        private readonly IObservable<Unit> extract;
        private readonly IObservable<Unit> guessed;
        private readonly Stopwatch faultTimer;
        private readonly Timer roundTimer;
        private List<string> alertInfo;
        private readonly IDispatcher dispatcher;

        public bool SessionIsActive
        {
            get => sessionIsActive;
            set => SetProperty(ref sessionIsActive, value);
        }
        public bool RoundTimerIsStarted
        {
            get => roundTimerIsStarted;
            set => SetProperty(ref roundTimerIsStarted, value);
        }
        public int NumberOfReadyTeams
        {
            get => numberOfReadyTeams;
            set => SetProperty(ref numberOfReadyTeams, value);
        }
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
        public bool GuessedCanExecute
        {
            get => guessedCanExecute;
            set => SetProperty(ref guessedCanExecute, value);
        }
        public double RoundTime 
        {
            get => roundTime;
            set => SetProperty(ref roundTime, value);
        }
        public string CurrentTeamName 
        {
            get => currentTeamName;
            set => SetProperty(ref currentTeamName, value);
        }
        public ICommand StartRoundCommand { get; }
        public ICommand GuessedCommand { get; }
        public ICommand SkipCommand { get; }
        public ICommand NavigateToMainMenu { get; }
        public ICommand NavigateToRoom { get; }

        public PlayGamePageViewModel(INavigationService navigationService, IPageDialogService dialogService)
            : base(navigationService, dialogService)
        {
            Stage = 1;
            RoundTime = 1;
            penalty = 0;
            guessedWords = new List<Word>();
            teams = new List<Team>();

            alertInfo = new List<string>
            {
                "First stage is starting. Try to explain word with another not single-root word. Good luck!",
                "Second stage is starting. Try to explain word with gestures. Good luck!",
                "Third and last stage is starting. Try to explain word with one not single-root word. Good luck!",
            };
            dispatcher = Prism.PrismApplicationBase.Current.Dispatcher;

            StartRoundCommand = new DelegateCommand(OnStartRoundExecuted).ObservesCanExecute(() => StartCanExecute);
            SkipCommand = new DelegateCommand(OnSkipExecuted).ObservesCanExecute(() => SkipCanExecute);
            GuessedCommand = new DelegateCommand(OnGuessedExecuted).ObservesCanExecute(() => GuessedCanExecute);
            NavigateToMainMenu = new DelegateCommand(OnNavigateToMainMenuExecuted, CanNavigateToMainMenuExecute)
                                                    .ObservesProperty(() => SessionIsActive);
            NavigateToRoom = new DelegateCommand(OnNavigateToRoomExecuted, CanNavigateToRoomExecute)
                                                .ObservesProperty(() => SessionIsActive);

            faultTimer = new Stopwatch();

            roundTimer = new Timer(100);
            roundTimer.Elapsed += RoundTimer_Elapsed;
            roundTimer.AutoReset = true;
                   
            Observable.FromAsync(async _ =>
            {
                dispatcher.BeginInvokeOnMainThread(async () =>
                {
                    await DialogService.DisplayAlertAsync("Information", alertInfo[Stage-1], "OK");
                });
                var sessionRequest = await SessionRef.GetDocumentsAsync();
                NumberOfReadyTeams = sessionRequest.ToObjects<Session>().Select(s => s.NumberOfReadyTeams).Single();
                NumberOfReadyTeams++;
                var hatRequest = await HatRef.GetDocumentsAsync();
                return hatRequest.Count;
            }).Subscribe(s => 
            {
                Title = $"{Stage} stage is running...";
                WordsInHat = s;
                if (teams != null)
                {
                    currentIndex = teams.Select(t => t.IsHost).IndexOf(true);
                    CurrentTeamName = teams[currentIndex].Name;
                    if (App.IsHost)
                        StopMode();
                    else
                        DisableMode();
                }
                else
                    throw new NullReferenceException("Something bad happened...");
            });

            extract = Observable.FromAsync(async _ => 
            {
                var request = await HatRef.GetDocumentsAsync();
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

            guessed = Observable.FromAsync(async _ =>
            {
                guessedWords.Add(new Word { Content = CurrentWord });
                await HatRef.GetDocument(CurrentWord)
                            .DeleteDocumentAsync();
            });

            RoomRef.ObserveRemoved()
                   .Subscribe(s =>
                   {
                       if (s.Document.Id == App.RoomId)
                       {
                           if (!App.IsHost) 
                           {
                               dispatcher.BeginInvokeOnMainThread(async () =>
                               {
                                   await DialogService.DisplayAlertAsync("Information", "Session was terminated by host!", "OK");
                                   await NavigationService.NavigateAsync("/NavigationPage/WelcomePage");
                               });
                           }
                       }
                   });

            SessionRef.ObserveModified()
                      .Subscribe(change => 
                      {
                          var status = change.Document.ToObject<Session>();
                          if (status.TimerStarted)
                              roundTimer.Start();
                          else
                              roundTimer.Stop();
                      });

            TeamsRef.ObserveAdded()
                    .Subscribe(t =>
                    {
                        var team = t.Document.ToObject<Team>();
                        if (!teams.Contains(team))
                            teams.Add(team);
                    });

            hatListener = HatRef.AddSnapshotListener((snapshot, error) =>
            {
                WordsInHat = snapshot.Count;
                if (WordsInHat == 0) 
                {
                    DisableMode();
                    Stage++;

                    Task.Run(async () =>
                    {
                        foreach (var word in guessedWords)
                            await HatRef.GetDocument(word.Content)
                                        .SetDataAsync(word);
                    }).ContinueWith(t => { StartCanExecute = true; });
                }
            });

            this.ObservableForProperty(p => p.Stage)
                .Select(p => p.Value)
                .Subscribe(s => 
                {
                    if (s <= 3) 
                    {
                        Title = $"{s} stage is running...";
                        dispatcher.BeginInvokeOnMainThread(async () =>
                        {
                            await DialogService.DisplayAlertAsync("Information", alertInfo[s-1], "OK");
                        });
                    }
                    else 
                    {
                        Title = $"Finish -> Your score:{Score}";
                        dispatcher.BeginInvokeOnMainThread(async () =>
                        {
                            await DialogService.DisplayAlertAsync("Information", "Game is finished. Hope your enjoyed:)", "OK");
                        });
                        DisableMode();
                        SessionIsActive = false;
                        RoundTimerIsStarted = false;
                    }
                });

            this.ObservableForProperty(p => p.SessionIsActive)
                .Select(p => p.Value)
                .Subscribe(s =>
                {
                    Task.Run(async () =>
                    {
                        await SessionRef.GetDocument("CurrentSession")
                                        .UpdateDataAsync(new { IsActive = s });
                    });
                });

            this.ObservableForProperty(p => p.RoundTimerIsStarted)
                .Select(p => p.Value)
                .Subscribe(s =>
                {
                    Task.Run(async () =>
                    {
                        await SessionRef.GetDocument("CurrentSession")
                                        .UpdateDataAsync(new { TimerStarted = s });
                    });
                });

            this.ObservableForProperty(p => p.NumberOfReadyTeams)
                .Select(p => p.Value)
                .Subscribe(s =>
                {
                    Task.Run(async () =>
                    {
                        var request = await SessionRef.GetDocumentsAsync();
                        var numberOfReadyTeams = request.ToObjects<Session>().Select(r => r.NumberOfReadyTeams).Single();
                        numberOfReadyTeams += s;
                        await SessionRef.GetDocument("CurrentSession")
                                        .UpdateDataAsync(new { NumberOfReadyTeams = numberOfReadyTeams });
                    });
                });

        }
        private bool CanNavigateToRoomExecute()
        {
            if (SessionIsActive)
                return false;
            else
                return true;
        }
        private bool CanNavigateToMainMenuExecute()
        {
            if (SessionIsActive) 
            {
                if (App.IsHost)
                    return true;
                else
                    return false;
            }
            else
                return true;
        }
        private void RoundTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            RoundTime -= roundTimer.Interval / 60000;

            if (RoundTime < 0) 
            {
                dispatcher.BeginInvokeOnMainThread(async () =>
                {
                    var answer = await DialogService.DisplayAlertAsync("Question", $"Is the last word guessed?", "YES", "NO");
                    if (answer) OnGuessedExecuted();
                });

                RoundTime = 1;

                currentIndex++;
                if (currentIndex == teams.Count)
                    currentIndex = 0;

                RoundTimerIsStarted = false;

                CurrentTeamName = teams[currentIndex].Name;
                if (CurrentTeamName != App.TeamName)
                    DisableMode();
                else
                    StopMode();
            }
        }
        private void OnStartRoundExecuted()
        {
            if (!SessionIsActive)
                SessionIsActive = true;

            if (NumberOfReadyTeams != teams.Count)
            {
                sessionIsActive = false;
                dispatcher.BeginInvokeOnMainThread(async () =>
                {
                    await DialogService.DisplayAlertAsync("Information", "Wait for all teams...", "OK");
                });
            }
            else
                RoundTimerIsStarted = true;

            extract.Subscribe(s =>
            {
                faultTimer.Start();
                PlayMode();
            });
        }
        private void OnSkipExecuted()
        {
            if (faultTimer.ElapsedMilliseconds > 5000)
                penalty++;
            extract.Subscribe(s => { faultTimer.Restart(); });
        }
        private void OnGuessedExecuted()
        {
            guessed.Subscribe(s =>
            {
                Score += 1 - penalty;
                penalty = 0;
                CurrentWord = null;
                if (wordsInHat > 0)
                {
                    if (roundTimer.Enabled)
                        extract.Subscribe(_ => { faultTimer.Restart(); });
                    else
                        faultTimer.Stop();
                }
            });
        }
        private async void OnNavigateToRoomExecuted()
        {
            NumberOfReadyTeams += -2;
            await NavigationService.NavigateAsync("/NavigationPage/MainPage");
        }
        private async void OnNavigateToMainMenuExecuted()
        {
            await NavigationService.NavigateAsync("/NavigationPage/WelcomePage");
            await RemoveFromFireStore(App.IsHost);
        }
        private void PlayMode() 
        {
            StartCanExecute = false;
            SkipCanExecute = true;
            GuessedCanExecute = true;
        }
        private void StopMode()
        {
            StartCanExecute = true;
            SkipCanExecute = false;
            GuessedCanExecute = false;
        }
        private void DisableMode()
        {
            StartCanExecute = false;
            SkipCanExecute = false;
            GuessedCanExecute = false;
        }
        protected override async Task RemoveFromFireStore(bool isHost)
        {
            if (isHost)
                hatListener.Remove();
            else
                NumberOfReadyTeams += -2;

            await base.RemoveFromFireStore(isHost);
        }
    }
}
