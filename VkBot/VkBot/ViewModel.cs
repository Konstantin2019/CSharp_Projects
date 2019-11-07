using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace VkBot
{
    public class ViewModel
    {
        private static VkService vkService = new VkService();

        private bool valid;
        private string saving_path = "token.txt";

        public string Token_uri { get; set; }
        public string Message { get; set; }

        public ICollection<string> Responses { get; set; } = new ObservableCollection<string>();

        public async void ValidateTokenAsync(object sender, RoutedEventArgs e)
        {
            valid = await vkService.ValidateAsync("token.txt");

            if (!valid)
                await vkService.GetAccessTokenUriAsync();
        }

        public async void SendMessageAsync(object sender, RoutedEventArgs e)
        {
            if (!valid)
            {
                if (Token_uri != null && Token_uri.Length > 100)
                {
                    var token = await vkService.GetTokenAsync(Token_uri, saving_path);
                    if (token.Error != null)
                        MessageBox.Show(token.Error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                        await SendAction();
                }
            }
            else
                await SendAction();
        }

        private async Task SendAction()
        {
            if (Message != null && Message.Length > 5)
                Responses = await vkService.SendAsync(Message);
            else
                MessageBox.Show("Сообщение пустое или слишком короткое!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
