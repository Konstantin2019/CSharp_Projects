using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FireBase_lib.Services
{
    /// <summary>
    /// Класс запросов к FireBase
    /// </summary>
    public class FireBaseRequests
    {
        private const string JSON_SUFFIX = ".json";

        /// <summary>
        /// Базовый URL
        /// </summary>
        private string BaseUrl { get; }

        public FireBaseRequests(string baseurl)
        {
            BaseUrl = baseurl.EndsWith("/")? baseurl : baseurl + "/";
        }

        /// <summary>
        /// Полный URL
        /// </summary>
        /// <param name="nodepath">относительный ноды путь</param>
        /// <returns>полный url</returns>
        private string FullPath(string nodepath) => BaseUrl + nodepath.Trim('/') + JSON_SUFFIX;

        /// <summary>
        /// Метод, реализующий Post-запросы
        /// </summary>
        /// <param name="json">передаваемый контент в формате json</param>
        /// <param name="nodepath">относительный ноды путь</param>
        /// <returns>успешность процедуры в булевом выражении</returns>
        public async Task<bool> PostAsync(string json, string nodepath)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var path = FullPath(nodepath);
                    var content = new StringContent(json);
                    var request = await client.PostAsync(path, content);
                    request.EnsureSuccessStatusCode();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Метод, реализующий Get-запросы
        /// </summary>
        /// <param name="nodepath">относительный ноды путь</param>
        /// <returns>ответ в формате json</returns>
        public async Task<string> GetAsync(string nodepath)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var path = FullPath(nodepath);
                    var response = await client.GetAsync(path);
                    response.EnsureSuccessStatusCode();
                    var json = await response.Content.ReadAsStringAsync();
                    return json;
                }
                catch (Exception)
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Метод, реализующий Delete-запросы
        /// </summary>
        /// <param name="nodepath">относительный ноды путь</param>
        /// <returns>успешность процедуры в булевом выражении</returns>
        public async Task<bool> DeleteAsync(string nodepath)
        {
            using (var client = new HttpClient())
            {
                try
                {
                    var path = FullPath(nodepath);
                    var response = await client.DeleteAsync(path);
                    response.EnsureSuccessStatusCode();
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
        }
    }
}
