# 🌾 KunFarm Server - Unity 2D Farm Game API

Đây là API backend cho game Unity 2D nông trại KunFarm, được xây dựng với ASP.NET Core và Entity Framework.

## 📋 Tính năng

- **Authentication System**: Đăng ký, đăng nhập với JWT token
- **Base Repository Pattern**: Cấu trúc code sạch và dễ bảo trì
- **Entity Framework Core**: Sử dụng SQLite database
- **Clean Architecture**: Phân chia rõ ràng DAL, BLL, Presentation layers
- **Swagger Documentation**: API documentation tự động
- **CORS Support**: Hỗ trợ Unity client

## 🏗️ Cấu trúc dự án

```
KunFarm-Server/
├── KunFarm.DAL/           # Data Access Layer
│   ├── Entities/          # Database entities
│   ├── Interfaces/        # Repository interfaces
│   ├── Repositories/      # Repository implementations
│   └── Data/              # DbContext
├── KunFarm.BLL/           # Business Logic Layer
│   ├── DTOs/              # Data Transfer Objects
│   ├── Interfaces/        # Service interfaces
│   └── Services/          # Service implementations
└── KunFarm.Presentation/  # API Layer
    ├── Controllers/       # API Controllers
    └── wwwroot/           # Static files
```

## 🚀 Cách chạy

1. **Clone repository và di chuyển đến thư mục dự án:**
   ```bash
   cd KunFarm-Server
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Chạy ứng dụng:**
   ```bash
   cd KunFarm.Presentation
   dotnet run
   ```

4. **Truy cập API:**
   - API: `https://localhost:7xxx/api/auth`
   - Swagger UI: `https://localhost:7xxx`
   - Test Page: `https://localhost:7xxx/test-api.html`

## 📚 API Endpoints

### Authentication

#### POST `/api/auth/register`
Đăng ký người dùng mới

**Request Body:**
```json
{
  "username": "testuser",
  "email": "test@kunfarm.com",
  "password": "123456",
  "displayName": "Test Player"
}
```

#### POST `/api/auth/login`
Đăng nhập người dùng

**Request Body:**
```json
{
  "usernameOrEmail": "testuser",
  "password": "123456"
}
```

#### POST `/api/auth/validate-token`
Xác thực JWT token

**Request Body:**
```json
"your-jwt-token-here"
```

#### GET `/api/auth/health`
Kiểm tra trạng thái API

### Response Format

**Success Response:**
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "user": {
    "id": 1,
    "username": "testuser",
    "email": "test@kunfarm.com",
    "displayName": "Test Player",
    "level": 1,
    "experience": 0,
    "coins": 1000,
    "gems": 10,
    "lastLoginAt": "2024-01-01T00:00:00Z"
  },
  "token": "eyJhbGciOiJIUzI1NiIs..."
}
```

## 🎮 Tích hợp với Unity

### 1. Cài đặt packages trong Unity:
- `Newtonsoft.Json` (JSON serialization)
- `Unity.WebRequest` (HTTP requests)

### 2. Sử dụng API trong Unity:

```csharp
using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;

public class AuthManager : MonoBehaviour
{
    private const string API_URL = "https://localhost:7xxx/api/auth";
    
    [Serializable]
    public class LoginRequest
    {
        public string usernameOrEmail;
        public string password;
    }
    
    [Serializable]
    public class LoginResponse
    {
        public bool success;
        public string message;
        public UserData user;
        public string token;
    }
    
    [Serializable]
    public class UserData
    {
        public int id;
        public string username;
        public string email;
        public string displayName;
        public int level;
        public int experience;
        public decimal coins;
        public decimal gems;
    }
    
    public IEnumerator Login(string usernameOrEmail, string password)
    {
        var loginData = new LoginRequest
        {
            usernameOrEmail = usernameOrEmail,
            password = password
        };
        
        string json = JsonConvert.SerializeObject(loginData);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
        
        using (UnityWebRequest request = new UnityWebRequest($"{API_URL}/login", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);
                
                if (response.success)
                {
                    // Lưu token và user data
                    PlayerPrefs.SetString("AuthToken", response.token);
                    PlayerPrefs.SetString("UserData", JsonConvert.SerializeObject(response.user));
                    
                    Debug.Log("Đăng nhập thành công!");
                }
                else
                {
                    Debug.LogError($"Đăng nhập thất bại: {response.message}");
                }
            }
            else
            {
                Debug.LogError($"Lỗi network: {request.error}");
            }
        }
    }
}
```

## 🔐 Bảo mật

- **Password Hashing**: Sử dụng BCrypt để hash password
- **JWT Token**: Token có thời hạn 7 ngày
- **HTTPS**: Sử dụng HTTPS trong production
- **Validation**: Validate input data

## 🗄️ Database

Dự án sử dụng SQLite với Entity Framework Core. Database được tạo tự động khi chạy ứng dụng lần đầu.

**Default Admin Account:**
- Username: `admin`
- Password: `admin123`
- Email: `admin@kunfarm.com`

## 🔧 Configuration

Cấu hình trong `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=kunfarm.db"
  },
  "JwtSettings": {
    "SecretKey": "YourSuperSecretKeyForKunFarmGame123456789",
    "Issuer": "KunFarm",
    "Audience": "KunFarmUsers",
    "ExpirationInDays": 7
  }
}
```

## 🧪 Testing

1. **Sử dụng Swagger UI**: Truy cập `https://localhost:7xxx`
2. **Sử dụng Test Page**: Truy cập `https://localhost:7xxx/test-api.html`
3. **Sử dụng Postman**: Import API endpoints

## 📝 Logs

Logs được ghi vào console với các mức độ:
- **Information**: Đăng nhập thành công
- **Warning**: Đăng nhập thất bại
- **Error**: Lỗi server

## 🤝 Đóng góp

1. Fork dự án
2. Tạo branch mới (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Tạo Pull Request

## 📄 License

Dự án này được phân phối dưới MIT License.

## 🎯 Roadmap

- [ ] Thêm chức năng quản lý nông trại
- [ ] Thêm system shop và inventory
- [ ] Thêm multiplayer features
- [ ] Thêm daily missions
- [ ] Thêm leaderboard system 