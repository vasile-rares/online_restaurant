using Microsoft.Extensions.DependencyInjection;
using OnlineRestaurant.ViewModels;
using System;
using System.Windows;

namespace OnlineRestaurant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Injectăm MainViewModel-ul ca DataContext
            DataContext = serviceProvider.GetRequiredService<MainViewModel>();
        }
    }
}