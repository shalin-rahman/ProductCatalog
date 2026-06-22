@echo off
echo ==========================================
echo Starting Product Catalog Full-Stack App
echo ==========================================

echo [1/3] Stopping any existing services on required ports...

:: Kill processes on Angular port 4200
FOR /F "tokens=5" %%T IN ('netstat -a -n -o ^| findstr :4200') DO (
    IF NOT "%%T"=="0" taskkill /PID %%T /F 2>nul
)
:: Kill processes on .NET HTTP port 5190
FOR /F "tokens=5" %%T IN ('netstat -a -n -o ^| findstr :5190') DO (
    IF NOT "%%T"=="0" taskkill /PID %%T /F 2>nul
)
:: Kill processes on .NET HTTPS port 7001
FOR /F "tokens=5" %%T IN ('netstat -a -n -o ^| findstr :7001') DO (
    IF NOT "%%T"=="0" taskkill /PID %%T /F 2>nul
)
echo Old services stopped (if any were running).

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
echo API: https://localhost:7001/swagger
echo UI:  http://localhost:4200
echo ==========================================
pause
