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
        private event Action<InMemoryDataProvider<Recipient>, ObservableCollection<Recipient>> RecipientsFiltrationEvent;
        #endregion

        #region Structs and Enums
        struct TaskToken
        {
            public Task taskValue;
            public CancellationTokenSource cts;
        }

        enum ItemStatus
        {
            Create, Edit, Delete
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
                RecipientsFiltrationEvent?.Invoke(recipientProvider, Recipients);
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

            RecipientsFiltrationEvent += Filtrate;
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
                Refresh(shedulerProvider, ShedulerTasks);
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
                    Refresh(shedulerProvider, ShedulerTasks);
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
                                                 Refresh(shedulerProvider, ShedulerTasks);
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

        #region CommonMethods
        private void Refresh<T>(InMemoryDataProvider<T> provider, ObservableCollection<T> collection)
            where T : BaseEntity
        {
            var success = provider.ReadData();
            if (success)
            {
                var items = provider.GetAll();
                if (items != null)
                {
                    collection.Clear();
                    items.ToObservableCollection(collection);
                }
            }
        }

        private void Filtrate<T>(InMemoryDataProvider<T> provider, ObservableCollection<T> recipients)
         where T : NamedEntity
        {
            Refresh(provider, recipients);

            if (recipients != null)
            {
                var recips = recipients.Where(r => r.Name.StartsWith(Filter)).ToArray();
                if (recips != null && recips.Length > 0)
                {
                    recipients.Clear();
                    recips.ToObservableCollection(recipients);
                }
            }
        }

        private bool UpgradeCollection<T>(ItemStatus status, 
                                          InMemoryDataProvider<T> provider, 
                                          ObservableCollection<T> collection, T item, 
                                          Action<InMemoryDataProvider<T>, ObservableCollection<T>> RefreshMethod)
            where T : BaseEntity
        {
            try
            {
                if (status == ItemStatus.Create)
                {
                    provider.Create(item);
                    var success = provider.SaveChanges();
                    if (success)
                        RefreshMethod(provider, collection);
                }

                if (status == ItemStatus.Edit)
                {
                    if (item != null)
                    {
                        provider.Edit(item.Id, item);
                        var success = provider.SaveChanges();
                        if (success)
                            RefreshMethod(provider, collection);
                    }
                }

                if (status == ItemStatus.Delete)
                {
                    if (item != null)
                    {
                        provider.Delete(item.Id);
                        var success = provider.SaveChanges();
                        if (success)
                            RefreshMethod(provider, collection);
                    }
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion

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
            Refresh(senderProvider, Senders);
            Refresh(serverProvider, Servers);
            Refresh(recipientProvider, Recipients);
            Refresh(emailProvider, Emails);
            Refresh(shedulerProvider, ShedulerTasks);
        }

        private void OnAbortCommand()
        {
            WindowsService.InputDataWindow.Close();
        }

        private void OnAcceptCommand()
        {
            if (WindowsService.InputDataWindow is AddSender)
                UpgradeCollection(ItemStatus.Create, senderProvider, Senders, NewSender, Refresh);

            if (WindowsService.InputDataWindow is EditSender)
                UpgradeCollection(ItemStatus.Edit, senderProvider, Senders, SelectedSender, Refresh);

            if (WindowsService.InputDataWindow is AddServer)
                UpgradeCollection(ItemStatus.Create, serverProvider, Servers, NewServer, Refresh);

            if (WindowsService.InputDataWindow is EditServer)
                UpgradeCollection(ItemStatus.Edit, serverProvider, Servers, SelectedServer, Refresh);

            if (WindowsService.InputDataWindow is AddRecipient)
            {
                var success = UpgradeCollection(ItemStatus.Create, recipientProvider, Recipients, NewRecipient, Refresh);
                if (success)
                    Filter = "";
            }

            if (WindowsService.InputDataWindow is EditRecipient)
            {
                Action<InMemoryDataProvider<Recipient>, ObservableCollection<Recipient>> RefreshMethod;
                
                if (Filter != null && Filter.Length > 0)
                    RefreshMethod = Filtrate;
                else
                    RefreshMethod = Refresh;

                UpgradeCollection(ItemStatus.Edit, recipientProvider, Recipients, SelectedRecipient, RefreshMethod);
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
                Refresh(emailProvider, Emails);

            TabIndex = 1;
        }

        private void OnDeleteEmailCommand()
        {
            UpgradeCollection(ItemStatus.Delete, emailProvider, Emails, SelectedEmail, Refresh);
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
            Action<InMemoryDataProvider<Recipient>, ObservableCollection<Recipient>> RefreshMethod;

            if (Filter != null && Filter.Length > 0)
                RefreshMethod = Filtrate;
            else
                RefreshMethod = Refresh;

            UpgradeCollection(ItemStatus.Delete, recipientProvider, Recipients, SelectedRecipient, RefreshMethod);
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
            UpgradeCollection(ItemStatus.Delete, serverProvider, Servers, SelectedServer, Refresh);
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
            UpgradeCollection(ItemStatus.Delete, senderProvider, Senders, SelectedSender, Refresh);
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