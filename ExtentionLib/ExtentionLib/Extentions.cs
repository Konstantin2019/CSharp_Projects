using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ExtentionLib
{
    /// <summary>
    /// Расширение ObservableExtention
    /// </summary>
    public static class ObservableExtention
    {
        public static void ToObservableCollection<T>(this IEnumerable<T> collection, ObservableCollection<T> observables)
        {
            if (observables == null)
                observables = new ObservableCollection<T>();
            foreach (var item in collection)
                observables.Add(item);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> collection)
        {
            var observables = new ObservableCollection<T>();
            foreach (var item in collection)
                observables.Add(item);
            return observables;
        }

        public static void ToObservableCollection<T>(this ICollection<T> collection, ObservableCollection<T> observables)
        {
            if (observables == null)
                observables = new ObservableCollection<T>();
            foreach (var item in collection)
                observables.Add(item);
        }

        public static ObservableCollection<T> ToObservableCollection<T>(this ICollection<T> collection)
        {
            var observables = new ObservableCollection<T>();
            foreach (var item in collection)
                observables.Add(item);
            return observables;
        }
    }
    /// <summary>
    /// Расширение для безопасной регистрации сервисов MVVM локатором
    /// </summary>
    public static class SimpleIocExtention
    {
        public static SimpleIoc TryRegister<TInterface, TService>(this SimpleIoc services)
            where TInterface : class
            where TService : class, TInterface
        {
            if (services.IsRegistered<TInterface>()) return services;
            services.Register<TInterface, TService>();
            return services;
        }

        public static SimpleIoc TryRegister<TService>(this SimpleIoc services)
            where TService : class
        {
            if (services.IsRegistered<TService>()) return services;
            services.Register<TService>();
            return services;
        }

        public static SimpleIoc TryRegister<TService>(this SimpleIoc services, Func<TService> Factory)
            where TService : class
        {
            if (services.IsRegistered<TService>()) return services;
            services.Register(Factory);
            return services;
        }
    }
}
