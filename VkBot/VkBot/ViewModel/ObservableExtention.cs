using System.Collections.Generic;
using System.Collections.ObjectModel;


namespace VkBot.ViewModel
{
    /// <summary>
    /// Расширение ObservableExtention
    /// </summary>
    public static class ObservableExtention
    {
        public static ObservableCollection<string> ToObservableCollection(this ICollection<string> collection)
        {
            var items = new ObservableCollection<string>();

            foreach (var item in collection)
                items.Add(item);

            return items;
        }
    }
}
