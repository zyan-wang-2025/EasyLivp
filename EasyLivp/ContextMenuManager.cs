using Microsoft.Win32;

namespace EasyLivp;

/// <summary>
/// 管理 .livp 文件的右键菜单。
/// 写入 HKEY_CURRENT_USER，不需要管理员权限。
/// 只有用户点击“注册右键菜单”按钮时才会真正写入。
/// </summary>
public static class ContextMenuManager
{
    // 右键菜单项在注册表中的位置
    private const string LivpExtractKey = @"Software\Classes\.livp\shell\EasyLivpExtract";

    /// <summary>
    /// 添加右键菜单项：“解压Live图”。
    /// </summary>
    public static void AddContextMenu(string exePath)
    {
        using var key = Registry.CurrentUser.CreateSubKey(LivpExtractKey);
        key.SetValue("", "解压Live图");       // 菜单显示的文字
        key.SetValue("Icon", exePath);          // 菜单左侧图标

        using var cmdKey = key.CreateSubKey("command");
        // %1 会被 Windows 替换为选中的文件路径
        cmdKey.SetValue("", $"\"{exePath}\" \"%1\" --extract");
    }

    /// <summary>
    /// 移除右键菜单项。
    /// </summary>
    public static void RemoveContextMenu()
    {
        try
        {
            Registry.CurrentUser.DeleteSubKeyTree(LivpExtractKey, throwOnMissingSubKey: false);
        }
        catch
        {
            // 忽略删除失败（例如键本来就不存在）
        }
    }

    /// <summary>
    /// 检查右键菜单是否已注册。
    /// </summary>
    public static bool IsContextMenuRegistered()
    {
        using var key = Registry.CurrentUser.OpenSubKey(LivpExtractKey);
        return key != null;
    }
}