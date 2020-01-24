using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MailSender_lib.Model;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MailSender.ViewModel
{
    public class MailSenderViewModel : ViewModelBase
    {
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

        public ObservableCollection<Sender> Senders { get; }
        public ObservableCollection<Server> Servers { get; }
        public ObservableCollection<Recipient> Recipients { get; }
        public ObservableCollection<Email> Emails { get; }
        public ObservableCollection<ShedulerTask> ShedulerTasks { get; }

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

        public MailSenderViewModel()
        {
            CreateSenderCommand = new RelayCommand(OnCreateSenderCommand);
            EditRecipientCommand = new RelayCommand(OnEditRecipientCommand);
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
        }

        private void OnCreateEmailCommand()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        private void OnCreateRecipientCommand()
        {
            throw new NotImplementedException();
        }

        private void OnDeleteServerCommand()
        {
            throw new NotImplementedException();
        }

        private void OnEditSeverCommand()
        {
            throw new NotImplementedException();
        }

        private void OnCreateServerCommand()
        {
            throw new NotImplementedException();
        }

        private void OnDeleteSenderCommand()
        {
            throw new NotImplementedException();
        }

        private void OnCreateSenderCommand()
        {
            throw new NotImplementedException();
        }
    }
}