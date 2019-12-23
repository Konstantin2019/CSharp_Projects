using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FireBase_lib.Services
{
    public class FireBaseRequests
    {
        private const string JSON_SUFFIX = ".json";
        private HttpClient client;

        private string BaseUrl { get; }

        public FireBaseRequests(string baseurl)
        {
            BaseUrl = baseurl.EndsWith("/")? baseurl : baseurl + "/";
            client = new HttpClient();
        }

        private string PathConstruct(string nodepath) => BaseUrl + nodepath.Trim('/') + JSON_SUFFIX;

        public async Task<bool> Post(string json, string nodepath)
        {
            try
            {
                var path = PathConstruct(nodepath);
                HttpContent content = new StringContent(json);
                HttpResponseMessage request = await client.PostAsync(path, content);
                request.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<string> Get(string nodepath)
        {
            try
            {
                var path = PathConstruct(nodepath);
                HttpResponseMessage response = await client.GetAsync(path);
                response.EnsureSuccessStatusCode();
                string json = await response.Content.ReadAsStringAsync();
                return json;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> Delete(string nodepath)
        {
            try
            {
                var path = PathConstruct(nodepath);
                HttpResponseMessage response = await client.DeleteAsync(path);
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
