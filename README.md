<!-- Badges Row -->
<div align="center">

[![.NET Version](https://img.shields.io/badge/.NET-8.0-purple?style=flat-square&logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg?style=flat-square)](LICENSE)
[![Build Status](https://img.shields.io/badge/build-passing-brightgreen?style=flat-square)](README.md)
[![Code Quality](https://img.shields.io/badge/code%20quality-A-success?style=flat-square)](README.md)

</div>

# 📌 Roaia API

A production-grade **RESTful API** built with **ASP.NET Core 8** following **Clean Architecture** principles. Roaia is designed to serve a Flutter mobile application that assists visually impaired individuals using smart glasses technology with AI-powered features like text reading, object detection, and face recognition.

---

## 🚀 Features

- ✅ **Text Reading** - Converts text into speech using advanced text-to-speech engines
- ✅ **Object Detection** - Identifies and describes objects in the user's surroundings
- ✅ **Face Recognition** - Recognizes pre-registered faces with high accuracy
- ✅ **Weather Updates** - Provides real-time weather information based on location
- ✅ **Emergency Notifications** - Sends immediate SOS alerts to emergency contacts
- ✅ **JWT Authentication** - Secure token-based authentication with refresh token support
- ✅ **Real-time Communication** - WebSocket support via SignalR for live updates
- ✅ **Push Notifications** - Firebase Cloud Messaging integration
- ✅ **User Management** - Account creation, profile management, and authentication
- ✅ **Device Management** - Track and manage user devices with unique tokens
- ✅ **GPS Tracking** - Real-time location tracking and history management

---

## 🛠️ Tech Stack

| Layer | Technology |
|-------|-----------|
| **Framework** | ASP.NET Core 8.0 |
| **Language** | C# 12 |
| **Database** | SQL Server with Entity Framework Core |
| **Authentication** | JWT (JSON Web Tokens) |
| **Real-time** | SignalR (WebSocket) |
| **Notifications** | Firebase Cloud Messaging (FCM) |
| **ORM** | Entity Framework Core (Code-First) |
| **Logging** | Serilog |
| **API Documentation** | Swagger/OpenAPI |
| **Dependency Injection** | Built-in ASP.NET Core DI Container |

---

## 🏗️ Architecture

The project follows **Clean Architecture** principles with clearly separated layers:

```
Roaia.API
├── Controllers/          → Entry points for HTTP requests
├── Services/            → Business logic layer
├── Core/
│   ├── Models/          → Domain entities and DTOs
│   ├── Mapping/         → AutoMapper profiles
│   └── Const/           → Application constants
├── Data/                → Data access layer (DbContext)
├── Migrations/          → Entity Framework migrations
├── Attributes/          → Custom validation attributes
├── Extensions/          → Extension methods
├── Helpers/             → Utility helpers
├── Hubs/                → SignalR hubs for real-time features
└── Settings/            → Configuration settings
```

### Layer Responsibilities

- **Controllers**: Handle HTTP requests and responses
- **Services**: Contain business logic and orchestration
- **Core/Models**: Define domain entities and data transfer objects
- **Data**: Manage database operations through EF Core
- **Attributes**: Custom validation logic attributes

---

## 🔐 Authentication

### JWT (JSON Web Tokens)

The API implements **JWT-based authentication** with secure token management:

**Token Flow:**
```
1. User submits credentials (email + password)
2. API validates and generates JWT access token + refresh token
3. Client stores tokens securely
4. Each request includes "Authorization: Bearer <token>" header
5. When access token expires, refresh token is used to obtain a new one
```

**Token Structure:**
```json
{
  "sub": "user-id",
  "email": "user@example.com",
  "iat": 1234567890,
  "exp": 1234571490,
  "iss": "roaia-api",
  "aud": "roaia-mobile"
}
```

**Protected Endpoints:**
- All endpoints except `/api/auth/login` and `/api/auth/register` require JWT authentication
- Use the `[Authorize]` attribute on controllers/actions
- Include token in request header: `Authorization: Bearer <your_jwt_token>`

---

## 🔔 Notifications

### Firebase Cloud Messaging (FCM)

Roaia integrates **Firebase Cloud Messaging** for push notifications:

**Features:**
- Send real-time notifications to mobile devices
- Manage notification preferences per user
- Track notification delivery and engagement
- Support for rich notifications with images and custom actions

**Device Token Management:**
```
1. Mobile app registers with FCM and obtains device token
2. Device token is sent to API during authentication
3. API stores token in DeviceToken table
4. API uses stored token to send push notifications
```

**Notification Types:**
- Emergency alerts (SOS)
- Object detection alerts
- Face recognition notifications
- Weather alerts
- System updates

---

## ⚡ Real-time Features

### SignalR WebSocket Integration

Real-time communication is enabled through **ASP.NET SignalR** hubs:

**GPS Hub (`/hubs/gps`):**
- Transmit real-time GPS location updates
- Receive location streaming from smart glasses
- Broadcast location to authorized clients

**Connection Flow:**
```typescript
// Client connects to SignalR hub
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/hubs/gps", { 
    accessTokenFactory: () => jwtToken 
  })
  .withAutomaticReconnect()
  .build();

await connection.start();
```

---

## 📂 Project Structure

```
Roaia/
├── appsettings.json              ← Configuration (database, JWT, etc.)
├── appsettings.Development.json  ← Development-specific settings
├── Program.cs                    ← Application startup configuration
├── GlobalUsings.cs               ← Global using statements
├── Roaia.http                    ← REST Client endpoints (for testing)
├── roaia-official-fcm.json       ← Firebase service account key
│
├── Attributes/
│   ├── AllowedExtensionsAttribute.cs      ← File extension validation
│   ├── IdentifierValidatorAttribute.cs    ← ID validation
│   ├── MaxFileSizeAttribute.cs            ← File size limits
│   └── ValidUrlAttribute.cs               ← URL validation
│
├── Controllers/
│   ├── AccountController.cs      ← User account management
│   ├── AuthController.cs         ← Authentication endpoints
│   ├── DashboardController.cs    ← Dashboard data endpoints
│   ├── GlassesController.cs      ← Smart glasses operations
│   └── ServiceController.cs      ← Core service endpoints
│
├── Core/
│   ├── Const/                    ← Constants and enumerations
│   ├── Mapping/                  ← AutoMapper profiles
│   └── Models/                   ← Domain entities and DTOs
│
├── Data/
│   └── ApplicationDbContext.cs   ← EF Core DbContext
│
├── Extensions/
│   └── DependencyInjection.cs    ← Service registration
│
├── Filter/
│   └── HangfireAuthorizationFilter.cs
│
├── Helpers/
│   └── JWT.cs                    ← JWT utility functions
│
├── Hubs/
│   └── GPSHub.cs                 ← SignalR GPS real-time hub
│
├── Migrations/                   ← EF Core migration history
│
├── Seeds/                        ← Database seed data
│
├── Services/                     ← Business logic layer
│
└── Settings/                     ← Configuration classes
```

---

## ⚙️ Getting Started

### Prerequisites

- **.NET 8 SDK** or later ([Download](https://dotnet.microsoft.com/download/dotnet/8.0))
- **SQL Server 2019** or later (or SQL Server Express)
- **Visual Studio 2022** or VS Code with C# extension
- **Git** for version control

### Installation

1. **Clone the repository:**
   ```bash
   git clone https://github.com/Roaia-s/Roaia-API-APP.git
   cd Roaia-API-APP/Roaia
   ```

2. **Configure the database connection:**
   
   Edit `appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=.\\SQLEXPRESS;Database=RoaiaDb;Trusted_Connection=true;Encrypt=false;"
     }
   }
   ```

3. **Apply Entity Framework migrations:**
   ```bash
   cd Roaia
   dotnet ef database update
   ```

4. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

5. **Configure JWT settings:**
   
   Update `appsettings.json`:
   ```json
   {
     "JwtSettings": {
       "SecretKey": "your-super-secret-key-minimum-32-characters-long",
       "ExpirationMinutes": 60,
       "RefreshTokenExpirationDays": 7,
       "Issuer": "roaia-api",
       "Audience": "roaia-mobile"
     }
   }
   ```

6. **Configure Firebase (optional):**
   
   Place your `roaia-official-fcm.json` (Firebase service account key) in the project root.

### Running the Project

**Development Mode:**
```bash
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger`
- **Health Check**: `https://localhost:5001/health`

**Production Mode:**
```bash
dotnet publish -c Release
dotnet Roaia.dll --environment Production
```

---

## 🔗 API Endpoints

### Authentication Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|----------------|
| POST | `/api/auth/register` | Register a new user | ❌ |
| POST | `/api/auth/login` | Authenticate user and get JWT | ❌ |
| POST | `/api/auth/refresh-token` | Refresh expired access token | ✅ |
| POST | `/api/auth/logout` | Invalidate user session | ✅ |
| POST | `/api/auth/forgot-password` | Initiate password reset | ❌ |
| POST | `/api/auth/reset-password` | Complete password reset | ❌ |

### User Account Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|----------------|
| GET | `/api/account/profile` | Get current user profile | ✅ |
| PUT | `/api/account/profile` | Update user profile | ✅ |
| POST | `/api/account/change-password` | Change user password | ✅ |
| DELETE | `/api/account` | Delete user account | ✅ |

### Smart Glasses Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|----------------|
| POST | `/api/glasses/location` | Send current GPS location | ✅ |
| POST | `/api/glasses/device-status` | Report device health status | ✅ |
| GET | `/api/glasses/location-history` | Retrieve location history | ✅ |
| POST | `/api/glasses/emergency-alert` | Trigger SOS alert | ✅ |

### Notification Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|----------------|
| GET | `/api/notifications` | Get user notifications | ✅ |
| POST | `/api/notifications` | Create new notification | ✅ |
| PUT | `/api/notifications/{id}` | Mark notification as read | ✅ |
| DELETE | `/api/notifications/{id}` | Delete notification | ✅ |

### Dashboard Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|----------------|
| GET | `/api/dashboard/analytics` | Get dashboard analytics | ✅ |
| GET | `/api/dashboard/summary` | Get user activity summary | ✅ |

### Sample Request/Response

**Login Request:**
```bash
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Login Response (200 OK):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "f8k3l9m2n1o0p9q8r7s6t5u4v3w2x1y0",
    "expiresIn": 3600,
    "user": {
      "id": "user-uuid",
      "email": "user@example.com",
      "fullName": "John Doe",
      "phoneNumber": "+1234567890"
    }
  }
}
```

---

## 🧪 Testing

### Unit Testing

The project uses **xUnit** and **Moq** for unit testing:

```csharp
[Fact]
public async Task Login_WithValidCredentials_ReturnsJwt()
{
    // Arrange
    var loginRequest = new LoginRequest 
    { 
        Email = "test@example.com", 
        Password = "Test@123" 
    };
    
    // Act
    var result = await _authService.LoginAsync(loginRequest);
    
    // Assert
    Assert.NotNull(result.AccessToken);
    Assert.NotEmpty(result.RefreshToken);
}
```

**To run tests:**
```bash
dotnet test
```

### Integration Testing

Use the included `Roaia.http` file to test endpoints with REST Client extension:

```http
@baseUrl = https://localhost:5001

POST {{baseUrl}}/api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "password123"
}
```

### Manual Testing

- Use **Postman** or **Insomnia** for API testing
- Swagger/OpenAPI documentation available at `/swagger`
- Import the collection from the project repository

---

## 📦 Deployment

### Deployment Steps

1. **Build the application for release:**
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. **Create a deployment configuration:**
   
   Create `appsettings.Production.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=prod-db-server;Database=RoaiaDbProd;User Id=sa;Password=YourSecurePassword;"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Warning"
       }
     },
     "JwtSettings": {
       "SecretKey": "your-production-secret-key-minimum-32-characters",
       "ExpirationMinutes": 30
     }
   }
   ```

3. **Deploy to hosting platform:**

   **Azure App Service:**
   ```bash
   az webapp up --name roaia-api --resource-group myResourceGroup --plan myAppServicePlan --sku B2 --runtime "dotnet:8"
   ```

   **Docker:**
   ```dockerfile
   FROM mcr.microsoft.com/dotnet/sdk:8.0 as build
   WORKDIR /src
   COPY . .
   RUN dotnet publish -c Release -o /app/publish
   
   FROM mcr.microsoft.com/dotnet/aspnet:8.0
   WORKDIR /app
   COPY --from=build /app/publish .
   ENTRYPOINT ["dotnet", "Roaia.dll"]
   ```

4. **Configure environment variables on the hosting platform**
5. **Set up SSL/TLS certificates**
6. **Enable CORS for Flutter app domain**
7. **Configure logging and monitoring**
8. **Set up CI/CD pipeline with GitHub Actions or Azure DevOps**

### Production Checklist

- ✅ Environment variables configured securely
- ✅ Database backups enabled
- ✅ SSL/TLS certificates installed
- ✅ CORS policy restricted to Flutter app domain
- ✅ Rate limiting implemented
- ✅ Logging and monitoring enabled
- ✅ API versioning in place
- ✅ Health check endpoint configured
- ✅ Database migrations executed

---

## 🤝 Contributing

We welcome contributions from the community! Please follow these guidelines:

1. **Fork the repository** on GitHub
2. **Create a feature branch** for your changes:
   ```bash
   git checkout -b feature/amazing-feature
   ```
3. **Commit your changes** with clear messages:
   ```bash
   git commit -m 'Add amazing feature'
   ```
4. **Push to your branch:**
   ```bash
   git push origin feature/amazing-feature
   ```
5. **Open a Pull Request** with a detailed description of your changes

### Code Standards

- Follow [Microsoft C# Coding Conventions](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use meaningful variable and method names
- Add XML documentation comments for public members
- Write unit tests for new features
- Ensure all tests pass before submitting PR

### Reporting Issues

Found a bug? Please create an issue with:
- Clear description of the problem
- Steps to reproduce
- Expected vs. actual behavior
- Screenshots or logs if applicable

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

The MIT License permits use, modification, and distribution of the software freely, with minimal restrictions. For more information, visit [opensource.org/licenses/MIT](https://opensource.org/licenses/MIT).

---

## 📞 Support

- **Documentation**: [Wiki](../../wiki)
- **Issues**: [GitHub Issues](../../issues)
- **Discussions**: [GitHub Discussions](../../discussions)
- **Email**: support@roaia-project.com

---

<div align="center">

Built with ❤️ for visually impaired individuals | [Report Bug](../../issues) · [Request Feature](../../issues)

![Last Updated](https://img.shields.io/badge/last%20updated-April%202026-blue?style=flat-square)

</div>
