using System.Windows;
using UI.ViewModels;

namespace UI.Views;

    public partial class CreateOrderWindow : Window
    {
        public CreateOrderWindow(CreateOrderViewModel viewModel)
        {
            InitializeComponent();
            this.DataContext = viewModel;
            viewModel.RequestCloseWindow += () => this.Close();
        }
    }
