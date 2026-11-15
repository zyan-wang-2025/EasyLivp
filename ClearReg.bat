@echo off
reg delete "HKCU\Software\Classes\.livp\shell\EasyLivpExtract" /f
reg delete "HKCU\Software\Classes\.livp" /f
echo 右键菜单和默认打开方式已清理，现在你可以直接删除本文件夹了。
pause