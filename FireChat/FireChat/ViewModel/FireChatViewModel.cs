using FireBase_lib.Entities;
using FireBase_lib.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace FireChat.ViewModel
{
    public class FireChatViewModel : ViewModelBase
    {
        private string windowTitle = "FireChat";
        private string name;
        private string id;
        private string message;
        private bool regIsEnabled;
        private bool authIsEnabled;

        private MessangerActions provider;
        private static Chat chat;

        enum AuthCodes
        {
            success = 0, not_found_error = 1, auth_failed_error = -1, user_exists_error = -2 
        }

        enum RegCodes
        {
            success = 0, already_exists_error = 1, reg_failed_error = -1
        }

        public string Title
        {
            get => windowTitle;
            set => Set(ref windowTitle, value);
        }

        public string Name
        {
            get => name;
            set => Set(ref name, value);
        }

        public string Id
        {
            get => id;
            set => Set(ref id, value);
        }

        public string Message
        {
            get => message;
            set => Set(ref message, value);
        }

        public bool RegIsEnabled
        {
            get => regIsEnabled;
            set => Set(ref regIsEnabled, value);
        }

        public bool AuthIsEnabled
        {
            get => authIsEnabled;
            set => Set(ref authIsEnabled, value);
        }

        public ObservableCollection<User> CurrentUsers { get; private set; }          
        public ObservableCollection<UserMessage> UserMessages { get; private set; } 

        public ICommand Auth { get; }
        public ICommand Reg { get; }
        public ICommand Send { get; }
        public ICommand ListenMessages { get; }
        public ICommand ListenCurrentUsers { get; }

        public FireChatViewModel(MessangerActions Provider)
        {
            provider = Provider;
            CurrentUsers = new ObservableCollection<User>();
            UserMessages = new ObservableCollection<UserMessage>();
            Auth = new RelayCommand(OnAuthExecuted);
            Reg = new RelayCommand(OnRegExecuted);
            Send = new RelayCommand(OnSendExecuted);
            ListenMessages = new RelayCommand(OnListenMessagesExecuted);
            ListenCurrentUsers = new RelayCommand(OnListenCurrentUsersExecuted);

            provider.OnCurrentUsersReceive += OnCurrentUsersReceiveExecuted;
            provider.OnMessagesReceive += OnMessagesReceiveExecuted;
        }

        private async void OnListenCurrentUsersExecuted()
        {
            await provider.ListenCurrentUsersThread();
        }

        private async void OnListenMessagesExecuted()
        {
            await provider.ListenMessagesThread();
        }

        private void OnMessagesReceiveExecuted(ICollection<UserMessage> items)
        {
            if (items != null)
                UserMessages.AddRange(items);
        }

        private void OnCurrentUsersReceiveExecuted(ICollection<User> items)
        {
            CurrentUsers.Clear();
            if (items != null)
                CurrentUsers.AddRange(items);
        }

        private async void OnSendExecuted()
        {
            var timeStamp = DateTime.Now.ToShortTimeString();
            var userMessage = new UserMessage { Name = message,
                                                Value = timeStamp,
                                                UserName = MessangerActions.CurrentUser.Name };
            await provider.SendMessage(userMessage);
        }

        private async void OnRegExecuted()
        {
            CheckSession();
            regIsEnabled = false;
            authIsEnabled = false;

            var user = new User { Name = name, Value = id };
            var reg = (RegCodes)await provider.Register(user);

            switch (reg)
            {
                case RegCodes.success:
                    MessageBox.Show($"����������� ������ �������!" +
                                    $"�� ����� � ������� ��� {MessangerActions.CurrentUser.Name}", 
                                    "���������", MessageBoxButton.OK, MessageBoxImage.Information);
                    chat = new Chat();
                    chat.Show();
                    break;
                case RegCodes.already_exists_error:
                    MessageBox.Show("����� ������������ ��� ����������!", 
                                    "���������", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case RegCodes.reg_failed_error:
                    MessageBox.Show("�� ������� ������ �����������! ���������� ��������� ������� �����!", 
                                    "���������", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    break;
            }

            regIsEnabled = true;
            authIsEnabled = true;
        }

        private async void OnAuthExecuted()
        {
            CheckSession();
            regIsEnabled = false;
            authIsEnabled = false;

            var user = new User { Name = name, Value = id };
            var auth = (AuthCodes)await provider.Auth(user);

            switch (auth)
            {
                case AuthCodes.success:
                    MessageBox.Show($"�������������� ������ �������! " +
                                    $"�� ����� � ������� ��� {MessangerActions.CurrentUser.Name}",
                                    "���������", MessageBoxButton.OK, MessageBoxImage.Information);
                    chat = new Chat();
                    chat.Show();
                    break;
                case AuthCodes.not_found_error:
                    MessageBox.Show("������������ �� ������!", 
                                    "���������", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case AuthCodes.auth_failed_error:
                    MessageBox.Show("�� ������� ������ ��������������! ���������� ��������� ������� �����!", 
                                    "���������", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                case AuthCodes.user_exists_error:
                    MessageBox.Show("����� ������������ ��� � ����!", 
                                    "���������", MessageBoxButton.OK, MessageBoxImage.Error);
                    break;
                default:
                    break;
            }            

            regIsEnabled = true;
            authIsEnabled = true;
        }

        private void CheckSession()
        {
            if (chat != null)
            {
                MessageBox.Show("������� ����� ������������ �� ������!", 
                                "���������", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            };
        }
    }
}