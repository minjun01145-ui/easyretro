@echo off
setlocal
pushd "%~dp0"
if not exist dist mkdir dist
"%WINDIR%\Microsoft.NET\Framework64\v4.0.30319\csc.exe" /nologo /target:winexe /platform:x64 /langversion:5 /optimize+ /codepage:65001 /warn:4 /warnaserror+ /out:dist\EasyRetro.exe /win32manifest:app.manifest /reference:System.dll /reference:System.Core.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.Web.Extensions.dll /recurse:src\*.cs
set "RESULT=%ERRORLEVEL%"
if "%RESULT%"=="0" echo Build succeeded: dist\EasyRetro.exe
popd
exit /b %RESULT%
