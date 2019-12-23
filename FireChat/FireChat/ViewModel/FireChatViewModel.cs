using FireBase_lib.Entities;
using FireBase_lib.Services;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace FireChat.ViewModel
{
    public class FireChatViewModel : ViewModelBase
    {
        private string windowTitle = "FireChat";
        private MessangerActions provider;
        private string name;
        private string id;

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

        public ObservableCollection<User> CurrentUsers { get; private set; }          
        public ObservableCollection<UserMessage> UserMessages { get; private set; } 

        public ICommand Auth { get; }
        public ICommand Reg { get; }
        public ICommand Send { get; }

        public FireChatViewModel(MessangerActions Provider)
        {
            provider = Provider;
            CurrentUsers = new ObservableCollection<User>();
            UserMessages = new ObservableCollection<UserMessage>();
            Auth = new RelayCommand(OnAuthExecuted);
            Reg = new RelayCommand(OnRegExecuted);
            Send = new RelayCommand(OnSendExecuted);

            provider.OnCurrentUsersReceive += OnCurrentUsersReceiveExecuted;
            provider.OnMessagesReceive += OnMessagesReceiveExecuted;
        }

        private void OnMessagesReceiveExecuted(ICollection<UserMessage> items)
        {
            throw new NotImplementedException();
        }

        private void OnCurrentUsersReceiveExecuted(ICollection<User> items)
        {
            throw new NotImplementedException();
        }

        private void OnSendExecuted()
        {
            throw new NotImplementedException();
        }

        private void OnRegExecuted()
        {
            throw new NotImplementedException();
        }

        private void OnAuthExecuted()
        {
            throw new NotImplementedException();
        }
    }
}