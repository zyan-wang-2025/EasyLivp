using System.Runtime.InteropServices;

namespace EasyLivp;

internal static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // 1. 关键：把 Support 文件夹加入环境变量，确保 libheif.dll 等能被找到
        string supportDir = Path.Combine(AppContext.BaseDirectory, "Support");
        if (Directory.Exists(supportDir))
        {
            string path = Environment.GetEnvironmentVariable("PATH") ?? "";
            Environment.SetEnvironmentVariable("PATH", supportDir + ";" + path);
        }

        // 2. 捕获全局异常，防止双击没反应（闪退）
        Application.ThreadException += (s, e) => 
            MessageBox.Show($"程序发生错误：\n{e.Exception.Message}\n\n{e.Exception.StackTrace}", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            var ex = e.ExceptionObject as Exception;
            MessageBox.Show($"程序发生致命错误：\n{ex?.Message}\n\n{ex?.StackTrace}", "严重错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
        };

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm(args));
    }
}