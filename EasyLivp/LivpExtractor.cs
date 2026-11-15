using System.IO.Compression;

namespace EasyLivp;

/// <summary>
/// .livp 解压逻辑。
/// .livp 本质是 ZIP，里面通常包含一个 .heic 和一个 .mov。
/// 规则：解压到与 livp 同名的文件夹；文件夹已存在则合并；同名文件冲突时询问用户。
/// </summary>
public static class LivpExtractor
{
    /// <summary>
    /// 解压单个 .livp 文件。
    /// </summary>
    /// <returns>解压出的 HEIC 和 MOV 路径（找不到则为 null）。</returns>
    public static (string? HeicPath, string? MovPath) Extract(string livpPath)
    {
        string fileName = Path.GetFileNameWithoutExtension(livpPath);
        string? parentDir = Path.GetDirectoryName(livpPath);
        if (parentDir == null) return (null, null);

        string targetDir = Path.Combine(parentDir, fileName);
        Directory.CreateDirectory(targetDir); // 已存在也不会报错，相当于“合并”

        string? heicPath = null;
        string? movPath = null;

        using var archive = ZipFile.OpenRead(livpPath);
        foreach (var entry in archive.Entries)
        {
            // 跳过目录条目（Name 为空）
            if (string.IsNullOrEmpty(entry.Name)) continue;

            string destPath = Path.Combine(targetDir, entry.FullName);

            // 冲突检测：同名文件已存在时，弹框询问是否覆盖
            if (File.Exists(destPath))
            {
                var result = MessageBox.Show(
                    $"文件已存在：\n{destPath}\n\n是否覆盖？",
                    "文件冲突",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    continue; // 跳过这个文件，继续处理下一个
            }

            entry.ExtractToFile(destPath, overwrite: true);

            // 记录 HEIC / MOV 路径，供预览使用
            string ext = Path.GetExtension(destPath).ToLowerInvariant();
            if (ext == ".heic") heicPath = destPath;
            else if (ext == ".mov") movPath = destPath;
        }

        return (heicPath, movPath);
    }
}