::=====================jenkins cmd=======================
::set OUTPATH=E:/FTP/%BUILD_NUMBER%
::call "E:\AraleEngine\AraleEngine\AraleEngine\unity.bat"
::=======================================================
echo off
tasklist|find /i "unity.exe" && echo build failed, unity is running && pause && exit 1
::set UNITY depend on your env
set UNITY="C:\Program Files\Unity\Editor\Unity.exe"
set BATPATH=%~dp0
set UNITYPROC="%BATPATH%"
set ANDROIDPROC=%BATPATH%\..\Android
set JENKINS=%WORKSPACE%
if not defined OUTPATH (
	::run unity.bat directly
	set OUTPATH=%BATPATH%\..\Publish
	set JENKINS=%BATPATH%
	set appVer=1.0.0
	set resVer=1
	set obb=false
)

set CONFIGFILE=%JENKINS%/configFile
set LOGPATH="%JENKINS%/unitylog.txt"
set ARGS=ARGS="{'publish':'%OUTPATH%','configFile':'%CONFIGFILE%','appVer':'%appVer%','resVer':'%resVer%','obb':'%obb%'}"
::export unity proc
echo on
%UNITY% -batchmode -quit -nographics -executeMethod BatchBuild.buildAndroid -projectPath %UNITYPROC% -logFile %LOGPATH% %ARGS%
::build apk
cd %ANDROIDPROC%
call gradlew assembleRelease
::copy apk to output
if not exist "%OUTPATH%" md "%OUTPATH%"
set DATETIME=%date:~0,4%%date:~5,2%%date:~8,2%_%time:~0,2%%time:~3,2%%time:~6,2%
set APKPATH=%OUTPATH%/release_%DATETIME%.apk
move %ANDROIDPROC%\app\build\outputs\apk\release\app-release.apk %APKPATH%
pause