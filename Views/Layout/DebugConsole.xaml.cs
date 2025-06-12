using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace ZNSO_Notepad.Views.Layout
{
    public partial class DebugConsole : UserControl
    {
        private const int MaxItems = 100; // 最大条目数
        public ObservableCollection<string> DebugItems { get; set; }

        public DebugConsole(ObservableCollection<string> sharedDebug)
        {
            InitializeComponent();

            DebugItems = sharedDebug ?? new ObservableCollection<string>();
            this.DataContext = this;

            // 监听集合变化，自动滚动
            DebugItems.CollectionChanged += DebugItems_CollectionChanged;
        }

        private void HistoryList_Loaded(object sender, RoutedEventArgs e)
        {
            ScrollToBottom();
        }

        private void DebugItems_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            Dispatcher.InvokeAsync(() =>
            {
                while (DebugItems.Count > MaxItems)
                {
                    DebugItems.RemoveAt(0); // 移除最旧的元素
                }
            });

            Dispatcher.InvokeAsync(() => ScrollToBottom());
        }

        private void ScrollToBottom()
        {
            if (HistoryList.Items.Count > 0)
            {
                HistoryList.ScrollIntoView(HistoryList.Items[HistoryList.Items.Count - 1]);
            }
        }
    }
}