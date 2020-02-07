using System.Windows;

namespace MailSender.ViewModel.WPFServices
{
    public class WindowsService
    {
        public static Window MainWindow { get; private set; }
        public static Window InputDataWindow { get; private set; }

        public WindowsService()
        {
            MainWindow = Application.Current.MainWindow;
        }

        public T CreateWindow<T>() where T : Window, new()
        {
            var window = new T();
            InputDataWindow = window;
            return window;
        }
    }
}
