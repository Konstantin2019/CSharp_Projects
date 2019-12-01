using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace VkBot
{
    /// <summary>
    /// Класс для работы c API VK. 
    /// Позволяет получать токен группы, сохранять его на диск, считывать с диска, проверять валидность,
    /// получать целевых членов группы и отправлять им сообщения.
    /// </summary>
    public class VkService
    {
        private static string access_token;

        const int client_id = 5475341;
        const int group_id = 127939533;
        const int host_id = 26995904;
        const string api_version = "5.102";
        const string base_uri = "https://oauth.vk.com/authorize?";
        const string redirect_uri = "https://oauth.vk.com/blank.html";
        const string method = "https://api.vk.com/method/";

        /// <summary>
        /// Перечисление прав доступа
        /// </summary>
        enum Scope
        {
            friends, photos, audio, video, messages
        }

        /// <summary>
        /// Асинхронный метод по получению строки с содержанием токена доступа группы
        /// </summary>
        #region GetAccessTokenUriAsync
        public async Task GetAccessTokenUriAsync()
        {
            var auth_request = base_uri
                               + $"client_id={client_id}&"
                               + $"group_ids={group_id}&"
                               + "display=page&"
                               + redirect_uri
                               + "&"
                               + $"scope={Scope.messages}&"
                               + "response_type=token&"
                               + $"v={api_version}&"
                               + "state=0";
            await Task.Run(() => { Process.Start(auth_request); });
        }
        #endregion

        /// <summary>
        /// Асинхронный метод по получению словаря с содержанием токена группы, времени его валидности, 
        /// снэпшота текущей даты и времени, локального ip
        /// </summary>
        /// <param name="uri">строка с содержанием токена доступа группы</param>
        /// <param name="saving_path">путь для сохранения словаря</param>
        /// <returns>объект класса Response, содержащий словарь и строку с Exception.Message</returns>
        #region GetAccessTokenAsync
        public async Task<Response<Dictionary<string, string>>> GetTokenAsync(string uri, string saving_path)
        {
            var token_info = new Dictionary<string, string>();

            try
            {
                await Task.Run(() => 
                {
                    var split = uri.Split('&', '#');

                    foreach (var i in split)
                    {
                        if (i.StartsWith("access_token") || i.StartsWith("expires_in"))
                        {
                            var subsplit = i.Split('=');
                            token_info.Add(subsplit[0], subsplit[1]);
                        }
                    }

                    token_info.Add("snapshot_time", DateTime.Now.ToString());
                    var ip = Utilities.GetLocalIP();
                    token_info.Add("ip", ip.ToString());

                    access_token = token_info["access_token" + $"_{group_id}"];
                });

                await SaveAsync(saving_path, token_info);
                return new Response<Dictionary<string, string>> { Value = token_info, Error = null };
            }
            catch (Exception error)
            {
                return new Response<Dictionary<string, string>> { Value = null, Error = error.Message };
            }
        }
        #endregion

        /// <summary>
        /// Асинхронный метод по сохранению словаря на диск
        /// </summary>
        /// <param name="token_path">путь к файлу на диске</param>
        /// <param name="token_info">словарь</param>
        /// <returns>true или false</returns>
        #region SaveOnDiskAsync
        private async Task<bool> SaveAsync(string token_path, Dictionary<string, string> token_info)
        {
            using (StreamWriter wr = new StreamWriter(token_path))
            {
                try
                {
                    foreach (var key_value in token_info)
                        await wr.WriteLineAsync(key_value.Key + "=" + key_value.Value);
                }
                catch (Exception)
                {
                    return false;
                }
                finally
                {
                    wr.Close();
                }
            }
            return true;
        }
        #endregion

        /// <summary>
        /// Асинхронный метод по чтению словаря из файла
        /// </summary>
        /// <param name="token_path">путь к файлу на диске</param>
        /// <returns>словарь с данными токена</returns>
        #region ReadFromDiskAsync
        private async Task<Dictionary<string, string>> ReadAsync(string token_path)
        {
            if (!File.Exists(token_path)) return null;

            var token_info = new Dictionary<string, string>();

            using (StreamReader r = new StreamReader(token_path))
            {
                try
                {
                    while (!r.EndOfStream)
                    {
                        var str = await r.ReadLineAsync();
                        var split = str.Split('=');
                        var keyValue = new KeyValuePair<string, string>(split[0], split[1]);
                        token_info.Add(keyValue.Key, keyValue.Value);
                    }
                }
                catch (Exception)
                {
                    return null;
                }
                finally
                {
                    r.Close();
                }
            }
            return token_info;
        }
        #endregion

        /// <summary>
        /// Асинхронный метод по валидации токена группы, записанного в файл на диск
        /// </summary>
        /// <param name="token_path">путь к файлу на диске</param>
        /// <returns>true или false</returns>
        #region ValidateAccessTokenAsync
        public async Task<bool> ValidateAsync(string token_path)
        {
            var token_info = await ReadAsync(token_path);
            if (token_info != null)
            {
                int.TryParse(token_info["expires_in"], out int expires_in);
                DateTime.TryParse(token_info["snapshot_time"], out DateTime snapshot_time);
                IPAddress.TryParse(token_info["ip"], out IPAddress ip);
                var current_time = snapshot_time.AddSeconds(expires_in);
                var local_ip = Utilities.GetLocalIP();
                if (current_time > DateTime.Now && local_ip.Equals(ip))
                {
                    access_token = token_info["access_token" + $"_{group_id}"];
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }
        #endregion

        /// <summary>
        /// Асинхронный метод по получению словаря членов группы с исключением администрации
        /// </summary>
        /// <returns>объект класса Response, содержащий словарь(id пользователя, имя пользователя) 
        /// и строку с Exception.Message</returns>
        #region GetTargetMembersAsync
        private async Task<Response<Dictionary<int, string>>> GetUsersAsync()
        {
            var get_managers_request = method
                                       + "groups.getMembers"
                                       + "?"
                                       + $"group_id={group_id}&"
                                       + $"v={api_version}&"
                                       + "fields=contacts&"
                                       + "filter=managers&"
                                       + $"access_token={access_token}";

            var get_members_request = method
                                      + "groups.getMembers"
                                      + "?"
                                      + $"group_id={group_id}&"
                                      + $"v={api_version}&"
                                      + "fields=contacts&"
                                      + $"access_token={access_token}";

            try
            {
                var json_managers = await Utilities.GetJObjectAsync(get_managers_request);

                if (json_managers.Value.ContainsKey("error"))
                {
                    var error = (string)json_managers.Value["error"]["error_msg"];
                    return new Response<Dictionary<int, string>> { Value = null, Error = error };
                }

                var count = (int)json_managers.Value["response"]["count"];
                var managers = new Dictionary<int, string>();
                for (int i = 0; i < count; i++)
                {
                    var id = (int)json_managers.Value["response"]["items"][i]["id"];
                    var fist_name = (string)json_managers.Value["response"]["items"][i]["first_name"];
                    managers.Add(id, fist_name);
                }

                var json_members = await Utilities.GetJObjectAsync(get_members_request);

                count = (int)json_members.Value["response"]["count"];
                var members = new Dictionary<int, string>();
                for (int i = 0; i < count; i++)
                {
                    var id = (int)json_members.Value["response"]["items"][i]["id"];
                    var fist_name = (string)json_members.Value["response"]["items"][i]["first_name"];
                    members.Add(id, fist_name);
                }

                var users = members.Except(managers).ToDictionary(i => i.Key, i => i.Value);

                return new Response<Dictionary<int, string>> { Value = users, Error = null };
            }
            catch (Exception error)
            {
                return new Response<Dictionary<int, string>> { Value = null, Error = error.Message };
            }
        }
        #endregion

        /// <summary>
        /// Асинхронный метод по рассылке сообщений внтури сообщества
        /// </summary>
        /// <param name="message">текст сообщения членам группы</param>
        /// <returns>коллекция ответов в json формате</returns>
        #region SendMessagesAsync
        public async Task<ICollection<string>> SendAsync(string message)
        {
            Dictionary<int, string> users = new Dictionary<int, string>();

            var get_response = await GetUsersAsync();
            if (get_response.Error != null)
                return new List<string>() { get_response.Error };
            else
                users = get_response.Value;

            var send_message_request = method
                                       + "messages.send"
                                       + "?"; 
            var content = new Dictionary<string, string>()
            {
                { "user_id", ""},
                { "random_id", ""},
                { "peer_id",  group_id.ToString()},
                { "message", "" },
                { "v", api_version },
                { "access_token", access_token}
            };

            ICollection<string> post_responses = new List<string>();
            var rnd = new Random();

            foreach (var user in users)
            {
                content["random_id"] = rnd.Next(int.MaxValue).ToString();
                content["user_id"] = user.Key.ToString();
                content["message"] = user.Value + ", " + message;
                var post_response = await Utilities.PostAsync(send_message_request, content);
                post_responses.Add(post_response);
            }

            return post_responses;
        }
        #endregion
    }
}
