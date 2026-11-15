using Microsoft.Win32;

namespace EasyLivp;

/// <summary>
/// 默认打开方式管理（半自动方案）。
/// 工具负责在注册表中声明 ProgID，让程序出现在“打开方式”列表里；
/// 最后一步“设为默认”由用户在 Windows 设置中手动确认。
/// </summary>
public static class DefaultAppManager
{
    private const string ProgId = "EasyLivp.LivpFile";
    private const string LivpClassKey = @"Software\Classes\.livp";
    private const string ProgIdKey = @"Software\Classes\" + ProgId;

    /// <summary>
    /// 注册 ProgID，并让 .livp 出现在“打开方式”列表中。
    /// 不直接强制设为默认（Windows 10/11 不允许静默修改默认应用）。
    /// </summary>
    public static void RegisterProgId(string exePath)
    {
        // 1. 声明 ProgID 及打开命令
        using (var openCmdKey = Registry.CurrentUser.CreateSubKey(ProgIdKey + @"\shell\open\command"))
        {
            openCmdKey.SetValue("", $"\"{exePath}\" \"%1\"");
        }

        // 2. 让 .livp 的默认值指向我们的 ProgID
        using (var livpKey = Registry.CurrentUser.CreateSubKey(LivpClassKey))
        {
            livpKey.SetValue("", ProgId);
        }
    }

    /// <summary>
    /// 打开 Windows“默认应用”设置页，引导用户完成最后一步。
    /// </summary>
    public static void OpenDefaultAppsSettings()
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = "ms-settings:defaultapps",
            UseShellExecute = true
        });
    }
}