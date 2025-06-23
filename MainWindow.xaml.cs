using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;
using ZNSO.Notepad.Editor.Services;
using ZNSO_Notepad.Services;
using ZNSO_Notepad.Views.Controls;
using ZNSO_Notepad.Windows;

namespace ZNSO_Notepad
{
    public partial class MainWindow : Window
    {
        public bool _isDirty = false;
        private string _currentWorkspacePath = null;

        private bool _isMiddleMouseDown = false;
        private Point _middleMouseStartPoint;
        private double _startHorizontalOffset;
        private double _startVerticalOffset;

        private DispatcherTimer _blurTimer;

        private Point _dragStartPoint;
        private UIElement _draggedElement = null;
        private Point _elementStartPosition;

        private Point _startPoint;
        private bool _isDrawing;
        private FrameworkElement _previewElement;

        public ObservableCollection<string> _historyLog;
        public ObservableCollection<string> _debugLog;

        private Tools toolState;
        enum Tools
        {
            None,
            Pancel,
            Eraser,
            TextEditTool,
            IVBarEditTool
        }

        public MainWindow()
        {
            InitializeComponent();

            toolState = Tools.None;

            WorkspaceScrollViewer.PreviewMouseDown += WorkspaceScrollViewer_PreviewMouseDown;
            WorkspaceScrollViewer.PreviewMouseMove += WorkspaceScrollViewer_PreviewMouseMove;
            WorkspaceScrollViewer.PreviewMouseUp += WorkspaceScrollViewer_PreviewMouseUp;

            WorkspaceCanvas.PreviewMouseLeftButtonDown += WorkspaceCanvas_PreviewMouseLeftButtonDown;
            WorkspaceCanvas.PreviewMouseMove += WorkspaceCanvas_PreviewMouseMove;
            WorkspaceCanvas.PreviewMouseLeftButtonUp += WorkspaceCanvas_PreviewMouseLeftButtonUp;

            Closing += WindowClosing;

            CommandBindings.Add(new CommandBinding(ApplicationCommands.New, NewControl_Executed));
            InputBindings.Add(new KeyBinding(ApplicationCommands.New, new KeyGesture(Key.T, ModifierKeys.Control | ModifierKeys.Shift)));

            _historyLog = new ObservableCollection<string>();
            _debugLog = new ObservableCollection<string>();
        }

        private void ShowHistoryBoard_Click(object sender, RoutedEventArgs e)
        {
            RightPanelContent.Content = new Views.Layout.HistoryBoard(_historyLog);
            _debugLog.Add("History Board is shown.");
        }

        private void ShowCodeViewer_Click(object sender, RoutedEventArgs e)
        {
            BottomPanelContent.Content = new Views.Layout.CodeViewer(_currentWorkspacePath);
            _debugLog.Add("Code Viewer is shown.");
        }

        private void ShowDebugConsole_Click(object sender, RoutedEventArgs e)
        {
            BottomPanelContent.Content = new Views.Layout.DebugConsole(_debugLog);
            _debugLog.Add("Debug Console is shown.");
        }

        // 切换为浅色主题
        private void ChangeThemeLight_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Switched to Light Theme (未实现具体逻辑)");
            // TODO: 实现切换到浅色主题的逻辑
            _debugLog.Add("Switched to Light Theme");
        }

