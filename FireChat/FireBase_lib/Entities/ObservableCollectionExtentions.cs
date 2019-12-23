using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FireBase_lib.Entities
{
    //пишем расширение для класса ObservableCollection
    public static class ObservableCollectionExtentions
    {
        public static void AddRange<T>(this ObservableCollection<T> ts, ICollection<T> collection)
        {
            foreach (var item in collection)
                ts.Add(item);
        }
    }
}
