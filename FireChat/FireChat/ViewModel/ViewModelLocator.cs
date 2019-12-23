using CommonServiceLocator;
using FireBase_lib.Services;
using GalaSoft.MvvmLight.Ioc;
using System;

namespace FireChat.ViewModel
{
    public class ViewModelLocator
    {
        public ViewModelLocator()
        {
            var service = SimpleIoc.Default;
            ServiceLocator.SetLocatorProvider(() => service);

            service.Register<FireChatViewModel>();

            service.TryRegister<MessangerActions>();
        }

        public FireChatViewModel FireChatVM => ServiceLocator.Current.GetInstance<FireChatViewModel>();
    }

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