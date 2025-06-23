using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ZNSO_Notepad.Windows
{
    /// <summary>
    /// Interaction logic for CreateControlDialog.xaml
    /// </summary>
    public partial class CreateControlDialog : Window
    {
        public CreateControlDialog()
        {
            InitializeComponent();
        }

        private void newTEbutton_click(object sender, RoutedEventArgs e)
        {
            App.znpEditor.BtnAddTextEdit();
            this.DialogResult = true;
            this.Close();
        }

        private void newIBbutton_click(object sender, RoutedEventArgs e)
        {
            App.znpEditor.BtnAddIVBar();
            this.DialogResult = true;
            this.Close();
        }
    }
}
