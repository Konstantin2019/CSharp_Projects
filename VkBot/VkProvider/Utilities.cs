using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace VkBot
{
    /// <summary>
    /// Вспомогательный класс для получения Json объектов, отправки контента и получения локального IP
    /// </summary>
    public static class Utilities
    {
        public static async Task<Response<JObject>> GetJObjectAsync(string request)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetAsync(request);
                    response.EnsureSuccessStatusCode();
                    var response_json = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(response_json);
                    return new Response<JObject> { Value = jObject, Error = null };
                }
                catch (Exception error)
                {
                    return new Response<JObject> { Value = null, Error = error.Message };
                }
            }
        }

        public static async Task<string> PostAsync(string request, Dictionary<string, string> content)
        {
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage response = null;
                try
                {
                    var http_content = new FormUrlEncodedContent(content);
                    response = await client.PostAsync(request, http_content);
                    response.EnsureSuccessStatusCode();
                    var response_string = await response.Content.ReadAsStringAsync();
                    return response_string;
                }
                catch (Exception)
                {
                    return response.ReasonPhrase;
                }
            }
        }

        public static IPAddress GetLocalIP()
        {
            IPAddress Ip = null;
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    Ip = ip;
            }
            return Ip;
        }
    }
}
