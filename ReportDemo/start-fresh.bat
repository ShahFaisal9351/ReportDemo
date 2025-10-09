@echo off
echo ================================================================
echo            🚀 EduManage - Fresh Startup Script
echo ================================================================
echo.

echo 🧹 Cleaning up any running processes...
taskkill /F /IM "ReportDemo.exe" >nul 2>&1
taskkill /F /IM "dotnet.exe" /FI "WINDOWTITLE eq *ReportDemo*" >nul 2>&1

echo 🔨 Building the application...
dotnet build --configuration Release

echo 🚀 Starting application with guaranteed login page...
echo.
echo ✅ The application will ALWAYS show the login page first!
echo 🌐 Open your browser and navigate to: http://localhost:5000
echo 🔑 You'll see the login page every time you run this script
echo.
echo Press Ctrl+C to stop the application
echo ================================================================
echo.

dotnet run