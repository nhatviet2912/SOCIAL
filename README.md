# SOCIAL - Dự án mạng xã hội ASP.NET Core

## Giới thiệu
Dự án **SOCIAL** là một hệ thống mạng xã hội được xây dựng trên nền tảng ASP.NET Core 8, sử dụng kiến trúc đa tầng (multi-project) gồm các module: WebAPI, Application, Infrastructure, Domain, Shared. Dự án hỗ trợ xác thực JWT, đăng nhập Google, gửi email xác thực, quản lý người dùng, phân quyền, refresh token, cache Redis, logging với Serilog, v.v.

## Cấu trúc thư mục
```
SOCIAL/
├── Application/      # Business logic, DTO, Service, Interface
├── Domain/           # Entity, Enum, Constant, Base class
├── Infrastructure/   # Data access, Repository, Service, Cache, Logging
├── Shared/           # Thư viện chia sẻ
├── WebAPI/           # API layer, Controller, Middleware, Program.cs
├── SOCIAL_BE.sln     # Solution file
├── Dockerfile        # Docker build file
```

## Công nghệ sử dụng
- ASP.NET Core 8
- Entity Framework Core
- MySQL
- Redis
- Serilog
- Swagger
- Docker

## Hướng dẫn chạy bằng Docker
1. **Build Docker image:**
   ```sh
   docker build -t social-api .
   ```
2. **Chạy container:**
   ```sh
   docker run -p 80:80 social-api
   ```
3. **Truy cập API:**
   - Mặc định API chạy ở: http://localhost:80
   - Swagger UI: http://localhost:80/swagger

## Cấu hình môi trường
- Sửa các thông tin kết nối DB, Redis, Email... trong `WebAPI/appsettings.json` hoặc truyền qua biến môi trường khi chạy Docker.

## Một số API chính
- Đăng ký, đăng nhập, xác thực email
- Đăng nhập Google
- Refresh token, logout, quản lý thiết bị
- Quản lý user, role