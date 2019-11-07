using System.Windows;
using System.Windows.Controls;

namespace VkBot
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ViewModel viewModel;

        public MainWindow()
        {
            InitializeComponent();

            viewModel = new ViewModel();

            MainGrid.Loaded += viewModel.ValidateTokenAsync;
            Send_btn.Click += viewModel.SendMessageAsync;

            token_tb.TextChanged += Token_tb_TextChanged;
            message_tb.TextChanged += Message_tb_TextChanged;

            response_view.ItemsSource = viewModel.Responses;
        }

        private void Message_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            viewModel.Message = message_tb.Text;
        }

        private void Token_tb_TextChanged(object sender, TextChangedEventArgs e)
        {
            viewModel.Token_uri = token_tb.Text;
        }
    }
}
