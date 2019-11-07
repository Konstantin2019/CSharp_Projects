using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;


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
        }
    }
}
