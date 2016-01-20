@echo off

@echo  ==================ThorCyte Project Build ALL Slutions====================
cd %VS110COMNTOOLS%
call VsDevCmd.bat
@ echo Build in progress ... 

@ echo -- Start to Build ThorCyte.sln ... 
msbuild %~dp0\ThorCyte\GUI\Application\ThorCyte\ThorCyte.sln  >  %~dp0\BulidLog.txt || echo !!! Build ThorCyte.sln FAILED! Please check the BuildLog.txt!

@ echo -- Start to Build ImageProcess.sln ... 
msbuild %~dp0\ThorCyte\Common\ImageProcess\ImageProcess.sln >> %~dp0\BulidLog.txt || echo !!! Build ImageProcess.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build ComponentDataService.sln ... 
msbuild %~dp0\ThorCyte\Common\ComponentDataService\ComponentDataService.sln >> %~dp0\BulidLog.txt || echo !!! Build ComponentDataService.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build ROIService.sln ... 
msbuild %~dp0\ThorCyte\Common\ROIService\ROIService.sln >> %~dp0\BulidLog.txt || echo !!! Build ROIService.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build ProtocolModule.sln ... 
msbuild %~dp0\ThorCyte\GUI\Modules\ProtocolModule\ProtocolModule.sln >> %~dp0\BulidLog.txt || echo !!! Build ProtocolModule.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build ImageViewerModule.sln ... 
msbuild %~dp0\ThorCyte\GUI\Modules\ImageViewerModule\ImageViewerModule.sln >> %~dp0\BulidLog.txt || echo !!! Build ImageViewerModule.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build HeaderModule.sln ... 
msbuild %~dp0\ThorCyte\GUI\Modules\HeaderModule\HeaderModule.sln >> %~dp0\BulidLog.txt  || echo !!! Build HeaderModule.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build ExperimentDetailModule.sln ... 
msbuild %~dp0\ThorCyte\GUI\Modules\ExperimentDetailModule\ExperimentDetailModule.sln >> %~dp0\BulidLog.txt || echo !!! Build ExperimentDetailModule.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build CarrierModule.sln ... 
msbuild %~dp0\ThorCyte\GUI\Modules\CarrierModule\CarrierModule.sln >> %~dp0\BulidLog.txt || echo !!! Build CarrierModule.sln FAILED! Please check the BuildLog.txt! 

@ echo -- Start to Build ThorCyte.GraphicModule.sln ... 
msbuild %~dp0\ThorCyte\GUI\Modules\GraphicModule\ThorCyte.GraphicModule.sln >> %~dp0\BulidLog.txt || echo !!! Build ThorCyte.GraphicModule.sln FAILED! Please check the BuildLog.txt! 



@ echo Build All Done. 

pause