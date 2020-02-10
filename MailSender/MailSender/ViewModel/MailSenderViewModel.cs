using ExtentionLib;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MailSender.ViewModel.WPFServices;
using MailSender_lib.Model;
using MailSender_lib.Model.Base;
using MailSender_lib.Services;
using MailSender_lib.Services.InMemory;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Threading;
using System.Windows.Threading;
using MailSender_lib.Services.Abstract;

namespace MailSender.ViewModel
{
    public class MailSenderViewModel : ViewModelBase
    {
        #region variables
        private InMemorySenderProvider senderProvider;
        private InMemoryServerProvider serverProvider;
        private InMemoryRecipientProvider recipientProvider;
        private InMemoryEmailProvider emailProvider;
        private InMemoryShedulerProvider shedulerProvider;
        private MailSenderService emailService;
        private WindowsService windowsService;
        private string windowTitle = "Рассыльщик почты v1.0";
        private string filter;
        private bool allRecipients;
        private Sender newSender;
        private Sender selectedSender;
        private Server newServer;
        private Server selectedServer;
        private Recipient newRecipient;
        private Recipient selectedRecipient;
        private string emailSubject;
        private string emailBody;
        private Email newEmail;
        private Email selectedEmail;
        private ShedulerTask selectedShedulerTask;
        private string selectedTime;
        private DateTime selectedDate;
        private int tabIndex;
        private Dictionary<int, TaskToken> tasks;
        #endregion

        #region Events
        private event Action RecipientsFiltrationEvent;
        #endregion

        #region Structs
        struct TaskToken
        {
            public Task taskValue;
            public CancellationTokenSource cts;
        }
        #endregion

        #region Properties
        public string WindowTitle
        {
            get => windowTitle;
            set => Set(ref windowTitle, value);
        }

        public string Filter
        {
            get => filter;
            set
            {
                Set(ref filter, value);
                RecipientsFiltrationEvent?.Invoke();
            }
        }

        public bool AllRecipients
        {
            get => allRecipients;
            set
            {
                Set(ref allRecipients, value);
                SelectedRecipient = null;
            }
        }

        public Sender NewSender
        {
            get => newSender;
            set => Set(ref newSender, value);
        }

        public Sender SelectedSender
        {
            get => selectedSender;
            set => Set(ref selectedSender, value);
        }

        public Server NewServer
        {
            get => newServer;
            set => Set(ref newServer, value);
        }

        public Server SelectedServer
        {
            get => selectedServer;
            set => Set(ref selectedServer, value);
        }

        public Recipient NewRecipient
        {
            get => newRecipient;
            set => Set(ref newRecipient, value);
        }

        public Recipient SelectedRecipient
        {
            get => selectedRecipient;
            set => Set(ref selectedRecipient, value);
        }

        public string EmailSubject
        {
            get => emailSubject;
            set => Set(ref emailSubject, value);
        }

        public string EmailBody
        {
            get => emailBody;
            set => Set(ref emailBody, value);
        }

        public Email SelectedEmail
        {
            get => selectedEmail;
            set => Set(ref selectedEmail, value);
        }

        public ShedulerTask SelectedShedulerTask
        {
            get => selectedShedulerTask;
            set => Set(ref selectedShedulerTask, value);
        }

        public string SelectedTime
        {
            get => selectedTime;
            set => Set(ref selectedTime, value);
        }

        public int TabIndex
        {
            get => tabIndex;
            set => Set(ref tabIndex, value);
        }

        public DateTime SelectedDate
        {
            get => selectedDate;
            set => Set(ref selectedDate, value);
        }

        public ObservableCollection<Sender> Senders { get; private set; }
        public ObservableCollection<Server> Servers { get; private set; }
        public ObservableCollection<Recipient> Recipients { get; private set; }
        public ObservableCollection<Email> Emails { get; private set; }
        public ObservableCollection<ShedulerTask> ShedulerTasks { get; private set; }
        #endregion

