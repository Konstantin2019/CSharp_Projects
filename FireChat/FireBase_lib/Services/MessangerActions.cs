using FireBase_lib.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FireBase_lib.Services
{
    /// <summary>
    /// Класс, реализующий функциональность мессенджера
    /// </summary>
    public class MessangerActions
    {
        private const int maxNumberOfMessages = 100;
        private const string path = "https://messanger-konst.firebaseio.com/";
        /// <summary>
        /// Нода: пользователи
        /// </summary>
        private const string nodeUsers = "Users";
        /// <summary>
        /// Нода: пользователи-онлайн
        /// </summary>
        private const string nodeCurrentUsers = "CurrentUsers";
        /// <summary>
        /// Нода: сообщения
        /// </summary>
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

        /// <summary>
        /// Метод аутентификации
        /// </summary>
        /// <param name="user">пользователь</param>
        /// <returns>код ответа</returns>
        public async Task<int> Auth(User user)
        {
            string getJson = await DataBase.GetAsync(nodeUsers);
            if (getJson == null) return -1; // -1 - не удалось произвести аутентификацию

            var users = UtilityHelper.TryDeserialize<User>(getJson);

            if (users != null)
            {
                foreach (var u in users)
                {
                    if (u.Name.ToLower() == user.Name.ToLower() && u.Value == user.Value)
                    {
                        CurrentUser = u;
                        var checkUser = await DataBase.GetAsync(nodeCurrentUsers);
                        if (checkUser != null)
                        {
                            if (checkUser == "null")
                            {
                                await AddCurentUser(CurrentUser, nodeCurrentUsers);
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
                            await AddCurentUser(CurrentUser, nodeCurrentUsers);
                            return 0; // 0 - аутентификация прошла успешно    
                        }           
                    }
                }
            }
            return 1; // 1 - пользователь не найден
        }

        /// <summary>
        /// Метод регистрации
        /// </summary>
        /// <param name="user">пользователь</param>
        /// <returns>код ответа</returns>
        public async Task<int> Register(User user)
        {
            string getJson = await DataBase.GetAsync(nodeUsers);
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
                bool reg = await DataBase.PostAsync(postJson, nodeUsers);
                if (reg)
                {
                    CurrentUser = user;
                    var json = UtilityHelper.TrySerialize(CurrentUser);
                    if (json != null) await DataBase.PostAsync(json, nodeCurrentUsers);
                    return 0; // 0 - регистрация прошла успешно          
                }
                else return -1; // -1 - не удалось произвести регистрацию
            }

            return -1;
        }

        /// <summary>
        /// Метод отправки сообщений
        /// </summary>
        /// <param name="message">сообщение</param>
        /// <returns>успешность процедуры в булевом выражении</returns>
        public async Task<bool> SendMessage(UserMessage message)
        {
            var success = false;
            var postJson = UtilityHelper.TrySerialize(message);

            if (postJson != null)
                success = await DataBase.PostAsync(postJson, nodeMessages);

            return success;
        }

        /// <summary>
        /// Метод удаления текущего пользователя
        /// </summary>
        /// <returns>текущая задача</returns>
        public async Task<bool> RemoveCurrentUser()
        {
            string childNode = null;
            bool success = false;

            string getJson = await DataBase.GetAsync(nodeCurrentUsers);

            if (getJson != null)
                childNode = UtilityHelper.GetKey(getJson, CurrentUser);

            if (childNode != null)
            {
                var nodePath = nodeCurrentUsers + "/" + childNode;
                success = await DataBase.DeleteAsync(nodePath);
            }

            return success;
        }

        /// <summary>
        /// Метод добавления текущего пользователя
        /// </summary>
        /// <param name="user">пользователь</param>
        /// <param name="node">нода</param>
        /// <returns>текущая задача</returns>
        private async Task AddCurentUser(User user, string node)
        {
            var json = UtilityHelper.TrySerialize(user);
            if (json != null) await DataBase.PostAsync(json, node);
        }

        /// <summary>
        /// Реализация потока слушателя текущих пользователей с использованием событий
        /// </summary>
        /// <returns>текущая задача</returns>
        public async Task ListenCurrentUsersThread() 
        {
            while (true)
            {
                string getJson = await DataBase.GetAsync(nodeCurrentUsers);
                if (getJson.Length != jsonCurrentUsersLength)
                {
                    jsonCurrentUsersLength = getJson.Length;
                    var currentUsers = UtilityHelper.TryDeserialize<User>(getJson);
                    if (currentUsers != null)
                        OnCurrentUsersReceive?.Invoke(currentUsers);
                }
            }
        }

        /// <summary>
        /// Реализация потока слушателя сообщений с использованием событий
        /// </summary>
        /// <returns>текущая задача</returns>
        public async Task ListenMessagesThread()
        {
            while (true)
            {
                string getJson = await DataBase.GetAsync(nodeMessages);
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
                            await DataBase.DeleteAsync(nodeMessages);
                    }
                }
            }
        }
    }
}
