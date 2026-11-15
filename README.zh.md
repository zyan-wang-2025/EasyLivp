# EasyLivp
[English](./README.md) | **汉语**

一个用于预览、浏览和解压苹果 iOS .livp 格式实况照片的便捷工具。

**EasyLivp** 是一款专为Windows平台打造的轻量级小工具，旨在解决Windows用户查看和提取苹果设备拍摄的Live图（.livp 格式）的痛点。
无需复杂的操作，双击即可像常规图片查看器一样预览 Live 图，并支持一键解压。

软件界面：
<img width="1179" height="840" alt="image" src="https://github.com/user-attachments/assets/3388868e-928b-46eb-aee7-1c75746c13bd" />

发布于Release V1.0的压缩包中内容：
<img width="266" height="167" alt="image" src="https://github.com/user-attachments/assets/9a670540-e6c4-40bb-8a3f-2e6909540d90" />


##  主要功能

-  **原生预览**：直接在软件内查看 .livp 文件（静态 HEIC 图片 + 动态 MOV 视频），支持完美还原 Live 图效果。
-  **视频控制**：提供完整的播放/暂停、上一帧、下一帧、进度条拖拽以及音量调节和静音功能。
-  **快捷解压**：支持将 .livp 文件一键解压为同名文件夹，内含独立的 .heic 和 .mov 文件。
-  **右键菜单**：可将“解压 Live 图”注册到 Windows 资源管理器的右键菜单中，操作更快捷。
-  **默认关联**：支持半自动设置为 .livp 文件的默认打开方式。
-  **目录浏览**：右侧自带文件列表，自动展示当前目录下所有的 .livp 文件，双击即可无缝切换预览。

##  如何使用

1. **直接预览**：双击 `EasyLivp.exe` 打开软件，可通过“文件 -> 打开”选择 .livp 文件，或直接将文件**拖拽**到软件窗口内。
2. **播放控制**：打开文件后会自动播放。下方进度条可拖动，按钮可操作上下帧。播放结束后，再次点击“播放”即可从头重播。
3. **注册右键菜单**：点击软件顶部菜单栏的 **“工具” -> “注册右键菜单”**。之后在资源管理器中对 .livp 文件右键，即可看到“解压 Live 图”选项。
4. **设为默认程序**：点击 **“工具” -> “设为默认打开方式”**，随后在弹出的 Windows 设置中，手动将 `.livp` 格式关联到本软件即可实现双击秒开。

##  如何卸载

本软件为绿色免安装版。
1. 如果你想清理系统关联，请双击运行压缩包内的 **`清理右键菜单.bat`**，这会自动清除注册表中的相关键值。
2. 删除本软件所在的整个文件夹即可完成卸载。

##  开发与致谢

本项目基于 .NET 8 (WinForms) 开发，并使用了以下优秀的开源库：
- [LibHeifSharp](https://github.com/0xC0000054/libheif-sharp)：提供 HEIC 图像的解码支持。
- [LibVLCSharp](https://github.com/videolan/libvlcsharp) & [VideoLAN](https://www.videolan.org/)：提供强大的视频播放与解析能力。

##  开源协议

本项目采用 [MIT License](LICENSE) 协议开源。
作者：Zyan
