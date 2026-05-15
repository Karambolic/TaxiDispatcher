using System.Windows;
using UI.ViewModels;

namespace UI.Views
{
    public partial class AssignDriverWindow : Window
    {
        public AssignDriverWindow(AssignDriverViewModel viewModel)
        {
            InitializeComponent();

            // Link the View to the ViewModel
            this.DataContext = viewModel;
        }

        /// <summary>
        /// Handles the click event for the Assign button
        /// </summary>
        private void Assign_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is AssignDriverViewModel vm && vm.SelectedDriver != null)
            {
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Please, select a driver from the list before confirming",
                                "No Driver Selected",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
            }
        }

        /// <summary>
        /// Handles the click event for the Cancel button.
        /// </summary>
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            // Setting DialogResult to false signals that the operation was aborted
            this.DialogResult = false;
            this.Close();
        }
    }
}