using FireBase_lib.Entities;
using FireBase_lib.Services;
using FireChat.ViewModel.WPFServices;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FireChat.ViewModel
{
    /// <summary>
    /// Класс, осуществляющий взаимодествие бекэнда с интерфейсом приложения
    /// </summary>
    public class FireChatViewModel : ViewModelBase
    {
        private string windowTitle = "FireChat";
        private string name;
        private string id;
        private string message;
        private bool regIsEnabled;
        private bool authIsEnabled;

        private MessangerActions fireBaseProvider;
        private WPFDialogService dialogService;
        private WindowsManager windowsManager;

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
        public ICommand WindowClosing { get; }

        public FireChatViewModel(MessangerActions FireBaseProvider, WPFDialogService DialogService, WindowsManager WindowsManager)
        {
            fireBaseProvider = FireBaseProvider;
            dialogService = DialogService;
            windowsManager = WindowsManager;
            CurrentUsers = new ObservableCollection<User>();
            UserMessages = new ObservableCollection<UserMessage>();
            Auth = new RelayCommand(OnAuthExecuted);
            Reg = new RelayCommand(OnRegExecuted);
            Send = new RelayCommand(OnSendExecuted);
            ListenMessages = new RelayCommand(OnListenMessagesExecuted);
            ListenCurrentUsers = new RelayCommand(OnListenCurrentUsersExecuted);
            WindowClosing = new RelayCommand(OnWindowClosingExecuted);
            RegIsEnabled = true;
            AuthIsEnabled = true;
            fireBaseProvider.OnCurrentUsersReceive += OnCurrentUsersReceiveExecuted;
            fireBaseProvider.OnMessagesReceive += OnMessagesReceiveExecuted;
        }

        private async void OnWindowClosingExecuted()
        {
            await fireBaseProvider.RemoveCurrentUserAsync();
            windowsManager.CurrentWindow = null;
            windowsManager.MainWindow.Show();
        }

        private async void OnListenCurrentUsersExecuted()
        {
            await fireBaseProvider.ListenCurrentUsersThread();
        }

        private async void OnListenMessagesExecuted()
        {
            await fireBaseProvider.ListenMessagesThread();
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
            await fireBaseProvider.SendMessageAsync(userMessage);
            message = "";
        }

        private async void OnRegExecuted()
        {
            CheckSession();
            RegIsEnabled = false;
            AuthIsEnabled = false;

            var user = new User { Name = name, Value = id };
            var reg = (RegCodes)await fireBaseProvider.RegisterAsync(user);

            switch (reg)
            {
                case RegCodes.success:
                    dialogService.ShowInfo($"Регистрация прошла успешно!" +
                                           $"Вы вошли в систему как {MessangerActions.CurrentUser.Name}",
                                           "Сообщение");
                    windowsManager.CurrentWindow = new Chat();
                    windowsManager.MainWindow.Hide();
                    windowsManager.CurrentWindow.Show();
                    break;
                case RegCodes.already_exists_error:
                    dialogService.ShowError("Такой пользователь уже существует!", "Сообщение");
                    break;
                case RegCodes.reg_failed_error:
                    dialogService.ShowError("Не удалось пройти регистрацию! " +
                                            "Попробуйте повторить попытку позже!", "Сообщение");
                    break;
                default:
                    break;
            }

            RegIsEnabled = true;
            AuthIsEnabled = true;
        }

        private async void OnAuthExecuted()
        {
            CheckSession();
            RegIsEnabled = false;
            AuthIsEnabled = false;

            var user = new User { Name = name, Value = id };
            var auth = (AuthCodes)await fireBaseProvider.AuthAsync(user);

            switch (auth)
            {
                case AuthCodes.success:
                    dialogService.ShowInfo($"Аутентификация прошла успешно! " +
                                           $"Вы вошли в систему как {MessangerActions.CurrentUser.Name}",
                                           "Сообщение");
                    windowsManager.CurrentWindow = new Chat();
                    windowsManager.MainWindow.Hide();
                    windowsManager.CurrentWindow.Show();
                    break;
                case AuthCodes.not_found_error:
                    dialogService.ShowError("Пользователь не найден!", "Сообщение");
                    break;
                case AuthCodes.auth_failed_error:
                    dialogService.ShowError("Не удалось пройти аутентификацию! " +
                                            "Попробуйте повторить попытку позже!", "Сообщение");
                    break;
                case AuthCodes.user_exists_error:
                    dialogService.ShowError("Такой пользователь уже в сети!", "Сообщение");
                    break;
                default:
                    break;
            }            

            RegIsEnabled = true;
            AuthIsEnabled = true;
        }

        private void CheckSession()
        {
            if (windowsManager.CurrentWindow != null)
            {
                dialogService.ShowError("Текущий сеанс пользователя не закрыт!", "Сообщение");
                return;
            };
        }
    }
}