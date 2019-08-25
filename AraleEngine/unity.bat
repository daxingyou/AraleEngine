::jenkins cmd
::set PUBLISH=E:/FTP/%BUILD_NUMBER%
::set LOGPATH="%WORKSPACE%/unitylog.txt"
::set ARGS="{'publish':'%PUBLISH%','configFile':'%WORKSPACE%/configFile','appVer':'%appVer%','resVer':'%resVer%','obb':'%obb%'}"
::set PUBLISH="%PUBLISH%"
::call "E:\AraleEngine\AraleEngine\AraleEngine\unity.bat" %LOGPATH% %ARGS% %PUBLISH%
echo off
tasklist|find /i "unity.exe" && echo build failed, unity is running && pause && exit 1
::set UNITY depend on your env
set UNITY="C:\Program Files\Unity\Editor\Unity.exe"
set BATPATH=%~dp0
set UNITYPROC="%BATPATH%"
set ANDROIDPROC=%BATPATH%/../Android
set LOGPATH=%1
set ARGS=%2
set OUTPATH=%3
::run unity.bat directly
if not defined LOGPATH set LOGPATH="unitylog.txt"
if not defined ARGS set ARGS="{}"
if not defined OUTPATH set OUTPATH=%BATPATH%\..\Publish
::export unity proc
echo on
%UNITY% -batchmode -quit -nographics -executeMethod BatchBuild.buildAndroid -projectPath %UNITYPROC% -logFile %LOGPATH% %ARGS%
::build apk
cd %ANDROIDPROC%
call gradlew assembleRelease
::copy result to output
if not exist %OUTPATH% md %OUTPATH%
set DATETIME=%date:~0,4%%date:~5,2%%date:~8,2%_%time:~0,2%%time:~3,2%%time:~6,2%
move %ANDROIDPROC%\app\build\outputs\apk\release\app-release.apk %OUTPATH%/release_%DATETIME%.apk
pause