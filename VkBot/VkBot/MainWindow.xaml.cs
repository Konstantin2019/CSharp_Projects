using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;


namespace VkBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VkService vkService;
        ICollection<string> responses;
        bool valid;

        public MainWindow()
        {
            InitializeComponent();

            vkService = new VkService();
            responses = new ObservableCollection<string>();

            Response_view.ItemsSource = responses;
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            valid = await vkService.ValidateAsync("token.txt");

            if (!valid)
                await vkService.GetAccessTokenUriAsync();
        }

        private async void Send_btn_Click(object sender, RoutedEventArgs e)
        {
            if (!valid)
            {
                var token_uri = token_tb.Text;

                if (token_uri != null && token_uri.Length > 100)
                {
                    
                    var token = await vkService.GetTokenAsync(token_uri);
                    if (token.Error != null)
                        MessageBox.Show(token.Error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    else
                    {
                        await vkService.SaveAsync("token.txt", token.Value);
                        var users = await vkService.GetUsersAsync();

                        if (users.Error != null)
                            MessageBox.Show(users.Error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        else
                        {
                            var message = Message_box.Text;

                            if (message != null && message.Length > 5)
                                responses = await vkService.SendAsync(message);
                            else
                                MessageBox.Show("Сообщение пустое или слишком короткое!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                var users = await vkService.GetUsersAsync();

                if (users.Error != null)
                    MessageBox.Show(users.Error, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                else
                {
                    var message = Message_box.Text;

                    if (message != null && message.Length > 5)
                        responses = await vkService.SendAsync(message);
                    else
                        MessageBox.Show("Сообщение пустое или слишком короткое!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
