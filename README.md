# Exchange Rate Comparator

A robust .NET 9.0 solution for comparing exchange rates from multiple APIs and selecting the best deal for remittance offers.

## 🎯 Project Overview

This project implements a currency exchange rate comparator that:
- Queries multiple exchange rate APIs simultaneously
- Handles different API formats (JSON, XML)
- Selects the highest conversion rate automatically
- Continues operating even when some APIs are unavailable
- Provides comprehensive logging and error handling

## 🏗️ Architecture

The solution follows **Clean Architecture** principles with clear separation of concerns:

```
ExchangeComparator/
├── ExchangeComparator.Domain/          # Core business logic and interfaces
├── ExchangeComparator.Application/     # Application services and use cases
├── ExchangeComparator.Infrastructure/  # External API integrations
├── ExchangeComparator.API/             # Web API endpoints
└── ExchangeComparator.Tests/           # Comprehensive unit tests
```

### Key Design Principles

- **SOLID Principles**: Single responsibility, Open/closed, Liskov substitution, Interface segregation, Dependency inversion
- **Clean Architecture**: Domain-driven design with clear boundaries
- **Dependency Injection**: Loose coupling through interfaces
- **Fault Tolerance**: Graceful handling of provider failures
- **Performance**: Parallel API calls for optimal response times

## 🚀 Features

### ✅ Core Functionality
- **Multi-Provider Support**: Integrates with 3 different exchange rate APIs
- **Best Rate Selection**: Automatically selects the highest conversion rate
- **Parallel Processing**: Queries all providers simultaneously for optimal performance
- **Fault Tolerance**: Continues operation even when some providers fail

### ✅ API Providers
1. **FirstApiProvider**: JSON-based API with simple request/response format
2. **SecondApiProvider**: XML-based API with structured request/response
3. **ThirdApiProvider**: JSON-based API with complex nested response structure

### ✅ Error Handling & Resilience
- **Provider Failures**: Graceful degradation when APIs are unavailable
- **Invalid Responses**: Validation and error handling for malformed data
- **Exception Handling**: Comprehensive try-catch blocks throughout
- **Logging**: Structured logging for observability and debugging

### ✅ Testing
- **Unit Tests**: 100% coverage of core functionality
- **Mock-based Testing**: Isolated testing with Moq framework
- **Error Scenarios**: Tests for provider failures, invalid responses, and edge cases
- **Integration Tests**: End-to-end API testing

## 📋 Requirements Met

- ✅ **No UI Required**: Pure API-based solution
- ✅ **No SQL Required**: In-memory processing only
- ✅ **Unit Tested**: Comprehensive test coverage
- ✅ **Fault Tolerant**: Works with unavailable/invalid APIs
- ✅ **Clean Architecture**: SOLID principles and proper separation
- ✅ **Logging**: Structured logging throughout
- ✅ **Error Handling**: Graceful error management

## 🛠️ Technology Stack

- **.NET 9.0**: Latest .NET framework
- **ASP.NET Core**: Web API framework
- **Docker**: Containerization
- **Docker Compose**: Multi-container orchestration
- **Microsoft.Extensions.Logging**: Structured logging
- **Moq**: Mocking framework for testing
- **FluentAssertions**: Readable test assertions
- **xUnit**: Unit testing framework

## 🚀 Getting Started

### Prerequisites
- Docker and Docker Compose (recommended)
- .NET 9.0 SDK (for local development)
- Visual Studio 2022 or VS Code