        #region Commands
        public ICommand CreateSenderCommand { get; }
        public ICommand EditSenderCommand { get; }
        public ICommand DeleteSenderCommand { get; }

        public ICommand CreateServerCommand { get; }
        public ICommand EditSeverCommand { get; }
        public ICommand DeleteServerCommand { get; }

        public ICommand CreateRecipientCommand { get; }
        public ICommand EditRecipientCommand { get; }
        public ICommand DeleteRecipientCommand { get; }

        public ICommand GoToShedulerCommand { get; }
        public ICommand PlanSendCommand { get; }
        public ICommand DeleteTaskCommand { get; }

        public ICommand FormEmailCommand { get; }
        public ICommand GoToEmailCommand { get; }
        public ICommand EditEmailCommand { get; }
        public ICommand DeleteEmailCommand { get; }

        public ICommand AcceptCommand { get; }
        public ICommand AbortCommand { get; }
        public ICommand TotalRefreshCommand { get; }
        public ICommand InitializeShedulerTasksCommand { get; }
        public ICommand DeleteNotActualTasks { get; }
        #endregion

        public MailSenderViewModel(MailSenderService EmailService, WindowsService WindowsService)
        {
            windowsService = WindowsService;
            emailService = EmailService;
            senderProvider = new InMemorySenderProvider("DataBase/Senders.db");
            serverProvider = new InMemoryServerProvider("DataBase/Servers.db");
            recipientProvider = new InMemoryRecipientProvider("DataBase/Recipients.db");
            emailProvider = new InMemoryEmailProvider("DataBase/Emails.db");
            shedulerProvider = new InMemoryShedulerProvider("DataBase/ShedulerTasks.db");
            tasks = new Dictionary<int, TaskToken>();

            Senders = new ObservableCollection<Sender>();
            Servers = new ObservableCollection<Server>();
            Recipients = new ObservableCollection<Recipient>();
            Emails = new ObservableCollection<Email>();
            ShedulerTasks = new ObservableCollection<ShedulerTask>();

            CreateSenderCommand = new RelayCommand(OnCreateSenderCommand);
            EditSenderCommand = new RelayCommand(OnEditSenderCommand);
            DeleteSenderCommand = new RelayCommand(OnDeleteSenderCommand);

            CreateServerCommand = new RelayCommand(OnCreateServerCommand);
            EditSeverCommand = new RelayCommand(OnEditSeverCommand);
            DeleteServerCommand = new RelayCommand(OnDeleteServerCommand);

            CreateRecipientCommand = new RelayCommand(OnCreateRecipientCommand);
            EditRecipientCommand = new RelayCommand(OnEditRecipientCommand);
            DeleteRecipientCommand = new RelayCommand(OnDeleteRecipientCommand);

            GoToShedulerCommand = new RelayCommand(OnGoToShedulerCommand);

            PlanSendCommand = new RelayCommand(OnPlanSendCommand);
            DeleteTaskCommand = new RelayCommand(OnDeleteTaskCommand);

            FormEmailCommand = new RelayCommand(OnFormEmailCommand);
            GoToEmailCommand = new RelayCommand(OnGoToEmailCommand);
            EditEmailCommand = new RelayCommand(OnEditEmailCommand);
            DeleteEmailCommand = new RelayCommand(OnDeleteEmailCommand);

            AcceptCommand = new RelayCommand(OnAcceptCommand);
            AbortCommand = new RelayCommand(OnAbortCommand);
            TotalRefreshCommand = new RelayCommand(OnTotalRefreshCommand);
            InitializeShedulerTasksCommand = new RelayCommand(OnInitializeShedulerTasksCommand);
            DeleteNotActualTasks = new RelayCommand(OnDeleteNotActualTasks);

            RecipientsFiltrationEvent += FiltrateRecipients;
        }

