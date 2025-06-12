using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace ZNSO_Notepad.Views.Controls
{
    public partial class TextEdit : UserControl
    {
        public string Header { get; set; }
        public TextEdit()
        {
            InitializeComponent();
            this.DataContext = this;
            Header = "Text Edit";

            EditorBox.Focusable = true;

            PreviewKeyDown += TextEdit_KeyDown;
        }

        private void TextEdit_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                if (e.Key == Key.Delete)
                {
                    Delete_Click(null, null);
                }
            }
        }
        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = this.Width + e.HorizontalChange;
            double newHeight = this.Height + e.VerticalChange;

            if (newWidth >= this.MinWidth)
                this.Width = newWidth;
            if (newHeight >= this.MinHeight)
                this.Height = newHeight;
        }

        private void Bold_Click(object sender, RoutedEventArgs e)
        {
            EditingCommands.ToggleBold.Execute(null, EditorBox);
        }

        private void Italic_Click(object sender, RoutedEventArgs e)
        {
            EditingCommands.ToggleItalic.Execute(null, EditorBox);
        }

        private void Header1_Click(object sender, RoutedEventArgs e)
        {
            ApplyHeader(24);
        }

        private void Header2_Click(object sender, RoutedEventArgs e)
        {
            ApplyHeader(18);
        }

        private void ApplyHeader(double fontSize)
        {
            TextSelection selection = EditorBox.Selection;
            if (!selection.IsEmpty)
            {
                selection.ApplyPropertyValue(TextElement.FontSizeProperty, fontSize);
                selection.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is Panel parentPanel)
            {
                parentPanel.Children.Remove(this);
            }
            else if (this.Parent is ContentControl contentControl)
            {
                contentControl.Content = null;
            }
            else
            {
                MessageBox.Show("Cannot delete this control", "Faild to delete", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            App.znpEditor._isDirty = true;
        }

        private void HeaderTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (HeaderTextBox.IsFocused == true)
            {
                if (e.Key == Key.Enter)
                {
                    Header = HeaderTextBox.Text;
                    EditorBox.Focus();
                }
            }

            App.znpEditor._isDirty = true;
        }

        private void HeaderTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!HeaderTextBox.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                HeaderTextBox.Focus();
                HeaderTextBox.SelectAll();
            }
        }

    }
}