### Quick Start with Docker (Recommended)

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ExchangeComparator
   ```

2. **Run with Docker Compose**
   ```bash
   docker-compose up --build
   ```

3. **Test the API**
   ```bash
   # Health check
   curl http://localhost:8080/health
   
   # Test the main endpoint
   curl -X POST http://localhost:8080/api/v1/best-rate \
     -H "Content-Type: application/json" \
     -d '{
       "sourceCurrency": "USD",
       "targetCurrency": "EUR",
       "amount": 1000.00
     }'
   ```

### Local Development

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd ExchangeComparator
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run tests**
   ```bash
   dotnet test
   ```

4. **Start the API**
   ```bash
   cd ExchangeComparator.API
   dotnet run
   ```

### API Usage

The API exposes a single endpoint for getting the best exchange rate:

**POST** `/api/v1/best-rate`

**Request Body:**
```json
{
  "sourceCurrency": "USD",
  "targetCurrency": "EUR", 
  "amount": 1000.00
}
```

**Response:**
```json
{
  "providerName": "SecondApiProvider",
  "rate": 0.85,
  "isSuccess": true,
  "errorMessage": null
}
```

## 🧪 Testing

### Running Tests
```bash
# Run all tests locally
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"

# Run specific test project
dotnet test ExchangeComparator.Tests/

# Run tests in Docker
docker-compose exec exchange-comparator-api dotnet test
```

### Test Coverage
- **Provider Tests**: Individual API provider testing
- **Service Tests**: Exchange rate service logic testing
- **Model Tests**: Domain model validation testing
- **Integration Tests**: End-to-end API testing

## 📊 Performance

- **Parallel Processing**: All providers queried simultaneously
- **Timeout Handling**: Configurable timeouts for API calls
- **Caching Ready**: Architecture supports future caching implementation
- **Scalable**: Easy to add new providers

## 🔧 Configuration

### API Endpoints
Configure provider base URLs in `Program.cs`:
```csharp
builder.Services.AddHttpClient<FirstApiProvider>(client =>
{
    client.BaseAddress = new Uri("https://api1.example.com");
});
```

### Docker Configuration
The application is containerized with:
- **Dockerfile**: Multi-stage build for optimized production image
- **docker-compose.yml**: Easy development and testing setup
- **Health checks**: Automatic health monitoring
- **Security**: Non-root user execution

### Logging
Logging is configured in `Program.cs` and provides:
- Request/response logging
- Performance metrics
- Error tracking
- Provider status monitoring

## 🏛️ Project Structure

```
ExchangeComparator/
├── ExchangeComparator.Domain/
│   ├── Interfaces/
│   │   └── IExchangeRateProvider.cs
│   └── Models/
│       ├── ExchangeRateRequest.cs
│       └── ExchangeRateResponse.cs
├── ExchangeComparator.Application/
│   └── Services/
│       └── ExchangeRateService.cs
├── ExchangeComparator.Infrastructure/
│   └── Providers/
│       ├── FirstApiProvider.cs
│       ├── SecondApiProvider.cs
│       └── ThirdApiProvider.cs
├── ExchangeComparator.API/
│   └── Program.cs
└── ExchangeComparator.Tests/
    ├── Models/
    ├── Providers/
    └── Services/
```

## 🔄 Process Flow

1. **Input**: Single request with source currency, target currency, and amount
2. **Processing**: 
   - Parallel API calls to all providers
   - Response validation and parsing
   - Rate comparison and selection
3. **Output**: Best rate with provider information

## 🛡️ Error Handling Strategy

- **Provider Failures**: Individual providers fail gracefully without affecting others
- **Invalid Responses**: JSON/XML parsing errors are caught and logged
- **Network Issues**: HTTP exceptions are handled with retry logic
- **Timeout Handling**: Configurable timeouts prevent hanging requests

## 📈 Monitoring & Observability

- **Structured Logging**: JSON-formatted logs with correlation IDs
- **Performance Metrics**: Response time tracking for each provider
- **Error Tracking**: Detailed error information for debugging
- **Health Checks**: Provider availability monitoring

## 🔮 Future Enhancements

- **Caching**: Redis-based caching for frequently requested rates
- **Rate Limiting**: API rate limiting and throttling
- **Circuit Breaker**: Advanced fault tolerance patterns
- **Metrics**: Prometheus/Grafana integration
- **Configuration**: External configuration management
- **Authentication**: API key management for providers

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add/update tests
5. Submit a pull request

## 📞 Support

For questions or issues, please create an issue in the repository.
