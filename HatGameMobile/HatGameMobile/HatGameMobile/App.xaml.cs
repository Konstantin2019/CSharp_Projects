using Prism;
using Prism.Ioc;
using HatGameMobile.ViewModels;
using HatGameMobile.Views;
using Xamarin.Essentials.Interfaces;
using Xamarin.Essentials.Implementation;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

[assembly: XamlCompilation(XamlCompilationOptions.Compile)]
namespace HatGameMobile
{
    public partial class App
    {
        /* 
         * The Xamarin Forms XAML Previewer in Visual Studio uses System.Activator.CreateInstance.
         * This imposes a limitation in which the App class must have a default constructor. 
         * App(IPlatformInitializer initializer = null) cannot be handled by the Activator.
         */
        public static string RoomId { get; set; }
        public static string TeamName { get; set; }
        public App() : this(null) { }
        public App(IPlatformInitializer initializer) : base(initializer) { }
        protected override async void OnInitialized()
        {
            InitializeComponent();

            await NavigationService.NavigateAsync("NavigationPage/WelcomePage");
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage, MainPageViewModel>();
            containerRegistry.RegisterForNavigation<CustomWordsPage, CustomWordsPageViewModel>();
            containerRegistry.RegisterForNavigation<PresetWordsPage, PresetWordsPageViewModel>();
            containerRegistry.RegisterForNavigation<PlayGamePage, PlayGamePageViewModel>();
            containerRegistry.RegisterForNavigation<WelcomePage, WelcomePageViewModel>();
            containerRegistry.RegisterForNavigation<CreateRoomPage, CreateRoomPageViewModel>();
            containerRegistry.RegisterForNavigation<JoinRoomPage, JoinRoomPageViewModel>();
        }
    }
}
