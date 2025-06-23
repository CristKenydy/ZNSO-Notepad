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
            App.ChangeTheme(ThemeComboBox.SelectedItem.ToString());
        }
    }
}
