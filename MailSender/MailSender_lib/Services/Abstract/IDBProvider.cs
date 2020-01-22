using System.Collections.Generic;

namespace MailSender_lib.Services.Abstract
{
    public interface IDBProvider<T>
    {
        IEnumerable<T> GetAll();

        T GetById(int id);

        int Create(T item);

        void Edit(int id, T item);

        bool SaveChanges();

        bool Delete(int id);
    }
}
