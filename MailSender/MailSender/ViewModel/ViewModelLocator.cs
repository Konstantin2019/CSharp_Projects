using CommonServiceLocator;
using ExtentionLib;
using GalaSoft.MvvmLight.Ioc;
using MailSender.ViewModel.WPFServices;
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
                   .TryRegister<MailSenderService>()
                   .TryRegister<WindowsService>();
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
}