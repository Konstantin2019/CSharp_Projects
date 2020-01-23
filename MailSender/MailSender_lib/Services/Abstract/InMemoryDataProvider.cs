using System.Collections.Generic;
using System.Linq;
using MailSender_lib.Model.Base;
using System.IO;
using System.Text.Json;
using System;

namespace MailSender_lib.Services.Abstract
{
    public abstract class InMemoryDataProvider<T> : IDBProvider<T> where T : BaseEntity
    {
        protected string path;
        protected List<T> items;

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
                var jsonString = JsonSerializer.Serialize(items);
                File.WriteAllText(path, jsonString);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Init()
        {
            try
            {
                var jsonString = File.ReadAllText(path);
                items = JsonSerializer.Deserialize<List<T>>(jsonString);
            }
            catch (Exception)
            {
                items = new List<T>();
            }
        }
    }
}
