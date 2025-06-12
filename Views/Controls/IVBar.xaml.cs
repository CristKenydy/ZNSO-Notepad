using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace ZNSO_Notepad.Views.Controls
{
    public partial class IVBar : UserControl
    {
        public string Header { get; set; }

        private bool _isPlaying = false;

        public IVBar()
        {
            InitializeComponent();
            this.DataContext = this;
            Header = "IVBar";

            PreviewKeyDown += IVBar_KeyDown;
        }

        private void IVBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                if (e.Key == Key.Delete)
                {
                    Delete_Click(null, null);
                }
            }
        }

        private void BtnLoadImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
            };

            if (dlg.ShowDialog() == true)
            {
                MediaDisplay.Stop();
                MediaDisplay.Visibility = Visibility.Collapsed;

                // 加载图像
                ImageDisplay.Source = new System.Windows.Media.Imaging.BitmapImage(new Uri(dlg.FileName));
                ImageDisplay.Visibility = Visibility.Visible;

                // 保存图像到用户文档文件夹
                string userDocumentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string savedImagePath = Path.Combine(userDocumentsFolder, "SavedImage.jpg");

                var bitmap = new System.Windows.Media.Imaging.BitmapImage(new Uri(dlg.FileName));
                using (var fileStream = new FileStream(savedImagePath, FileMode.Create))
                {
                    var encoder = new System.Windows.Media.Imaging.JpegBitmapEncoder();
                    encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bitmap));
                    encoder.Save(fileStream);
                }

                BtnPlayPause.IsEnabled = false;
                _isPlaying = false;
                BtnPlayPause.Content = "播放";

                // TODO: 如果需要生成 MP4，可以在这里调用相关逻辑
            }
            App.znpEditor._isDirty = true;
        }

        private void BtnLoadVideo_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog()
            {
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos),
                Filter = "Video Files (*.mp4; *.avi; *.mov; *.wmv; *.webm)|*.mp4;*.avi;*.mov;*.wmv;*.webm"
            };

            if (dlg.ShowDialog() == true)
            {
                ImageDisplay.Source = null;
                ImageDisplay.Visibility = Visibility.Collapsed;

                MediaDisplay.Source = new Uri(dlg.FileName);
                MediaDisplay.Visibility = Visibility.Visible;
                MediaDisplay.Stop();

                BtnPlayPause.IsEnabled = true;
                _isPlaying = false;
                BtnPlayPause.Content = "▶";
            }
            App.znpEditor._isDirty = true;
        }

        private void BtnPlayPause_Click(object sender, RoutedEventArgs e)
        {
            if (_isPlaying)
            {
                MediaDisplay.Pause();
                BtnPlayPause.Content = "▶";
            }
            else
            {
                MediaDisplay.Play();
                BtnPlayPause.Content = "⏸";
            }
            _isPlaying = !_isPlaying;
        }

        private void ResizeThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            double newWidth = Width + e.HorizontalChange;
            double newHeight = Height + e.VerticalChange;

            if (newWidth >= MinWidth)
                Width = newWidth;
            if (newHeight >= MinHeight)
                Height = newHeight;

            App.znpEditor._isDirty = true;
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
                MessageBox.Show($"Cannot delete {HeaderTextBox.Text}", $"Faild to delete {HeaderTextBox.Text}", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

            App.znpEditor._isDirty = true;
        }

        private void HeaderTextBox_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            HeaderTextBox.SelectAll();
        }

        private void HeaderTextBox_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
