using FireBase_lib.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FireBase_lib.Services
{
    public class MessangerActions
    {
        private const int maxNumberOfMessages = 100;
        private const string path = "https://messanger-konst.firebaseio.com/";
        private const string nodeUsers = "Users";
        private const string currentUsersNode = "CurrentUsers";
        private const string nodeMessages = "Messages";

        private int jsonMessagesLength, jsonCurrentUsersLength;
        private int messagesCount;

        private FireBaseRequests DataBase { get; }
        public static User CurrentUser { get; set; }

        public event Action<ICollection<User>> OnCurrentUsersReceive;
        public event Action<ICollection<UserMessage>> OnMessagesReceive;

        public MessangerActions()
        {
            DataBase = new FireBaseRequests(path);
        }

        public async Task<int> Auth(User user)
        {
            string getJson = await DataBase.Get(nodeUsers);
            if (getJson == null) return -1; // -1 - не удалось произвести аутентификацию

            var users = UtilityHelper.TryDeserialize<User>(getJson);

            if (users != null)
            {
                foreach (var u in users)
                {
                    if (u.Name.ToLower() == user.Name.ToLower() && u.Value == user.Value)
                    {
                        CurrentUser = u;
                        var checkUser = await DataBase.Get(currentUsersNode);
                        if (checkUser != null)
                        {
                            if (checkUser == "null")
                            {
                                await AddCurentUser(CurrentUser, currentUsersNode);
                                return 0; // 0 - аутентификация прошла успешно    
                            }
                            var currentUsers = UtilityHelper.TryDeserialize<User>(checkUser);
                            foreach (var cu in currentUsers)
                            {
                                if (cu.Name == CurrentUser.Name && cu.Value == CurrentUser.Value)
                                {
                                    return -2; // -2 - такой пользователь уже в сети                               
                                }
                            }
                            await AddCurentUser(CurrentUser, currentUsersNode);
                            return 0; // 0 - аутентификация прошла успешно    
                        }           
                    }
                }
            }
            return 1; // 1 - пользователь не найден
        }

        public async Task<int> Register(User user)
        {
            string getJson = await DataBase.Get(nodeUsers);
            if (getJson == null) return -1; // -1 - не удалось произвести регистрацию

            var users = UtilityHelper.TryDeserialize<User>(getJson);

            if (users != null)
            {
                foreach (var u in users)
                    if (u.Name.ToLower() == user.Name.ToLower() && u.Value == user.Value)
                        return 1; // 1 - такой пользователь уже существует
            }

            CurrentUser = null;

            var postJson = UtilityHelper.TrySerialize(user);

            if (postJson != null)
            {
                bool reg = await DataBase.Post(postJson, nodeUsers);
                if (reg)
                {
                    CurrentUser = user;
                    var json = UtilityHelper.TrySerialize(CurrentUser);
                    if (json != null) await DataBase.Post(json, currentUsersNode);
                    return 0; // 0 - регистрация прошла успешно          
                }
                else return -1; // -1 - не удалось произвести регистрацию
            }

            return -1;
        }

        public async Task<bool> SendMessage(UserMessage message)
        {
            var success = false;
            var postJson = UtilityHelper.TrySerialize(message);

            if (postJson != null)
                success = await DataBase.Post(postJson, nodeMessages);

            return success;
        }

        //реализация потока слушателя текущих пользователей с использованием событий
        public async Task ListenCurrentUsersThread() 
        {
            while (true)
            {
                string getJson = await DataBase.Get(currentUsersNode);
                if (getJson.Length != jsonCurrentUsersLength)
                {
                    jsonCurrentUsersLength = getJson.Length;
                    var currentUsers = UtilityHelper.TryDeserialize<User>(getJson);
                    if (currentUsers != null)
                        OnCurrentUsersReceive?.Invoke(currentUsers);
                }
            }
        }

        //реализация потока слушателя сообщений с использованием событий
        public async Task ListenMessagesThread()
        {
            while (true)
            {
                string getJson = await DataBase.Get(nodeMessages);
                if (getJson.Length > jsonMessagesLength)
                {
                    jsonMessagesLength = getJson.Length;
                    var messages = UtilityHelper.TryDeserialize<UserMessage>(getJson);

                    if (messages != null)
                    {
                        if (messages.Count > messagesCount && messages.Count <= maxNumberOfMessages)
                        {
                            var newMessages = new List<UserMessage>();
                            for (int i = messagesCount; i < messages.Count; i++)
                                newMessages.Add(messages[i]);

                            messagesCount = messages.Count;
                            OnMessagesReceive?.Invoke(newMessages);
                        }

                        if (messages.Count > maxNumberOfMessages)
                            await DataBase.Delete(nodeMessages);
                    }
                }
            }
        }

        public async Task RemoveCurrentUser()
        {
            string childNode = null;

            string getJson = await DataBase.Get(currentUsersNode);

            if (getJson != null)
                childNode = UtilityHelper.GetKey(getJson, CurrentUser);

            if (childNode != null)
            {
                var nodePath = currentUsersNode + "/" + childNode;
                await DataBase.Delete(nodePath);
            }
        }

        private async Task AddCurentUser(User user, string node)
        {
            var json = UtilityHelper.TrySerialize(user);
            if (json != null) await DataBase.Post(json, node);
        }
    }
}