        #region ShedulerTasksWrapperMethods
        private void CreateShedulerTask(Recipient recipient)
        {
            var shedulerTask = new ShedulerTask()
            {
                Sender = SelectedSender,
                Recipient = recipient,
                Email = SelectedEmail,
            };

            if (shedulerTask.Sender == null || shedulerTask.Recipient == null || shedulerTask.Email == null)
                return;

            DateTime.TryParse(SelectedTime, out DateTime time);

            if (time != null)
                shedulerTask.Time = SelectedDate.AddHours(time.Hour);

            if (shedulerTask.Time != null && shedulerTask.Time > DateTime.Now)
                shedulerTask.Status = "Отправка запланирована на "
                                      + shedulerTask.Time.ToLongDateString()
                                      + " в " + SelectedTime;
            else
            {
                shedulerTask.Time = DateTime.Now;
                shedulerTask.Status = "Немедленная отправка...";
            }

            shedulerProvider.Create(shedulerTask);
            var success = shedulerProvider.SaveChanges();
            if (success)
                Refresh(ShedulerTasks);
        }

        private bool DeleteShedulerTask(ShedulerTask shedulerTask)
        {
            if (shedulerTask != null)
            {
                if (tasks != null && tasks.ContainsKey(shedulerTask.Id))
                {
                    var abort = tasks[shedulerTask.Id];
                    if (abort.cts != null)
                        abort.cts.Cancel();
                    tasks.Remove(shedulerTask.Id);
                }

                shedulerProvider.Delete(shedulerTask.Id);
                var success = shedulerProvider.SaveChanges();
                if (success)
                    Refresh(ShedulerTasks);
                return true;
            }
            else
                return false;
        }

        private void InitShedulerTasks()
        {
            if (ShedulerTasks != null && ShedulerTasks.Count > 0)
            {
                foreach (var shedulerTask in ShedulerTasks)
                {
                    if (tasks.ContainsKey(shedulerTask.Id)
                     ||(!shedulerTask.Status.StartsWith("Отправка запланирована") 
                     && !shedulerTask.Status.StartsWith("Немедленная отправка")))
                        continue;

                    var abort = new CancellationTokenSource();
                    var task = new Task(async () =>
                    {
                        var delay = shedulerTask.Time - DateTime.Now;
                        if (delay.TotalMinutes < 0)
                            delay = TimeSpan.FromMinutes(0);
                        var response = await SendTaskAsync(shedulerTask, delay);
                        if (response.Success)
                            shedulerTask.Status = "Успешно отправлено!";
                        else
                            shedulerTask.Status = response.Error;
                        shedulerProvider.Edit(shedulerTask.Id, shedulerTask);
                        var success = shedulerProvider.SaveChanges();
                        if (success)
                        {
                            var dispatcher = WindowsService.MainWindow.Dispatcher;
                            dispatcher.Invoke(DispatcherPriority.Normal,
                                             (ThreadStart)delegate ()
                                             {
                                                 Refresh(ShedulerTasks);
                                             });
                        }
                    }, abort.Token);
                    tasks.Add(shedulerTask.Id, new TaskToken() { taskValue = task, cts = abort });
                }

                var options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = 8
                };
                Parallel.ForEach(tasks.Values, options, task =>
                {
                    if (task.taskValue != null && task.taskValue.Status.ToString() == "Created")
                        task.taskValue.Start();
                });
            }
        }

        private async Task<Response> SendTaskAsync(ShedulerTask shedulerTask, TimeSpan delay)
        {
            if (shedulerTask == null) return new Response { Success = false, Error = "Список задач пуст" };

            await Task.Delay(delay);
            var response = await emailService.SendAsync(shedulerTask.Sender,
                                                         shedulerTask.Recipient,
                                                         shedulerTask.Email);
            return response;
        }
        #endregion

