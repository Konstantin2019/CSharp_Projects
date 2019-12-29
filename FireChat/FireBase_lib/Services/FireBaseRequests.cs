using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace FireBase_lib.Services
{
    public class FireBaseRequests
    {
        private const string JSON_SUFFIX = ".json";

        private string BaseUrl { get; }

        public FireBaseRequests(string baseurl)
        {
            BaseUrl = baseurl.EndsWith("/")? baseurl : baseurl + "/";
        }

        private string FullPath(string nodepath) => BaseUrl + nodepath.Trim('/') + JSON_SUFFIX;

        public async Task<bool> Post(string json, string nodepath)
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

        public async Task<string> Get(string nodepath)
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

        public async Task<bool> Delete(string nodepath)
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
