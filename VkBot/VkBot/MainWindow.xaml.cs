using System.Windows;


namespace VkBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        VkService vkService;

        public MainWindow()
        {
            InitializeComponent();

            vkService = new VkService();
        }
    }
}