        private void Refresh<T>(ObservableCollection<T> collection) where T : BaseEntity
        {
            if (collection != null && collection.Count > 0)
                collection.Clear();

            if (collection is ObservableCollection<Sender>)
            {
                var success = senderProvider.ReadData();
                if (success)
                {
                    var items = senderProvider.GetAll();
                    if (items != null)
                        items.ToObservableCollection(Senders);
                }
            }

            if (collection is ObservableCollection<Server>)
            {
                var success = serverProvider.ReadData();
                if (success)
                {
                    var items = serverProvider.GetAll();
                    if (items != null)
                        items.ToObservableCollection(Servers);
                }
            }

            if (collection is ObservableCollection<Recipient>)
            {
                var success = recipientProvider.ReadData();
                if (success)
                {
                    var items = recipientProvider.GetAll();
                    if (items != null)
                        items.ToObservableCollection(Recipients);
                }
            }

            if (collection is ObservableCollection<Email>)
            {
                var success = emailProvider.ReadData();
                if (success)
                {
                    var items = emailProvider.GetAll();
                    if (items != null)
                        items.ToObservableCollection(Emails);
                }
            }

            if (collection is ObservableCollection<ShedulerTask>)
            {
                var success = shedulerProvider.ReadData();
                if (success)
                {
                    var items = shedulerProvider.GetAll();
                    if (items != null)
                        items.ToObservableCollection(ShedulerTasks);
                }
            }
        }

        private void FiltrateRecipients()
        {
            Refresh(Recipients);

            if (Recipients != null)
            {
                var recips = Recipients.Where(r => r.Name.StartsWith(Filter)).ToArray();
                if (recips != null && recips.Length > 0)
                {
                    Recipients.Clear();
                    recips.ToObservableCollection(Recipients);
                }
            }
        }

        #region OnMethods
        private void OnDeleteNotActualTasks()
        {
            for (int i = 0; i < ShedulerTasks.Count; i++)
            {
                if (!ShedulerTasks[i].Status.StartsWith("Отправка запланирована")
                 && !ShedulerTasks[i].Status.StartsWith("Немедленная отправка"))
                {
                    var success = DeleteShedulerTask(ShedulerTasks[i]);
                    if (success) i--;
                }
            }
        }

        private void OnInitializeShedulerTasksCommand()
        {
            InitShedulerTasks();
        }

        private void OnTotalRefreshCommand()
        {
            Refresh(Senders);
            Refresh(Servers);
            Refresh(Recipients);
            Refresh(Emails);
            Refresh(ShedulerTasks);
        }

        private void OnAbortCommand()
        {
            WindowsService.InputDataWindow.Close();
        }

        private void OnAcceptCommand()
        {
            if (WindowsService.InputDataWindow is AddSender)
            {
                senderProvider.Create(NewSender);
                var success = senderProvider.SaveChanges();
                if (success)
                    Refresh(Senders);
            }

            if (WindowsService.InputDataWindow is EditSender)
            {
                senderProvider.Edit(SelectedSender.Id, SelectedSender);
                var success = senderProvider.SaveChanges();
                if (success)
                    Refresh(Senders);
            }

            if (WindowsService.InputDataWindow is AddServer)
            {
                serverProvider.Create(NewServer);
                var success = serverProvider.SaveChanges();
                if (success)
                    Refresh(Servers);
            }

            if (WindowsService.InputDataWindow is EditServer)
            {
                serverProvider.Edit(SelectedServer.Id, SelectedServer);
                var success = serverProvider.SaveChanges();
                if (success)
                    Refresh(Servers);
            }

            if (WindowsService.InputDataWindow is AddRecipient)
            {
                recipientProvider.Create(NewRecipient);
                var success = recipientProvider.SaveChanges();
                if (success)
                {
                    Refresh(Recipients);
                    Filter = "";
                }
            }

            if (WindowsService.InputDataWindow is EditRecipient)
            {
                recipientProvider.Edit(SelectedRecipient.Id, SelectedRecipient);
                var success = recipientProvider.SaveChanges();
                if (success)
                {
                    if (Filter != null && Filter.Length > 0)
                        FiltrateRecipients();
                    else
                        Refresh(Recipients);
                }
            }

            WindowsService.InputDataWindow.Close();
        }

