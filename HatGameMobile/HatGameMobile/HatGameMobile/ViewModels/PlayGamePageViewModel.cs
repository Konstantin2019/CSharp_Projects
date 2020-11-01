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
using Timer = System.Timers.Timer;

namespace HatGameMobile.ViewModels
{
    public class PlayGamePageViewModel : ViewModelBase
    {
        private bool isHost;
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
        private bool doneCanExecute;
        private double roundTime;
        private string currentTeamName;
        private readonly ICollectionReference roomRef;
        private readonly ICollectionReference hatRef;
        private readonly ICollectionReference sessionRef;
        private readonly ICollectionReference teamsRef;
        private IListenerRegistration hatListener;
        private List<Word> doneWords;
        private List<Team> teams;
        private readonly IObservable<Unit> extract;
        private readonly IObservable<Unit> done;
        private readonly Stopwatch faultTimer;
        private readonly Timer roundTimer;
        private IPageDialogService dialog;

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
        public string CurrentTeamName 
        {
            get => currentTeamName;
            set => SetProperty(ref currentTeamName, value);
        }
        public ICommand StartRoundCommand { get; }
        public ICommand DoneCommand { get; }
        public ICommand SkipCommand { get; }
        public ICommand NavigateToMainMenu { get; }
        public ICommand NavigateToRoom { get; }

