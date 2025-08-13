@echo off
chcp 65001 > nul

echo.
echo =======================================
echo    Unity 프로젝트 Git 연결 배치파일
echo =======================================
echo.

:: GitHub 사용자 이름
set githubUser=leedh1211

:: 템플릿 폴더 경로
set templatePath=E:\game\UnityGitTemplate

:: 현재 폴더 이름을 프로젝트 이름으로 설정
for %%I in (.) do set projectName=%%~nxI

echo 프로젝트 이름: %projectName%
echo GitHub 사용자: %githubUser%
echo.

:: GitHub 저장소 존재 여부 확인
git ls-remote https://github.com/%githubUser%/%projectName%.git > nul 2>&1
if %errorlevel% equ 0 (
    echo GitHub 레포지토리가 이미 존재합니다.
    set remoteUrl=https://github.com/%githubUser%/%projectName%.git
) else (
    echo GitHub에 "%projectName%" 레포지토리가 없습니다.
    set /p remoteUrl="직접 만든 레포지토리 주소를 입력하세요 (전체 URL): "
)

:: README 및 .gitignore 복사
if exist "%templatePath%\README.md" (
    copy "%templatePath%\README.md" "%cd%\README.md" > nul
    powershell -Command "(Get-Content 'README.md' -Encoding Default) -replace '프로젝트명', '%projectName%' | Set-Content 'README.md' -Encoding utf8"
    powershell -Command "(Get-Content 'README.md' -Encoding utf8) -replace '오늘', (Get-Date -Format 'yyyy-MM-dd') | Set-Content 'README.md' -Encoding utf8"
    echo README.md를 생성했습니다.
) else (
    echo README.md 템플릿이 없습니다. 확인하세요.
)

if exist "%templatePath%\.gitignore" (
    copy "%templatePath%\.gitignore" "%cd%\.gitignore" > nul
    echo .gitignore를 생성했습니다.
) else (
    echo .gitignore 템플릿이 없습니다. 확인하세요.
)

:: Git 초기화 및 원격 설정
git init
git branch -M main

:: 기존 origin 제거 (이미 있을 경우)
git remote remove origin > nul 2>&1

git remote add origin %remoteUrl%

:: 초기 커밋 및 푸시
git add .
git commit -m "Initial commit (%projectName%)"
git push -u origin main --force

echo.
echo =======================================
echo     Git 초기화 및 원격 연동 완료
echo =======================================
echo.

pause