        private void OnFormEmailCommand()
        {
            var success = false;

            if (newEmail != null)
            {
                newEmail.Subject = EmailSubject;
                newEmail.Body = EmailBody;
                emailProvider.Create(newEmail);

                EmailSubject = "";
                EmailBody = "";
                newEmail = null;

                success = emailProvider.SaveChanges();
            }
            else
            {
                SelectedEmail.Subject = EmailSubject;
                SelectedEmail.Body = EmailBody;
                emailProvider.Edit(SelectedEmail.Id, SelectedEmail);
                success = emailProvider.SaveChanges();
            }
            if (success)
                Refresh(Emails);

            TabIndex = 1;
        }

        private void OnDeleteEmailCommand()
        {
            if (SelectedEmail != null)
            {
                emailProvider.Delete(SelectedEmail.Id);
                var success = emailProvider.SaveChanges();
                if (success)
                    Refresh(Emails);
            }
        }

        private void OnEditEmailCommand()
        {
            if (SelectedEmail != null)
            {
                TabIndex = 2;
                EmailSubject = SelectedEmail.Subject;
                EmailBody = SelectedEmail.Body;
            }
        }

        private void OnGoToEmailCommand()
        {
            TabIndex = 2;
            newEmail = new Email();
            EmailSubject = "";
            EmailBody = "";
        }

        private void OnGoToShedulerCommand()
        {
            TabIndex = 1;
        }

        private void OnDeleteTaskCommand()
        {
            DeleteShedulerTask(SelectedShedulerTask);
        }

        private void OnPlanSendCommand()
        {
            if (!AllRecipients)
            {
                if (SelectedRecipient == null) return;
                CreateShedulerTask(SelectedRecipient);
            }
            else
            {
                if (Recipients == null) return;

                foreach (var recip in Recipients)
                    CreateShedulerTask(recip);
            }

            InitShedulerTasks();
        }

        private void OnDeleteRecipientCommand()
        {
            if (SelectedRecipient != null)
            {
                recipientProvider.Delete(SelectedRecipient.Id);
                var success = recipientProvider.SaveChanges();
                if (success)
                {
                    if (Filter != null && Filter.Length > 0)
                        FiltrateRecipients();
                    else
                        Refresh(Recipients);
                }
            }
        }

        private void OnEditRecipientCommand()
        {
            var window = windowsService.CreateWindow<EditRecipient>();
            window.ShowDialog();
        }

        private void OnCreateRecipientCommand()
        {
            NewRecipient = new Recipient();
            var window = windowsService.CreateWindow<AddRecipient>();
            window.ShowDialog();
        }

        private void OnDeleteServerCommand()
        {
            if (SelectedServer != null)
            {
                serverProvider.Delete(SelectedServer.Id);
                var success = serverProvider.SaveChanges();
                if (success)
                    Refresh(Servers);
            }
        }

        private void OnEditSeverCommand()
        {
            var window = windowsService.CreateWindow<EditServer>();
            window.ShowDialog();
        }

        private void OnCreateServerCommand()
        {
            newServer = new Server();
            var window = windowsService.CreateWindow<AddServer>();
            window.ShowDialog();
        }

        private void OnDeleteSenderCommand()
        {
            if (SelectedSender != null)
            {
                senderProvider.Delete(SelectedSender.Id);
                var success = senderProvider.SaveChanges();
                if (success)
                    Refresh(Senders);
            }
        }

        private void OnEditSenderCommand()
        {
            var window = windowsService.CreateWindow<EditSender>();
            window.ShowDialog();
        }

        private void OnCreateSenderCommand()
        {
            NewSender = new Sender();
            var window = windowsService.CreateWindow<AddSender>();
            window.ShowDialog();
        }
        #endregion
    }
}