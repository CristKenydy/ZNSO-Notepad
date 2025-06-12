using System.Configuration;
using System.Data;
using System.Windows;

namespace ZNSO_Notepad;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static MainWindow znpEditor;
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        string? SelectionZnpFile = null;

        // 遍历命令行参数
        foreach (var arg in e.Args)
        {
            if (arg.EndsWith(".znp", StringComparison.OrdinalIgnoreCase))
            {
                SelectionZnpFile = arg; // 获取双击的 znp 文件路径
                break; // 停止遍历，因为只需要第一个文件
            }
        }

        znpEditor = new MainWindow();

        // 如果有选中的 znp 文件，加载它
        if (SelectionZnpFile != null)
        {
            znpEditor.Load(SelectionZnpFile);
        }

        znpEditor.Show();
    }
}

