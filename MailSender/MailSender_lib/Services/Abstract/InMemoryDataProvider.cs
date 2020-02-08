using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MailSender_lib.Model.Base;
using Newtonsoft.Json;

namespace MailSender_lib.Services.Abstract
{
    public abstract class InMemoryDataProvider<T> : IDBProvider<T> where T : BaseEntity
    {
        protected string path;
        protected List<T> items = new List<T>();

        public IEnumerable<T> GetAll() => items;

        public T GetById(int id) => items.FirstOrDefault(item => item.Id == id);

        public int Create(T item)
        {
            if (items.Contains(item)) return item.Id;
            item.Id = items.Count == 0 ? 1 : items.Max(i => i.Id) + 1;
            items.Add(item);
            return item.Id;
        }

        public bool Delete(int id)
        {
            var item = GetById(id);
            return items.Remove(item);
        }

        public abstract void Edit(int id, T item);

        public bool SaveChanges()
        {
            try
            {
                var jsonString = JsonConvert.SerializeObject(items);
                var dir = Path.GetDirectoryName(path);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                File.WriteAllText(path, jsonString);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool ReadData()
        {
            try
            {
                var jsonString = File.ReadAllText(path);
                items = JsonConvert.DeserializeObject<List<T>>(jsonString);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
