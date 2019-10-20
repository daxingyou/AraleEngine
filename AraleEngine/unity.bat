::=====================jenkins cmd=======================
::set OUTPATH=E:/FTP/%BUILD_NUMBER%
::call "E:\AraleEngine\AraleEngine\AraleEngine\unity.bat"
::=======================================================
echo off
tasklist|find /i "unity.exe" && echo build failed, unity is running && pause && exit 1
::set UNITY depend on your env
set UNITY="D:\YiYouClient\soft\Unity\Editor\Unity.exe"
set BATPATH=%~dp0
set UNITYPROC="%BATPATH%"
set ANDROIDPROC=%BATPATH%\..\Android
set JENKINS=%WORKSPACE%
if not defined OUTPATH (
	::run unity.bat directly
	set OUTPATH=%BATPATH%\..\Publish
	set JENKINS=%BATPATH%
	set version=1.0.1
	set obb=false
	set debug=false
)

set CONFIGFILE=%JENKINS%/ConfigFile
set LOGPATH="%JENKINS%/unitylog.txt"
set ARGS="{'publish':'%OUTPATH%','configFile':'%CONFIGFILE%','version':'%version%','obb':'%obb%','debug':'%debug%'}"
::export unity proc
echo on
%UNITY% -batchmode -quit -nographics -executeMethod BatchBuild.buildAndroid -projectPath %UNITYPROC% -logFile %LOGPATH% %ARGS%
::build apk
cd /d %ANDROIDPROC%
call gradlew assembleRelease
::copy apk to output
if not exist "%OUTPATH%" md "%OUTPATH%"
set DATETIME=%date:~0,4%%date:~5,2%%date:~8,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set APKPATH=%OUTPATH%/release_%DATETIME%.apk
move %ANDROIDPROC%\app\build\outputs\apk\release\app-release.apk %APKPATH%
::show download url
echo FTP=http://192.168.31.20:280/FTP/%BUILD_NUMBER%/
pause