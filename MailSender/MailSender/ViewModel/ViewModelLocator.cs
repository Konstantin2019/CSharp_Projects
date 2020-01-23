using System;
using CommonServiceLocator;
using GalaSoft.MvvmLight.Ioc;
using MailSender_lib.Services;
using MailSender_lib.Services.InMemory;

namespace MailSender.ViewModel
{
    public class ViewModelLocator
    {
        private static MailSenderViewModel singleInstance;

        public ViewModelLocator()
        {
            var service = SimpleIoc.Default;
            ServiceLocator.SetLocatorProvider(() => service);

            service.Register<MailSenderViewModel>();

            service.TryRegister<InMemoryEmailProvider>()
                   .TryRegister<InMemoryRecipientProvider>()
                   .TryRegister<InMemorySenderProvider>()
                   .TryRegister<InMemoryServerProvider>()
                   .TryRegister<InMemoryShedulerProvider>()
                   .TryRegister<MailSenderService>();
        }

        public MailSenderViewModel MailSenderVM
        {
            get
            {
                if (singleInstance != null)
                    return singleInstance;
                else
                {
                    singleInstance = ServiceLocator.Current.GetInstance<MailSenderViewModel>();
                    return singleInstance;
                }
            }
        }
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