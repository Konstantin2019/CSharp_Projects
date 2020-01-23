using GalaSoft.MvvmLight;
using MailSender_lib.Model;
using System.Collections.ObjectModel;

namespace MailSender.ViewModel
{
    public class MailSenderViewModel : ViewModelBase
    {
        private string windowTitle = "Рассыльщик почты v1.0";
        private string filter;

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

        public ObservableCollection<Sender> Senders { get; }
        public ObservableCollection<Server> Servers { get; }
        public ObservableCollection<Recipient> Recipients { get; }
        public ObservableCollection<Email> Emails { get; }
        public ObservableCollection<ShedulerTask> ShedulerTasks { get; }

        public MailSenderViewModel()
        {

        }
    }
}