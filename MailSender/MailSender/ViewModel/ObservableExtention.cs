using MailSender_lib.Model.Base;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MailSender.ViewModel
{
    /// <summary>
    /// Расширение ObservableExtention
    /// </summary>
    public static class ObservableExtention
    {
        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection) 
            where T : BaseEntity
        {
            var items = new ObservableCollection<T>();

            foreach (var item in collection)
                items.Add(item);

            return items;
        }
    }
}
