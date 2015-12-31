@echo off

:: BatchGotAdmin 
:------------------------------------- 
REM --> Check for permissions 
>nul 2>&1 "%SYSTEMROOT%\system32\cacls.exe" "%SYSTEMROOT%\system32\config\system" 

REM --> If error flag set, we do not have admin. 
if '%errorlevel%' NEQ '0' ( 
echo Requesting administrative privileges... 
goto UACPrompt 
) else ( goto gotAdmin ) 

:UACPrompt 
echo Set UAC = CreateObject^("Shell.Application"^) > "%temp%\getadmin.vbs" 
echo UAC.ShellExecute "%~s0", "", "", "runas", 1 >> "%temp%\getadmin.vbs" 
"%temp%\getadmin.vbs" 
exit /B 

:gotAdmin 
if exist "%temp%\getadmin.vbs" ( del "%temp%\getadmin.vbs" ) 
pushd "%CD%" 
CD /D "%~dp0" 
:-------------------------------------- 

@echo  ==================ThorCyte Project Build ALL Slutions====================
cd %VS110COMNTOOLS%
call VsDevCmd.bat
@ echo Build in progress ... 

@ echo -- Start to Build in ThorCyte.sln ... 
msbuild %~dp0\ThorCyte\GUI\Application\ThorCyte\ThorCyte.sln  >  %~dp0\BulidLog.txt || echo !!! Build ThorCyte.sln FAILED! Please check the BuildLog.txt!

@ echo -- Start to Build in ImageProcess.sln ... 
msbuild %~dp0\ThorCyte\Common\ImageProcess\ImageProcess.sln >> %~dp0\BulidLog.txt || echo !!! Build ImageProcess.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build in ProtocolModule.sln ... 
msbuild %~dp0\ThorCyte\GUI\Modules\ProtocolModule\ProtocolModule.sln >> %~dp0\BulidLog.txt || echo !!! Build ProtocolModule.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build in ImageViewerModule.sln ... 
msbuild %~dp0\ThorCyte\GUI\Modules\ImageViewerModule\ImageViewerModule.sln >> %~dp0\BulidLog.txt || echo !!! Build ImageViewerModule.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build in HeaderModule.sln ... 
msbuild %~dp0\ThorCyte\GUI\Modules\HeaderModule\HeaderModule.sln >> %~dp0\BulidLog.txt  || echo !!! Build HeaderModule.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build in ExperimentDetailModule.sln ... 
msbuild %~dp0\ThorCyte\GUI\Modules\ExperimentDetailModule\ExperimentDetailModule.sln >> %~dp0\BulidLog.txt || echo !!! Build ExperimentDetailModule.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build CarrierModule.sln ... 
msbuild %~dp0\ThorCyte\GUI\Modules\CarrierModule\CarrierModule.sln >> %~dp0\BulidLog.txt || echo !!! Build CarrierModule.sln FAILED! Please check the BuildLog.txt! 

@ echo Build All Done. 

pause