#!/bin/bash

# Exchange Rate Comparator - Quick Start Script
# This script helps evaluators quickly test the project

echo "🚀 Exchange Rate Comparator - Quick Start"
echo "=========================================="

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    exit 1
fi

# Check if Docker Compose is installed
if ! command -v docker-compose &> /dev/null; then
    echo "❌ Docker Compose is not installed. Please install Docker Compose first."
    exit 1
fi

echo "✅ Docker and Docker Compose are available"

# Build and start the application
echo "🔨 Building and starting the application..."
docker-compose up --build -d

# Wait for the application to start
echo "⏳ Waiting for the application to start..."
sleep 10

# Test health endpoint
echo "🏥 Testing health endpoint..."
curl -s http://localhost:8080/health | jq . || echo "Health check failed or jq not installed"

# Test the main API endpoint
echo "🧪 Testing the main API endpoint..."
curl -X POST http://localhost:8080/api/v1/best-rate \
  -H "Content-Type: application/json" \
  -d '{
    "sourceCurrency": "USD",
    "targetCurrency": "EUR",
    "amount": 1000.00
  }' | jq . || echo "API test failed or jq not installed"

echo ""
echo "🎉 Quick start completed!"
echo ""
echo "📋 Next steps:"
echo "1. Run tests: docker-compose exec exchange-comparator-api dotnet test"
echo "2. View logs: docker-compose logs -f"
echo "3. Stop application: docker-compose down"
echo ""
echo "📖 For detailed evaluation, see EVALUATION_GUIDE.md" 