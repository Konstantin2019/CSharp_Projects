using System.Windows;

namespace FireChat.ViewModel.WPFServices
{
    public class WPFDialogService
    {
        public enum DialogButton
        {
            OK = 0, OKCancel = 1, AbortRetryIgnore = 2, YesNoCancel = 3, YesNo = 5, RetryCancel = 5
        }

        public enum DialogImage
        {
            None = 0, Hand = 16, Stop = 16, Error = 16, Question = 32,
            Exclamation = 48, Warning = 48, Information = 64, Asterisk = 64
        }

        public enum DialogResult
        {
            None = 0, OK = 1, Cancel = 2, Yes = 6, No = 7
        }

        public DialogResult ShowInfo(string text, string caption)
            =>
            (DialogResult)MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Information);

        public DialogResult ShowError(string text, string caption)
            =>
            (DialogResult)MessageBox.Show(text, caption, MessageBoxButton.OK, MessageBoxImage.Error);

        public DialogResult ShowMessage(string text, string caption, DialogButton button, DialogImage image) 
            => 
            (DialogResult)MessageBox.Show(text, caption, (MessageBoxButton)button, (MessageBoxImage)image);
    }
}
