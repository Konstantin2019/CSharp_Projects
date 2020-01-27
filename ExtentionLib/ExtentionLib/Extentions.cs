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
            foreach (var item in collection)
                observables.Add(item);
        }

        public static void ToObservableCollection<T>(this ICollection<T> collection, ObservableCollection<T> observables)
        {
            foreach (var item in collection)
                observables.Add(item);
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
