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
        private Dictionary<int, CancellationTokenSource> tokenSourceDict;
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
            set => Set(ref filter, value);
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
        public ICommand ContinueTaskCommand { get; }
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
            tokenSourceDict = new Dictionary<int, CancellationTokenSource>();

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
            ContinueTaskCommand = new RelayCommand(OnContinueTaskCommand);
        }

        private ShedulerTask CreateShedulerTask(Recipient recipient, out TimeSpan delay)
        {
            delay = new TimeSpan();

            var shedulerTask = new ShedulerTask()
            {
                Sender = SelectedSender,
                Recipient = recipient,
                Email = SelectedEmail,
            };

            if (shedulerTask.Sender == null || shedulerTask.Recipient == null || shedulerTask.Email == null)
                return null;

            DateTime.TryParse(SelectedTime, out DateTime time);

            if (time != null)
                shedulerTask.Time = SelectedDate.AddHours(time.Hour);

            if (shedulerTask.Time != null && shedulerTask.Time > DateTime.Now)
            {
                delay = shedulerTask.Time - DateTime.Now;
                shedulerTask.Status = "Отправка запланирована на " + shedulerTask.Time.ToLongDateString() + " в " + SelectedTime;
            }
            else
            {
                shedulerTask.Time = DateTime.Now;
                delay = shedulerTask.Time - DateTime.Now;
                shedulerTask.Status = "Немедленная отправка...";
            }

            shedulerProvider.Create(shedulerTask);
            var success = shedulerProvider.SaveChanges();
            if (success)
                Refresh(ShedulerTasks);

            return shedulerTask;
        }

        private async Task SendAsync(ShedulerTask shedulerTask, TimeSpan delay)
        {
            if (shedulerTask == null) return;

            await Task.Delay(delay);
            var task = await emailService.SendAsync(shedulerTask.Sender, 
                                                    shedulerTask.Recipient, 
                                                    shedulerTask.Email);
            if (task.Success)
            {
                shedulerTask.Status = "Успешно отправлено!";
                shedulerProvider.Edit(shedulerTask.Id, shedulerTask);
                var success = shedulerProvider.SaveChanges();
                if (success)
                    Refresh(ShedulerTasks);
            }
            else
            {
                shedulerTask.Status = task.Error;
                shedulerProvider.Edit(shedulerTask.Id, shedulerTask);
                var success = shedulerProvider.SaveChanges();
                if (success)
                    Refresh(ShedulerTasks);
            }
        }

        private async Task SendAsync(IEnumerable<ShedulerTask> shedulerTasks, TimeSpan delay)
        {
            if (shedulerTasks == null) return;

            var sender = shedulerTasks.Select(t => t.Sender).FirstOrDefault();
            var email = shedulerTasks.Select(t => t.Email).FirstOrDefault();
            var recipients = shedulerTasks.Select(t => t.Recipient).ToArray();
            var tasks = shedulerTasks.ToArray();

            if (sender == null || recipients == null || email == null ) return;

            await Task.Delay(delay);
            var responses = emailService.SendAsync(sender, recipients, email);

            var idx = 0;
            foreach (var response in responses)
            {
                var inner_idx = idx;
                if (response.Success)
                {
                    tasks[inner_idx].Status = "Успешно отправлено!";
                    shedulerProvider.Edit(tasks[inner_idx].Id, tasks[inner_idx]);
                    var success = shedulerProvider.SaveChanges();
                    if (success)
                        Refresh(ShedulerTasks);
                }
                else
                {
                    tasks[inner_idx].Status = response.Error;
                    shedulerProvider.Edit(tasks[inner_idx].Id, tasks[inner_idx]);
                    var success = shedulerProvider.SaveChanges();
                    if (success)
                        Refresh(ShedulerTasks);
                }
                idx++;
            }
        }

        private void Refresh<T>(ObservableCollection<T> collection) where T : BaseEntity
        {
            if (collection != null)
                collection.Clear();

            if (collection is ObservableCollection<Sender>)
            {
                var success = senderProvider.ReadData();
                if (success)
                {
                    var items = senderProvider.GetAll();
                    items.ToObservableCollection(Senders);
                }
            }

            if (collection is ObservableCollection<Server>)
            {
                var success = serverProvider.ReadData();
                if (success)
                {
                    var items = serverProvider.GetAll();
                    items.ToObservableCollection(Servers);
                }
            }

            if (collection is ObservableCollection<Recipient>)
            {
                var success = recipientProvider.ReadData();
                if (success)
                {
                    var items = recipientProvider.GetAll();
                    items.ToObservableCollection(Recipients);
                }
            }

            if (collection is ObservableCollection<Email>)
            {
                var success = emailProvider.ReadData();
                if (success)
                {
                    var items = emailProvider.GetAll();
                    items.ToObservableCollection(Emails);
                }
            }

            if (collection is ObservableCollection<ShedulerTask>)
            {
                var success = shedulerProvider.ReadData();
                if (success)
                {
                    var items = shedulerProvider.GetAll();
                    items.ToObservableCollection(ShedulerTasks);
                }
            }
        }

        private void OnContinueTaskCommand()
        {
            if (ShedulerTasks != null)
            {
                var tasks = new List<Task>();
                foreach (var shedulerTask in ShedulerTasks)
                {
                    var abort = new CancellationTokenSource();
                    tokenSourceDict.Add(shedulerTask.Id, abort);
                    var task = new Task(async () =>
                    {
                        var delay = shedulerTask.Time - DateTime.Now;
                        if (delay.TotalMinutes < 0)
                            delay = TimeSpan.FromMinutes(0);
                        await SendAsync(shedulerTask, delay);
                    }, abort.Token);
                    tasks.Add(task);
                }
                var options = new ParallelOptions()
                {
                    MaxDegreeOfParallelism = 8
                };
                Parallel.ForEach(tasks, options, task =>
                {
                    task.Start();
                });
            }
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
                    Refresh(Recipients);
            }

            if (WindowsService.InputDataWindow is EditRecipient)
            {
                recipientProvider.Edit(SelectedRecipient.Id, SelectedRecipient);
                var success = recipientProvider.SaveChanges();
                if (success)
                    Refresh(Recipients);
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
            emailProvider.Delete(SelectedEmail.Id);
            var success = emailProvider.SaveChanges();
            if (success)
                Refresh(Emails);
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
            if (tokenSourceDict != null)
            {
                var abort = tokenSourceDict[SelectedShedulerTask.Id];
                abort.Cancel();
            }
            shedulerProvider.Delete(SelectedShedulerTask.Id);
            var success = shedulerProvider.SaveChanges();
            if (success)
                Refresh(ShedulerTasks);
        }

        private async void OnPlanSendCommand()
        {
            if (!AllRecipients)
            {
                if (SelectedRecipient == null) return;

                var shedulerTask = CreateShedulerTask(SelectedRecipient, out TimeSpan delay);
                await SendAsync(shedulerTask, delay);
            }
            else
            {
                if (Recipients == null) return;

                var shedulerTasks = new List<ShedulerTask>();
                var delay = new TimeSpan();

                foreach (var recip in Recipients)
                {
                    var task = CreateShedulerTask(recip, out delay);
                    shedulerTasks.Add(task);
                }

                await SendAsync(shedulerTasks, delay);
            }
        }

        private void OnDeleteRecipientCommand()
        {
            recipientProvider.Delete(SelectedRecipient.Id);
            var success = recipientProvider.SaveChanges();
            if (success)
                Refresh(Recipients);
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
            serverProvider.Delete(SelectedServer.Id);
            var success = serverProvider.SaveChanges();
            if (success)
                Refresh(Servers);
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
            senderProvider.Delete(SelectedSender.Id);
            var success = senderProvider.SaveChanges();
            if (success)
                Refresh(Senders);
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
    }
}