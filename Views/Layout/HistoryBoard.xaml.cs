using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ZNSO_Notepad.Views.Layout
{
    public partial class HistoryBoard : UserControl
    {
        public ObservableCollection<string> HistoryItems { get; set; }

        public HistoryBoard(ObservableCollection<string> sharedHistory)
        {
            InitializeComponent();

            HistoryItems = sharedHistory ?? new ObservableCollection<string>();
            this.DataContext = this;
        }

        private void HistoryList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HistoryList.SelectedItem is string selected)
            {
                MessageBox.Show($"你选择了历史记录项：{selected}", "历史记录", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }

}
