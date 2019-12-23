using System;
using System.Collections.Generic;
using System.Linq;
using FireBase_lib.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FireBase_lib.Services
{
    public static class UtilityHelper
    {
        public static string TrySerialize(ISerializableObject obj)
        {
            try
            {
                var json = JsonConvert.SerializeObject(obj);
                return json;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static List<T> TryDeserialize<T>(string json) where T: ISerializableObject
        {
            try
            {
                JObject jsonObj = JObject.Parse(json);
                Dictionary<string, T> dictObj = jsonObj.ToObject<Dictionary<string, T>>();
                List<string> keys = dictObj.Keys.ToList();

                var objs = new List<T>();
                foreach (var key in keys)
                    objs.Add(dictObj[key]);

                return objs;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public static string GetKey<T>(string json, T obj) where T : ISerializableObject
        {
            try
            {
                JObject jsonObj = JObject.Parse(json);
                Dictionary<string, T> dictObj = jsonObj.ToObject<Dictionary<string, T>>();
                List<string> keys = dictObj.Keys.ToList();

                foreach (var key in keys)
                {
                    if (dictObj[key].Name == obj.Name && dictObj[key].Value == obj.Value)
                        return key;
                }

                return null;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
