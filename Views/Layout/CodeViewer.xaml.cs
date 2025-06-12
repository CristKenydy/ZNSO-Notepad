using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ZNSO_Notepad.Views.Layout
{
    public partial class CodeViewer : UserControl
    {
        private readonly string filePath;
        private readonly DispatcherTimer updateTimer;

        public CodeViewer(string filePath)
        {
            InitializeComponent();

            this.filePath = filePath;

            updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();

            // 初次加载
            LoadFileContent();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            LoadFileContent();
        }

        private void LoadFileContent()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    if (CodeTextBox.Text != content)
                    {
                        CodeTextBox.Text = content;
                    }
                }
                else
                {
                    CodeTextBox.Text = "⚠️ File not found: " + filePath;
                }
            }
            catch (Exception ex)
            {
                CodeTextBox.Text = "⚠️ Error reading file: " + ex.Message;
            }
        }
    }
}
