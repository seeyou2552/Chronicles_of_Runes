@echo off
setlocal enabledelayedexpansion

:: 설정
set "ROOT=Assets\06_Sounds"
set "OUTPUT=wav_list.csv"

:: 절대 경로 획득
pushd "%ROOT%"
set "ABS_ROOT=%CD%"
popd

:: CSV 헤더 작성
echo folder_path,file_name,full_path,relative_path > %OUTPUT%

:: .wav 파일 검색
for /R "%ROOT%" %%F in (*.wav) do (
    set "FULL=%%F"
    set "FILE=%%~nxF"
    set "FOLDER=%%~dpF"
    set "FOLDER=!FOLDER:~0,-1!"  :: 마지막 \ 제거

    :: 상대 경로 계산
    set "RELATIVE=!FOLDER:%ABS_ROOT%=!"
    if "!RELATIVE!"=="" (
        set "RELATIVE=."
    )

    :: 경로 정규화 (\ → /)
    set "RELATIVE=!RELATIVE:\=/!"
    set "FULL_PATH=%%F"
    set "FULL_PATH=!FULL_PATH:\=/!"

    :: 맨 앞 '/' 제거
    if "!RELATIVE:~0,1!"=="/" (
        set "RELATIVE=!RELATIVE:~1!"
    )

    :: folder_path/file_name 조합
    if "!RELATIVE!"=="." (
        set "RELATIVE_PATH=!FILE!"
    ) else (
        set "RELATIVE_PATH=!RELATIVE!/!FILE!"
    )

    :: CSV 행 작성
    echo !RELATIVE!,!FILE!,!FULL_PATH!,!RELATIVE_PATH!>> %OUTPUT%
)

echo.
echo [완료] %OUTPUT% 생성됨
pause
