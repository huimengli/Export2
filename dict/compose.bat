@echo off
setlocal

:: 检查 %cd% 路径是否包含 dict\ 目录
echo "%cd%\" | findstr /i "\\dict\\" >nul
if %errorlevel% neq 0 (
    echo 当前路径中未找到 dict\ 目录
    exit /b 1
)

:: 定义目标 ZIP 文件名
set "zipfile=Export.zip"

:: 删除已存在的 ZIP 文件（避免追加模式干扰）
if exist "%zipfile%" del "%zipfile%"

:: 调用 PowerShell 压缩指定文件
powershell -Command "Compress-Archive -Path 'Core.dll', 'Runtime.dll', 'EditorEX.dll', 'Core.pdb', 'Runtime.pdb', 'EditorEX.pdb' -DestinationPath '%zipfile%' -Force"

echo 压缩完成：%zipfile%

del /S /Q *.dll
del /S /Q *.pdb

echo dll文件和pdb文件已删除

pause
