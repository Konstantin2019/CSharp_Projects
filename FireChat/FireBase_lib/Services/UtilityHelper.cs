using System;
using System.Collections.Generic;
using System.Linq;
using FireBase_lib.Entities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FireBase_lib.Services
{
    /// <summary>
    /// Вспомогательный класс для сериализации и десериализации объектов
    /// </summary>
    public static class UtilityHelper
    {
        /// <summary>
        /// Метод, пытающийся осуществить json-сериализацию
        /// </summary>
        /// <param name="obj">сериализуемый объект</param>
        /// <returns>сериализованный объект</returns>
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

        /// <summary>
        /// Метод, пытающийся осуществить json-десериализацию
        /// </summary>
        /// <typeparam name="T">тип объекта десериализации</typeparam>
        /// <param name="json">сериализованный объект</param>
        /// <returns>десериализованный объект</returns>
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

        /// <summary>
        /// Вспомогательный класс для навигации по нодам БД
        /// </summary>
        /// <typeparam name="T">тип целевого объекта</typeparam>
        /// <param name="json">сериализованный целевой объект</param>
        /// <param name="obj">целевой объект</param>
        /// <returns>имя ноды, содержащей целевой объект</returns>
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