        // 切换为深色主题
        private void ChangeThemeDark_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Switched to Dark Theme (this version is not support)");
            // TODO: 实现切换到深色主题的逻辑
            _debugLog.Add("Switched to Dark Theme");
        }

        // 切换为系统默认主题
        private void ChangeThemeSystem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Switched to System Default Theme (this version is not support)");
            // TODO: 实现切换到系统主题的逻辑
            _debugLog.Add("Switched to System Default Theme");
        }

        // 打开主题设置窗口（或面板）
        private void BtnChangeTheme_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("打开主题设置窗口 (this version is not support)");
        }

        // 打开首选项设置
        private void BtnOpenPreferences_Click(object sender, RoutedEventArgs e)
        {
            OptionsDialog optionsDialog = new OptionsDialog();
            optionsDialog.ShowDialog();
        }

        private void WindowClosing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!_isDirty) return;

            MessageBoxResult result = MessageBox.Show("If you want to save changes before exit?", "Remind", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                BtnSaveWorkspace_Click(null, null);
            }
            else if (result == MessageBoxResult.No)
            {
                _currentWorkspacePath = null;
            }
            else if (result == MessageBoxResult.Cancel)
            {
                e.Cancel = true;
            }
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            base.OnPreviewMouseWheel(e);

            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                e.Handled = true;

                // 初始化 TransformGroup + ScaleTransform
                var group = WorkspaceCanvas.RenderTransform as TransformGroup;
                ScaleTransform scaleTransform = null;

                if (group == null)
                {
                    scaleTransform = new ScaleTransform(1.0, 1.0);
                    group = new TransformGroup();
                    group.Children.Add(scaleTransform);
                    WorkspaceCanvas.RenderTransform = group;
                    WorkspaceCanvas.RenderTransformOrigin = new Point(0, 0);
                }
                else
                {
                    scaleTransform = group.Children.OfType<ScaleTransform>().FirstOrDefault();
                    if (scaleTransform == null)
                    {
                        scaleTransform = new ScaleTransform(1.0, 1.0);
                        group.Children.Add(scaleTransform);
                    }
                }

                // 缩放逻辑
                double zoomStep = 0.1;
                double zoom = e.Delta > 0 ? zoomStep : -zoomStep;
                double currentScale = scaleTransform.ScaleX;
                double newScale = Math.Max(0.2, Math.Min(5.0, currentScale + zoom));
                double scaleRatio = newScale / currentScale;

                // 获取鼠标在 WorkspaceCanvas 中的位置（逻辑坐标）
                Point mousePosCanvas = e.GetPosition(WorkspaceCanvas);

                // 获取鼠标在 ScrollViewer 中的位置（视觉坐标）
                Point mousePosScroll = e.GetPosition(WorkspaceScrollViewer);

                // 计算缩放前的视觉偏移
                double absX = WorkspaceScrollViewer.HorizontalOffset + mousePosScroll.X;
                double absY = WorkspaceScrollViewer.VerticalOffset + mousePosScroll.Y;

                // 更新缩放
                scaleTransform.ScaleX = newScale;
                scaleTransform.ScaleY = newScale;

                // 计算缩放后应有的偏移，确保鼠标焦点保持位置
                double newAbsX = absX * scaleRatio;
                double newAbsY = absY * scaleRatio;

                WorkspaceScrollViewer.ScrollToHorizontalOffset(newAbsX - mousePosScroll.X);
                WorkspaceScrollViewer.ScrollToVerticalOffset(newAbsY - mousePosScroll.Y);

                _debugLog.Add($"Zoom scale: {newScale:F2} | Mouse at: ({mousePosCanvas.X:F1}, {mousePosCanvas.Y:F1})");
            }
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            base.OnMouseEnter(e);

            if (_blurTimer != null && _blurTimer.IsEnabled)
                return; // 避免多次触发

            _blurTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // 约 60 FPS
            };

            _blurTimer.Tick += (s, args) =>
            {
                if (EFFECT_Blur.Radius > 0)
                {
                    EFFECT_Blur.Radius -= 0.5;
                }
                else
                {
                    EFFECT_Blur.Radius = 0;
                    _blurTimer.Stop();
                }
            };

            _blurTimer.Start();
        }

        private void WorkspaceScrollViewer_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                _isMiddleMouseDown = true;
                _middleMouseStartPoint = e.GetPosition(this);
                _startHorizontalOffset = WorkspaceScrollViewer.HorizontalOffset;
                _startVerticalOffset = WorkspaceScrollViewer.VerticalOffset;
                WorkspaceScrollViewer.Cursor = Cursors.SizeAll;
                WorkspaceScrollViewer.CaptureMouse();

                _debugLog.Add("Started moving workspace with middle mouse button at: " +
                              $"({WorkspaceScrollViewer.HorizontalOffset}, {WorkspaceScrollViewer.VerticalOffset})");
            }
        }

        private void WorkspaceScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_isMiddleMouseDown)
            {
                Point currentPoint = e.GetPosition(this);
                Vector delta = Point.Subtract(currentPoint, _middleMouseStartPoint);

                WorkspaceScrollViewer.ScrollToHorizontalOffset(WorkspaceScrollViewer.HorizontalOffset - delta.X);
                WorkspaceScrollViewer.ScrollToVerticalOffset(WorkspaceScrollViewer.VerticalOffset - delta.Y);

                _middleMouseStartPoint = currentPoint;
                _debugLog.Add("Moved workspace with middle mouse button: " +
                              $"Offset (X: {WorkspaceScrollViewer.HorizontalOffset}, Y: {WorkspaceScrollViewer.VerticalOffset})");
            }
        }

        private void WorkspaceScrollViewer_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isMiddleMouseDown && e.ChangedButton == MouseButton.Middle)
            {
                _isMiddleMouseDown = false;
                WorkspaceScrollViewer.Cursor = Cursors.Arrow;
                WorkspaceScrollViewer.ReleaseMouseCapture();

                _debugLog.Add("Stopped moving workspace with middle mouse button.");
            }
        }


        private void PlaySTCRoughAnimation(UIElement element)
        {
            TranslateTransform transform = new TranslateTransform { X = 300 };
            element.RenderTransform = transform;

            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            var animation = new DoubleAnimation
            {
                From = 300,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(600),
                EasingFunction = ease
            };

            transform.BeginAnimation(TranslateTransform.XProperty, animation);
            _debugLog.Add("Created control: " + element.GetType().Name);
        }

        private void WorkspaceCanvas_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                var element = e.Source as UIElement;
                if (element != null && WorkspaceCanvas.Children.Contains(element))
                {
                    element.Focus();
                    _draggedElement = element;
                    _dragStartPoint = e.GetPosition(WorkspaceCanvas);
                    _elementStartPosition = new Point((int)Canvas.GetLeft(_draggedElement), (int)Canvas.GetTop(_draggedElement));
                }
            }
            _debugLog.Add("Dragged element: " + (_draggedElement != null ? _draggedElement.GetType().Name : "null"));
            _isDirty = true;
        }

        private void WorkspaceCanvas_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (_draggedElement != null && e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPoint = e.GetPosition(WorkspaceCanvas);
                Vector offset = currentPoint - _dragStartPoint;

                if (offset.Length > 5)
                {
                    Canvas.SetLeft(_draggedElement, _elementStartPosition.X + offset.X);
                    Canvas.SetTop(_draggedElement, _elementStartPosition.Y + offset.Y);
                    _isDirty = true;
                }
            }
        }

        private void WorkspaceCanvas_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _draggedElement = null;
        }

        private void NewControl_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CreateControlDialog controlDialog = new CreateControlDialog();
            controlDialog.ShowDialog();
        }

        public void BtnAddTextEdit()
        {
            Point mousePosition = Mouse.GetPosition(WorkspaceCanvas);
            AddTextEditAt(mousePosition);
            _isDirty = true;
        }

        public void BtnAddIVBar()
        {
            Point mousePosition = Mouse.GetPosition(WorkspaceCanvas);
            AddIVBarAt(mousePosition);
            _isDirty = true;
        }

        private void AddTextEditAt(Point position)
        {
            var textEdit = new TextEdit { Focusable = true };
            Canvas.SetLeft(textEdit, position.X);
            Canvas.SetTop(textEdit, position.Y);
            WorkspaceCanvas.Children.Add(textEdit);
            PlaySTCRoughAnimation(textEdit);
            _historyLog.Add("Added A Text Edit");
            _debugLog.Add("Added Text Edit at position: " + position);
        }

        private void AddIVBarAt(Point position)
        {
            var ivBar = new IVBar();
            Canvas.SetLeft(ivBar, position.X);
            Canvas.SetTop(ivBar, position.Y);
            WorkspaceCanvas.Children.Add(ivBar);
            PlaySTCRoughAnimation(ivBar);
            _historyLog.Add("Added A IVBar");
            _debugLog.Add("Added IVBar at position: " + position);
        }

        private void BtnSaveWorkspace_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_currentWorkspacePath))
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Filter = "ZNSO Notepad Files (*.znp)|*.znp",
                    Title = "Save Work Space"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    _currentWorkspacePath = saveFileDialog.FileName;
                    SaveWorkspaceToFile(_currentWorkspacePath);
                    _debugLog.Add("Saved workspace to: " + _currentWorkspacePath);
                }
                else
                {
                    
                }
            }
            else
            {
                SaveWorkspaceToFile(_currentWorkspacePath);
                _debugLog.Add("Saved Successfully");
            }
            _isDirty = false;
        }

        private void SaveWorkspaceToFile(string filePath)
        {
            var elements = new List<UIElementData>();
            foreach (UIElement element in WorkspaceCanvas.Children)
            {
                if (element is FrameworkElement frameworkElement)
                {
                    var elementData = new UIElementData
                    {
                        Type = frameworkElement.GetType().FullName,
                        X = Canvas.GetLeft(frameworkElement),
                        Y = Canvas.GetTop(frameworkElement)
                    };

                    if (frameworkElement is TextEdit textEdit)
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            var range = new TextRange(textEdit.EditorBox.Document.ContentStart, textEdit.EditorBox.Document.ContentEnd);
                            range.Save(memoryStream, DataFormats.Rtf);
                            elementData.Content = Convert.ToBase64String(memoryStream.ToArray());
                            elementData.ContentFormat = "RTF";
                        }
                        elementData.Width = textEdit.Width;
                        elementData.Height = textEdit.Height;
                        elementData.Header = textEdit.Header; // 保存 Header
                    }
                    else if (frameworkElement is IVBar ivBar)
                    {
                        elementData.Content = ivBar.ImageDisplay.Source?.ToString() ?? ivBar.MediaDisplay.Source?.ToString();
                        elementData.ContentFormat = "URI";
                        elementData.Width = ivBar.Width;
                        elementData.Height = ivBar.Height;
                        elementData.Header = ivBar.Header; // 保存 Header
                    }

                    elements.Add(elementData);
                }
            }

            ZnpSerializer.Save(filePath, elements);
            _isDirty = false;
        }

        public void Load(string path)
        {
            _currentWorkspacePath = path;
            var elements = ZnpSerializer.Load<List<UIElementData>>(_currentWorkspacePath);

            WorkspaceCanvas.Children.Clear();
            ShowDebugConsole_Click(null, null);
            ShowHistoryBoard_Click(null, null);
            foreach (var elementData in elements)
            {
                if (elementData.Type == typeof(TextEdit).FullName)
                {
                    var textEdit = new TextEdit();
                    Canvas.SetLeft(textEdit, elementData.X);
                    Canvas.SetTop(textEdit, elementData.Y);
                    textEdit.Width = elementData.Width;
                    textEdit.Height = elementData.Height;

                    if (!string.IsNullOrEmpty(elementData.Content) && elementData.ContentFormat == "RTF")
                    {
                        using (var memoryStream = new MemoryStream(Convert.FromBase64String(elementData.Content)))
                        {
                            var range = new TextRange(textEdit.EditorBox.Document.ContentStart, textEdit.EditorBox.Document.ContentEnd);
                            range.Load(memoryStream, DataFormats.Rtf);
                        }
                    }

                    textEdit.Header = elementData.Header; // 读取 Header
                    WorkspaceCanvas.Children.Add(textEdit);
                }
                else if (elementData.Type == typeof(IVBar).FullName)
                {
                    var ivBar = new IVBar();
                    Canvas.SetLeft(ivBar, elementData.X);
                    Canvas.SetTop(ivBar, elementData.Y);
                    ivBar.Width = elementData.Width;
                    ivBar.Height = elementData.Height;

                    if (!string.IsNullOrEmpty(elementData.Content) && elementData.ContentFormat == "URI")
                    {
                        if (elementData.Content.EndsWith(".jpg") || elementData.Content.EndsWith(".png"))
                        {
                            ivBar.ImageDisplay.Source = new BitmapImage(new Uri(elementData.Content));
                            ivBar.ImageDisplay.Visibility = Visibility.Visible;
                            ivBar.MediaDisplay.Visibility = Visibility.Collapsed;
                        }
                        else if (elementData.Content.EndsWith(".mp4") || elementData.Content.EndsWith(".avi"))
                        {
                            ivBar.MediaDisplay.Source = new Uri(elementData.Content);
                            ivBar.MediaDisplay.Visibility = Visibility.Visible;
                            ivBar.ImageDisplay.Visibility = Visibility.Collapsed;
                        }
                    }

                    ivBar.Header = elementData.Header; // 读取 Header
                    WorkspaceCanvas.Children.Add(ivBar);
                }
            }
            _isDirty = false;

            _historyLog.Clear();
            _debugLog.Add("Loaded workspace from: " + _currentWorkspacePath);
        }

        private void BtnLoadWorkspace_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Filter = "ZNSO Notepad Files (*.znp)|*.znp",
                    Title = "Load a work space"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    _currentWorkspacePath = openFileDialog.FileName;
                    var elements = ZnpSerializer.Load<List<UIElementData>>(_currentWorkspacePath);

                    WorkspaceCanvas.Children.Clear();
                    ShowDebugConsole_Click(null, null);
                    ShowHistoryBoard_Click(null, null);
                    foreach (var elementData in elements)
                    {
                        if (elementData.Type == typeof(TextEdit).FullName)
                        {
                            var textEdit = new TextEdit();
                            Canvas.SetLeft(textEdit, elementData.X);
                            Canvas.SetTop(textEdit, elementData.Y);
                            textEdit.Width = elementData.Width;
                            textEdit.Height = elementData.Height;

                            if (!string.IsNullOrEmpty(elementData.Content) && elementData.ContentFormat == "RTF")
                            {
                                using (var memoryStream = new MemoryStream(Convert.FromBase64String(elementData.Content)))
                                {
                                    var range = new TextRange(textEdit.EditorBox.Document.ContentStart, textEdit.EditorBox.Document.ContentEnd);
                                    range.Load(memoryStream, DataFormats.Rtf);
                                }
                            }

                            textEdit.Header = elementData.Header; // 读取 Header
                            WorkspaceCanvas.Children.Add(textEdit);
                        }
                        else if (elementData.Type == typeof(IVBar).FullName)
                        {
                            var ivBar = new IVBar();
                            Canvas.SetLeft(ivBar, elementData.X);
                            Canvas.SetTop(ivBar, elementData.Y);
                            ivBar.Width = elementData.Width;
                            ivBar.Height = elementData.Height;

                            if (!string.IsNullOrEmpty(elementData.Content) && elementData.ContentFormat == "URI")
                            {
                                if (elementData.Content.EndsWith(".jpg") || elementData.Content.EndsWith(".png"))
                                {
                                    ivBar.ImageDisplay.Source = new BitmapImage(new Uri(elementData.Content));
                                    ivBar.ImageDisplay.Visibility = Visibility.Visible;
                                    ivBar.MediaDisplay.Visibility = Visibility.Collapsed;
                                }
                                else if (elementData.Content.EndsWith(".mp4") || elementData.Content.EndsWith(".avi"))
                                {
                                    ivBar.MediaDisplay.Source = new Uri(elementData.Content);
                                    ivBar.MediaDisplay.Visibility = Visibility.Visible;
                                    ivBar.ImageDisplay.Visibility = Visibility.Collapsed;
                                }
                            }

                            ivBar.Header = elementData.Header; // 读取 Header
                            WorkspaceCanvas.Children.Add(ivBar);
                        }
                    }
                    _isDirty = false;
                }
                _historyLog.Clear();
                _debugLog.Add("Loaded workspace from: " + _currentWorkspacePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"错误: {ex.Message}", "错误❌");
                return;
            }
        }

        private void BtnIncreaseTextEditSize_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in WorkspaceCanvas.Children)
            {
                if (child is TextEdit textEdit)
                {
                    textEdit.Width += 50;
                    textEdit.Height += 50;
                    _isDirty = true;
                }
            }
        }

        private void PenTool_Click(object sender, RoutedEventArgs e) => toolState = Tools.Pancel;

        private void EraserTool_Click(object sender, RoutedEventArgs e) => toolState = Tools.Eraser;

        private void BtnTextEditTool_Click(object sender, RoutedEventArgs e) => toolState = Tools.TextEditTool;

        private void BtnIVBarTool_Click(object sender, RoutedEventArgs e) => toolState = Tools.IVBarEditTool;


        private void Close(object sender, RoutedEventArgs e) => this.Close();

        private void WorkspaceCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _startPoint = e.GetPosition(WorkspaceCanvas);
            _isDrawing = true;

            switch (toolState)
            {
                case Tools.Pancel:
                    // 可以在 MouseMove 中实时画线
                    break;

                case Tools.TextEditTool:
                    _previewElement = new Border
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Background = Brushes.Transparent
                    };
                    Canvas.SetLeft(_previewElement, _startPoint.X);
                    Canvas.SetTop(_previewElement, _startPoint.Y);
                    WorkspaceCanvas.Children.Add(_previewElement);
                    break;
                case Tools.IVBarEditTool:
                    _previewElement = new Border
                    {
                        BorderBrush = Brushes.Gray,
                        BorderThickness = new Thickness(1),
                        Background = Brushes.Transparent
                    };
                    Canvas.SetLeft(_previewElement, _startPoint.X);
                    Canvas.SetTop(_previewElement, _startPoint.Y);
                    WorkspaceCanvas.Children.Add(_previewElement);
                    break;
            }
        }

        private void WorkspaceCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isDrawing) return;
            Point currentPoint = e.GetPosition(WorkspaceCanvas);

            switch (toolState)
            {
                case Tools.Pancel:
                    var line = new Line
                    {
                        Stroke = Brushes.Black,
                        StrokeThickness = 2,
                        X1 = _startPoint.X,
                        Y1 = _startPoint.Y,
                        X2 = currentPoint.X,
                        Y2 = currentPoint.Y
                    };
                    WorkspaceCanvas.Children.Add(line);
                    _startPoint = currentPoint; // 连续线条
                    break;

                case Tools.TextEditTool:
                    if (_previewElement != null)
                    {
                        double width = Math.Abs(currentPoint.X - _startPoint.X);
                        double height = Math.Abs(currentPoint.Y - _startPoint.Y);
                        double left = Math.Min(currentPoint.X, _startPoint.X);
                        double top = Math.Min(currentPoint.Y, _startPoint.Y);

                        Canvas.SetLeft(_previewElement, left);
                        Canvas.SetTop(_previewElement, top);
                        _previewElement.Width = width;
                        _previewElement.Height = height;
                    }
                    break;
                case Tools.IVBarEditTool:
                    if (_previewElement != null)
                    {
                        double width = Math.Abs(currentPoint.X - _startPoint.X);
                        double height = Math.Abs(currentPoint.Y - _startPoint.Y);
                        double left = Math.Min(currentPoint.X, _startPoint.X);
                        double top = Math.Min(currentPoint.Y, _startPoint.Y);

                        Canvas.SetLeft(_previewElement, left);
                        Canvas.SetTop(_previewElement, top);
                        _previewElement.Width = width;
                        _previewElement.Height = height;
                    }
                    break;
            }
        }

        private void WorkspaceCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;

            switch (toolState)
            {
                case Tools.TextEditTool:
                    if (_previewElement != null)
                    {
                        WorkspaceCanvas.Children.Remove(_previewElement);

                        var textBox = new TextEdit
                        {
                            Width = _previewElement.RenderSize.Width,
                            Height = _previewElement.RenderSize.Height,
                            BorderBrush = Brushes.LightBlue,
                            BorderThickness = new Thickness(1)
                        };

                        Canvas.SetLeft(textBox, Canvas.GetLeft(_previewElement));
                        Canvas.SetTop(textBox, Canvas.GetTop(_previewElement));

                        WorkspaceCanvas.Children.Add(textBox);
                        _previewElement = null;
                    }
                    break;
                case Tools.IVBarEditTool:
                    if (_previewElement != null)
                    {
                        WorkspaceCanvas.Children.Remove(_previewElement);

                        var textBox = new IVBar
                        {
                            Width = _previewElement.RenderSize.Width,
                            Height = _previewElement.RenderSize.Height,
                            BorderBrush = Brushes.LightBlue,
                            BorderThickness = new Thickness(1)
                        };

                        Canvas.SetLeft(textBox, Canvas.GetLeft(_previewElement));
                        Canvas.SetTop(textBox, Canvas.GetTop(_previewElement));

                        WorkspaceCanvas.Children.Add(textBox);
                        _previewElement = null;
                    }
                    break;
            }
        }
    }
}
