@echo off 
setlocal enabledelayedexpansion
cd %~dp0
del /q %1_out
mkdir %1_out
set /a ind=1
for /r "%1" %%i in (*.mp4) do (
 	ffmpeg -i "%%i" -c copy -map 0 -f segment -segment_list "%1_out\%%~ni.m3u8" -segment_time 5 %1_out\ts_!ind!_%%3d.ts
	ffmpeg -i "%%i" -ss 00:00:06 -vframes 1 "%1_out\%%~ni.jpg"
    set /a ind+=1
)
