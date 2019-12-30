using System.Windows;

namespace FireChat.ViewModel.WPFServices
{
    public class WindowsManager
    {
        public Window MainWindow { get; set; }
        public Window CurrentWindow { get; set; }

        public WindowsManager()
        {
            MainWindow = Application.Current.MainWindow;
            CurrentWindow = null;
        }
    }
}