        public PlayGamePageViewModel(INavigationService navigationService, IPageDialogService dialogService)
            : base(navigationService)
        {
            dialog = dialogService;

            Stage = 1;
            RoundTime = 1;
            penalty = 0;
            doneWords = new List<Word>();
            teams = new List<Team>();

            StartRoundCommand = new DelegateCommand(OnStartRoundExecuted).ObservesCanExecute(() => StartCanExecute);
            SkipCommand = new DelegateCommand(OnSkipExecuted).ObservesCanExecute(() => SkipCanExecute);
            DoneCommand = new DelegateCommand(OnDoneExecuted).ObservesCanExecute(() => DoneCanExecute);
            NavigateToMainMenu = new DelegateCommand(OnNavigateToMainMenuExecuted, CanNavigateToMainMenuExecute)
                                                    .ObservesProperty(() => SessionIsActive);
            NavigateToRoom = new DelegateCommand(OnNavigateToRoomExecuted, CanNavigateToRoomExecute)
                                                .ObservesProperty(() => SessionIsActive);

            roomRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom");
            hatRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                         .GetDocument(App.RoomId)
                                                         .GetCollection("Hat");
            sessionRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                             .GetDocument(App.RoomId)
                                                             .GetCollection("Session");
            teamsRef = CrossCloudFirestore.Current.Instance.GetCollection("GameRoom")
                                                           .GetDocument(App.RoomId)
                                                           .GetCollection("Teams");

            faultTimer = new Stopwatch();

            roundTimer = new Timer(100);
            roundTimer.Elapsed += RoundTimer_Elapsed;
            roundTimer.AutoReset = true;
                   
            Observable.FromAsync(async _ =>
            {
                var sessionRequest = await sessionRef.GetDocumentsAsync();
                NumberOfReadyTeams = sessionRequest.ToObjects<Session>().Select(s => s.NumberOfReadyTeams).Single();
                NumberOfReadyTeams++;
                var hatRequest = await hatRef.GetDocumentsAsync();
                return hatRequest.Count;
            }).Subscribe(s => 
            {
                Title = $"Идёт {Stage} этап";
                WordsInHat = s;
                if (teams != null)
                {
                    currentIndex = teams.Select(t => t.IsHost).IndexOf(true);
                    isHost = teams.Where(t => t.Name == App.TeamName).Select(t => t.IsHost).FirstOrDefault();
                    CurrentTeamName = teams[currentIndex].Name;
                    if (isHost)
                        StopMode();
                    else
                        DisableMode();
                }
                else
                    throw new NullReferenceException("Что то пошло не так с подпиской...");
            });

            extract = Observable.FromAsync(async _ => 
            {
                var request = await hatRef.GetDocumentsAsync();
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

            done = Observable.FromAsync(async _ => 
            {
                doneWords.Add(new Word { Content = CurrentWord });
                await hatRef.GetDocument(CurrentWord).DeleteDocumentAsync();
            });

            roomRef.ObserveRemoved()
                   .Subscribe(s =>
                   {
                       if (s.Document.Id == App.RoomId)
                       {
                           if (!isHost) 
                           {
                               var dispatcher = Prism.PrismApplicationBase.Current.Dispatcher;
                               dispatcher.BeginInvokeOnMainThread(async () =>
                               {
                                   await dialog.DisplayAlertAsync("Информация", "Хост прервал сессию!", "OK");
                                   await NavigationService.NavigateAsync("/NavigationPage/WelcomePage");
                               });
                           }
                       }
                   });

            sessionRef.ObserveModified()
                      .Subscribe(change => 
                      {
                          var status = change.Document.ToObject<Session>();
                          if (status.TimerStarted)
                              roundTimer.Start();
                          else
                              roundTimer.Stop();
                      });

            teamsRef.ObserveAdded()
                    .Subscribe(t =>
                    {
                        var team = t.Document.ToObject<Team>();
                        if (!teams.Contains(team))
                            teams.Add(team);
                    });

            hatListener = hatRef.AddSnapshotListener((snapshot, error) =>
            {
                WordsInHat = snapshot.Count;
                if (WordsInHat == 0) 
                {
                    DisableMode();
                    Stage++;

                    Task.Run(async () =>
                    {
                        foreach (var word in doneWords)
                            await hatRef.GetDocument(word.Content).SetDataAsync(word);
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
                        Title = $"Финиш -> Cчёт:{Score}";
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
                        await sessionRef.GetDocument("CurrentSession").UpdateDataAsync(new { IsActive = s });
                    });
                });

            this.ObservableForProperty(p => p.RoundTimerIsStarted)
                .Select(p => p.Value)
                .Subscribe(s =>
                {
                    Task.Run(async () =>
                    {
                        await sessionRef.GetDocument("CurrentSession").UpdateDataAsync(new { TimerStarted = s });
                    });
                });

            this.ObservableForProperty(p => p.NumberOfReadyTeams)
                .Select(p => p.Value)
                .Subscribe(s =>
                {
                    Task.Run(async () =>
                    {
                        var request = await sessionRef.GetDocumentsAsync();
                        var numberOfReadyTeams = request.ToObjects<Session>().Select(r => r.NumberOfReadyTeams).Single();
                        numberOfReadyTeams += s;
                        await sessionRef.GetDocument("CurrentSession").UpdateDataAsync(new { NumberOfReadyTeams = numberOfReadyTeams });
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
                if (isHost)
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
                RoundTime = 1;
                if (currentIndex < teams.Count - 1)
                    currentIndex++;
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
                var dispatcher = Prism.PrismApplicationBase.Current.Dispatcher;
                dispatcher.BeginInvokeOnMainThread(async () =>
                {
                    await dialog.DisplayAlertAsync("Информация", "Не все команды готовы!", "OK");
                });
            }
            else
            {
                if (currentIndex == teams.Count)
                    currentIndex = 0;
                RoundTimerIsStarted = true;
            }
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
        private void OnDoneExecuted()
        {
            done.Subscribe(s => 
            {
                Score += 1 - penalty;
                penalty = 0;
                if (wordsInHat > 0)
                    extract.Subscribe(_ => { faultTimer.Restart(); });
            });
        }
        private async void OnNavigateToRoomExecuted()
        {
            NumberOfReadyTeams += -2;
            await NavigationService.NavigateAsync("/NavigationPage/MainPage");
        }
        private async void OnNavigateToMainMenuExecuted()
        {
            if (!isHost) 
            {
                await NavigationService.NavigateAsync("/NavigationPage/WelcomePage");
                NumberOfReadyTeams += -2;
                await teamsRef.GetDocument(App.TeamName).DeleteDocumentAsync();
            }
            else 
            {
                await NavigationService.NavigateAsync("/NavigationPage/WelcomePage");

                await sessionRef.GetDocument("CurrentSession").DeleteDocumentAsync();

                var teamsRequest = await teamsRef.GetDocumentsAsync();
                var teams = teamsRequest.Documents.Select(d => d.Id).ToList();
                foreach (var t in teams)
                    await teamsRef.GetDocument(t).DeleteDocumentAsync();

                hatListener.Remove();
                var hatRequest = await hatRef.GetDocumentsAsync();
                var words = hatRequest.Documents.Select(d => d.Id).ToList();
                foreach (var w in words)
                    await hatRef.GetDocument(w).DeleteDocumentAsync();

                await roomRef.GetDocument(App.RoomId).DeleteDocumentAsync();
            }
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
