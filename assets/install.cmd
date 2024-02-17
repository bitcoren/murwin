cd /D %0%\..\..
set MURZILLA=%cd%
mkdir temp data apps
powershell -Command "Start-Process cmd -Verb RunAs -ArgumentList '/c setx -m MURZILLA %cd% & setx -m PATH \"%cd%\assets;%PATH%\"'"
powershell -Command "Start-Process powershell -Verb RunAs -ArgumentList '& {Set-ExecutionPolicy -ExecutionPolicy Unrestricted -Force};'"
set PATH=%PATH%;%cd%\assets
start /w assets\windowsdesktop-runtime-8.0.2-win-x64.exe /install /quiet /norestart
start /w assets\MicrosoftEdgeWebview2Setup.exe /silent /install
start /w assets\Firebird-5.0.0.1306-0-windows-x64.exe /SP- /SILENT /SUPPRESSMSGBOXES /NOCANCEL /NORESTART /SYSDBAPASSWORD="murzilla" /FORCE
start /w assets\IPFS-Desktop-Setup-0.33.0.exe
copy assets\mstart.cmd "C:\Users\%USERNAME%\AppData\Roaming\Microsoft\Windows\Start Menu\Programs\Startup\mstart.cmd"
rd /s /q temp
mkdir temp
shutdown /r /f /t 60
