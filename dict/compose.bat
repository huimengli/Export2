@echo off
setlocal

:: ����Ŀ�� ZIP �ļ���
set "zipfile=Export.zip"

:: ɾ���Ѵ��ڵ� ZIP �ļ�������׷��ģʽ���ţ�
if exist "%zipfile%" del "%zipfile%"

:: ���� PowerShell ѹ��ָ���ļ�
powershell -Command "Compress-Archive -Path 'Core.dll', 'Runtime.dll', 'EditorEX.dll', 'Core.pdb', 'Runtime.pdb', 'EditorEX.pdb' -DestinationPath '%zipfile%' -Force"

echo ѹ����ɣ�%zipfile%

del /S /Q *.dll
del /S /Q *.pdb

echo dll�ļ���pdb�ļ���ɾ��

pause
