using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FireBase_lib.Entities
{
    /// <summary>
    /// Расширение для класса ObservableCollection
    /// </summary>
    public static class ObservableCollectionExtentions
    {
        public static void AddRange<T>(this ObservableCollection<T> ts, ICollection<T> collection)
        {
            foreach (var item in collection)
                ts.Add(item);
        }
    }
}
