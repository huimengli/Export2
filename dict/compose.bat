@echo off
setlocal

:: ����Ŀ�� ZIP �ļ���
set "zipfile=Export.zip"

:: ɾ���Ѵ��ڵ� ZIP �ļ�������׷��ģʽ���ţ�
if exist "%zipfile%" del "%zipfile%"

:: ���� PowerShell ѹ��ָ���ļ�
powershell -Command "Compress-Archive -Path 'Core.dll', 'Runtime.dll', 'EditorEX.dll' -DestinationPath '%zipfile%' -Force"

echo ѹ����ɣ�%zipfile%

del /S /Q *.dll

echo dll�ļ���ɾ��

pause
