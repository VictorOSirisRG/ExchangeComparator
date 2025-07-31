@echo off
REM Exchange Rate Comparator - Quick Start Script for Windows
REM This script helps evaluators quickly test the project

echo 🚀 Exchange Rate Comparator - Quick Start
echo ==========================================

REM Check if Docker is installed
docker --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker is not installed. Please install Docker Desktop first.
    pause
    exit /b 1
)

REM Check if Docker Compose is installed
docker-compose --version >nul 2>&1
if errorlevel 1 (
    echo ❌ Docker Compose is not installed. Please install Docker Compose first.
    pause
    exit /b 1
)

echo ✅ Docker and Docker Compose are available

REM Build and start the application
echo 🔨 Building and starting the application...
docker-compose up --build -d

REM Wait for the application to start
echo ⏳ Waiting for the application to start...
timeout /t 10 /nobreak >nul

REM Test health endpoint
echo 🏥 Testing health endpoint...
curl -s http://localhost:8080/health

REM Test the main API endpoint
echo 🧪 Testing the main API endpoint...
curl -X POST http://localhost:8080/api/v1/best-rate -H "Content-Type: application/json" -d "{\"sourceCurrency\":\"USD\",\"targetCurrency\":\"EUR\",\"amount\":1000.00}"

echo.
echo 🎉 Quick start completed!
echo.
echo 📋 Next steps:
echo 1. Run tests: docker-compose exec exchange-comparator-api dotnet test
echo 2. View logs: docker-compose logs -f
echo 3. Stop application: docker-compose down
echo.
echo 📖 For detailed evaluation, see EVALUATION_GUIDE.md
pause 