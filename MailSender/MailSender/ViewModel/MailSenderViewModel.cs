using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MailSender.ViewModel.WPFServices;
using MailSender_lib.Model;
using MailSender_lib.Model.Base;
using MailSender_lib.Services;
using MailSender_lib.Services.InMemory;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MailSender.ViewModel
{
    public class MailSenderViewModel : ViewModelBase
    {
        private InMemorySenderProvider senderProvider;
        private InMemoryServerProvider serverProvider;
        private InMemoryRecipientProvider recipientProvider;
        private InMemoryEmailProvider emailProvider;
        private InMemoryShedulerProvider shedulerProvider;
        private MailSenderService emailService;
        private WindowsService windowsService;

        private string windowTitle = "Рассыльщик почты v1.0";
        private string filter;
        private string emailSubject;
        private string emailBody;
        private bool allRecipients;
        private Sender newSender;
        private Sender selectedSender;
        private Server newServer;
        private Server selectedServer;
        private Recipient newRecipient;
        private Recipient selectedRecipient;
        private Email newEmail;
        private Email selectedEmail;
        private ShedulerTask newShedulerTask;
        private ShedulerTask selectedShedulerTask;
        private string selectedTime;

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

        public bool AllRecipients
        {
            get => allRecipients;
            set => Set(ref allRecipients, value);
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

        public Email NewEmail
        {
            get => newEmail;
            set => Set(ref newEmail, value);
        }

        public Email SelectedEmail
        {
            get => selectedEmail;
            set => Set(ref selectedEmail, value);
        }

        public ShedulerTask NewShedulerTask
        {
            get => newShedulerTask;
            set => Set(ref newShedulerTask, value);
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

        public ObservableCollection<Sender> Senders { get; private set; }
        public ObservableCollection<Server> Servers { get; private set; }
        public ObservableCollection<Recipient> Recipients { get; private set; }
        public ObservableCollection<Email> Emails { get; private set; }
        public ObservableCollection<ShedulerTask> ShedulerTasks { get; private set; }

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
        public ICommand GoToEMailCommand { get; }

        public ICommand PlanCommand { get; }
        public ICommand SendCommand { get; }
        public ICommand CreateEmailCommand { get; }

        public ICommand AcceptCommand { get; }
        public ICommand AbortCommand { get; }
        public ICommand TotalRefreshCommand { get; }

        public MailSenderViewModel(MailSenderService EmailService, WindowsService WindowsService)
        {
            windowsService = WindowsService;
            emailService = EmailService;
            senderProvider = new InMemorySenderProvider("DataBase/Senders.db");
            serverProvider = new InMemoryServerProvider("DataBase/Servers.db");
            recipientProvider = new InMemoryRecipientProvider("DataBase/Recipients.db");
            emailProvider = new InMemoryEmailProvider("DataBase/Emails.db");
            shedulerProvider = new InMemoryShedulerProvider("DataBase/ShedulerTasks.db");

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
            GoToEMailCommand = new RelayCommand(OnGoToEMailCommand);

            PlanCommand = new RelayCommand(OnPlanCommand);
            SendCommand = new RelayCommand(OnSendCommand);
            CreateEmailCommand = new RelayCommand(OnCreateEmailCommand);

            AcceptCommand = new RelayCommand(OnAcceptCommand);
            AbortCommand = new RelayCommand(OnAbortCommand);
            TotalRefreshCommand = new RelayCommand(OnTotalRefreshCommand);
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
                    foreach (var item in items)
                        Senders.Add(item);
                }
            }

            if (collection is ObservableCollection<Server>)
            {
                var success = serverProvider.ReadData();
                if (success)
                {
                    var items = serverProvider.GetAll();
                    foreach (var item in items)
                        Servers.Add(item);
                }
            }

            if (collection is ObservableCollection<Recipient>)
            {
                var success = recipientProvider.ReadData();
                if (success)
                {
                    var items = recipientProvider.GetAll();
                    foreach (var item in items)
                        Recipients.Add(item);
                }
            }
        }

        private void OnTotalRefreshCommand()
        {
            Refresh(Senders);
            Refresh(Servers);
            Refresh(Recipients);
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

        private void OnCreateEmailCommand()
        {
            NewEmail = new Email()
            {
                Subject = EmailBody, Body = EmailBody
            };
            emailProvider.Create(NewEmail);
            emailProvider.SaveChanges();
        }

        private void OnGoToEMailCommand()
        {
            throw new NotImplementedException();
        }

        private void OnSendCommand()
        {
            throw new NotImplementedException();
        }

        private void OnPlanCommand()
        {
            throw new NotImplementedException();
        }

        private void OnGoToShedulerCommand()
        {
            throw new NotImplementedException();
        }

        private void OnDeleteRecipientCommand()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
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