using System.Windows;
using System.Windows.Controls;

namespace ZNSO_Notepad.Windows
{
    public partial class OptionsDialog : Window
    {
        public OptionsDialog()
        {
            InitializeComponent();
        }

        private void ApplyTheme_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("This version of ZNSO Notepad does not support changing themes yet.", "Feature Unavailable", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}
