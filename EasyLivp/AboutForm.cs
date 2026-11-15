namespace EasyLivp;

public class AboutForm : Form
{
    public AboutForm()
    {
        Text = "关于 EasyLivp";
        Width = 520;
        Height = 450;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        
        // 👇👇👇 这里就是你可以随意改动的文本 👇👇👇
        string aboutText = 
@"EasyLivp - 苹果Live图预览与解压工具

版本：1.0.0
作者：Zyan
开源协议：MIT License

【功能简介】
1. 支持直接双击预览 .livp 格式的Live图（包含HEIC静态图和MOV动态视频）。
2. 支持解析并提供进度条、逐帧查看、音量控制等视频播放功能。
3. 提供右键菜单快捷解压功能，一键将.livp解压为同名文件夹。
4. 右侧提供同目录下的Livp文件列表，支持双击快速切换。

【使用说明】
- 双击或拖拽.livp文件到窗口即可预览。
- 点击菜单栏“工具”可注册/移除右键菜单。
- 点击菜单栏“工具”可引导设为默认打开方式。
- 点击“停止”按钮可清空预览并重置播放状态。

【致谢】
感谢 LibHeifSharp、LibVLCSharp 和 VideoLAN 开源库的支持。

【反馈与支持】
当前版本也许还存在诸多未发现的问题，欢迎随时与作者反馈。
作者的GitHub主页：https://github.com/zyan-wang-2025
电子邮箱：zy.wang.med@gmail.com

";


        // 👆👆👆 这里就是你可以随意改动的文本 👆👆👆

        // 使用 RichTextBox 以支持文字选中与复制
        var richTextBox = new RichTextBox
        {
            Dock = DockStyle.Fill,
            Text = aboutText,
            ReadOnly = true,
            BorderStyle = BorderStyle.None,
            Font = new Font("Microsoft YaHei", 10),
            BackColor = Color.White
        };

        // 用一个 Panel 包住它，用来制造内边距，看起来更舒服
        var panel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15) };
        panel.Controls.Add(richTextBox);

        // 底部的“确定”按钮
        var btnOk = new Button
        {
            Text = "确定",
            DialogResult = DialogResult.OK,
            Width = 80,
            Height = 30,
            Dock = DockStyle.Right
        };
        
        var bottomPanel = new Panel { Dock = DockStyle.Bottom, Height = 50, Padding = new Padding(0, 10, 20, 10) };
        bottomPanel.Controls.Add(btnOk);

        Controls.Add(panel);
        Controls.Add(bottomPanel);
        
        AcceptButton = btnOk;
        CancelButton = btnOk;
    }
}