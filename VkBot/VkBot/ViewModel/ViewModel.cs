﻿using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace VkBot.ViewModel
{
    /// <summary>
    /// Класс, выполняющий функции контроллера по взаимодействию пользовательского интерфейса с VkProvider
    /// </summary>
    public class ViewModel
    {
        private static VkService vkService;
        private bool valid;

        const string saving_path = "token.txt";

        public string Token_uri { get; set; }
        public string Message { get; set; }
        public ObservableCollection<string> Responses { get; private set; }

        public ICommand ValidateTokenAsync { get; }
        public ICommand SendMessageAsync { get; }

        public ViewModel()
        {
            vkService = new VkService();
            Responses = new ObservableCollection<string>();
            ValidateTokenAsync = new RelayCommand(OnValidateTokenAsyncExecuted);
            SendMessageAsync = new RelayCommand(OnSendMessageAsyncExecuted);
        }

        public async void OnValidateTokenAsyncExecuted(object obj)
        {
            valid = await vkService.ValidateAsync(saving_path);

            if (!valid)
                await vkService.GetAccessTokenUriAsync();
        }

        public async void OnSendMessageAsyncExecuted(object obj)
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
                else
                    MessageBox.Show("Token_uri пустой или слишком короткий!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
                await SendAction();
        }

        private async Task SendAction()
        {
            if (Message != null && Message.Length > 5)
            {
                var responses = await vkService.SendAsync(Message);
                foreach (var response in responses)
                    Responses.Add(response);
            }
                
            else
                MessageBox.Show("Сообщение пустое или слишком короткое!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
