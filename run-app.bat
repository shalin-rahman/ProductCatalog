@echo off
echo ==========================================
echo Starting Product Catalog Full-Stack App
echo ==========================================

echo [1/3] Stopping any existing services on required ports...

:: Kill processes on Angular port 4200
for /f "tokens=5" %%a in ('netstat -ano ^| findstr /R " :4200 "') do (
    if not "%%a"=="0" (
        taskkill /PID %%a /F >nul 2>&1
    )
)

:: Kill processes on .NET HTTP port 5190
for /f "tokens=5" %%a in ('netstat -ano ^| findstr /R " :5190 "') do (
    if not "%%a"=="0" (
        taskkill /PID %%a /F >nul 2>&1
    )
)

:: Kill processes on .NET HTTPS port 7098
for /f "tokens=5" %%a in ('netstat -ano ^| findstr /R " :7098 "') do (
    if not "%%a"=="0" (
        taskkill /PID %%a /F >nul 2>&1
    )
)

echo Old services stopped (if any were running).
timeout /t 2 /nobreak >nul

echo [2/3] Starting .NET 8 Backend API...
cd ProductCatalog.Api
start "Product Catalog API" cmd /k "dotnet run"
cd ..

echo [3/3] Starting Angular 19 UI...
cd product-catalog-ui
start "Product Catalog UI" cmd /k "npm start"
cd ..

echo ==========================================
echo Application is booting up in separate windows!
echo API: http://localhost:5190/swagger
echo     https://localhost:7098/swagger
echo UI:  http://localhost:4200
echo ==========================================
pause
