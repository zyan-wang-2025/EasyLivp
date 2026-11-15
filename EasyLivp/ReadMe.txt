









关于卸载/删除


在本软件根目录下有一个名叫【ClearReg.bat】的文件，双击执行可清除此前写入的注册表（若有）。

注：该.bat文件的内容如下所示，如担心风险，可手动修改为.txt查看代码。

	@echo off
	reg delete "HKCU\Software\Classes\.livp\shell\EasyLivpExtract" /f
	reg delete "HKCU\Software\Classes\.livp" /f
	echo 右键菜单和默认打开方式已清理，现在你可以直接删除本文件夹了。
	pause

执行完毕，即表明EasyLivp在您的电脑上已无任何残留，直接将整个文件夹删除即可。