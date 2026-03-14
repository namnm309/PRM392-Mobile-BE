# Cấu hình SignalR Chat cho Azure App Service

## Lỗi 401 Unauthorized khi kết nối chat

### Nguyên nhân 1: User chưa có trong DB (thường gặp nhất)

`OnTokenValidated` kiểm tra user theo ClerkId (sub trong JWT). **Nếu không tìm thấy → 401.**

**Cách sửa:** Đảm bảo user đã được tạo trong bảng Users:
- Đăng ký qua app → Clerk webhook `user.created` gọi backend tạo user
- Hoặc gọi `POST /api/Users` để tạo thủ công với ClerkId
- Kiểm tra: `SELECT * FROM Users WHERE ClerkId = 'user_xxx'` (lấy ClerkId từ Clerk Dashboard)

### Nguyên nhân 2: Token không được đọc từ query

SignalR **không gửi Authorization header**. Token phải truyền qua query string `?access_token=xxx`.

**Frontend** (đã có trong `supportChatService.ts`):
```ts
.withUrl(HUB_URL, {
  accessTokenFactory: async () => {
    const t = await getToken();
    if (!t) throw new Error('Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.');
    return t;
  },  // SignalR tự thêm ?access_token=... vào mọi request
  transport: signalR.HttpTransportType.LongPolling,
})
```

**Backend** (trong `ClerkJwtBearerPostConfigure.cs`):
- `OnMessageReceived` đọc `context.Request.Query["access_token"]` khi path chứa `hubs/`
- Đã cập nhật để match path chính xác hơn (StartsWith, Contains)

## Lỗi 404 / Timeout khi kết nối chat

Nếu mobile hoặc admin gặp lỗi **404** hoặc **Timeout** khi kết nối SignalR, kiểm tra các mục sau trên **Azure Portal**:

### 1. Bật Web Sockets (bắt buộc)

1. Vào **Azure Portal** → App Service **TechStoreBE**
2. **Configuration** → **General settings**
3. Đặt **Web Sockets** = **On**
4. Lưu và restart app

### 2. Bật ARR Affinity (nên bật với Long Polling)

1. **Configuration** → **General settings**
2. Đặt **ARR Affinity** = **On**
3. Đảm bảo mọi request từ cùng client đi tới cùng instance

### 3. Kiểm tra URL

- Hub endpoint đúng: `https://<your-app>.azurewebsites.net/hubs/support-chat`
- **Sai**: `https://TechStoreBE:80/...` (port 80 không dùng với HTTPS; dùng domain đầy đủ)
- Mobile/Admin dùng `EXPO_PUBLIC_API_BASE_URL` / `NEXT_PUBLIC_API_BASE_URL` trỏ đúng backend

### 4. User trong Database

- User (mobile) và Staff/Admin phải có trong bảng **Users** với `ClerkId` khớp tài khoản Clerk
- Role phải là Customer (user) hoặc Staff/Admin (nhân viên)

## Chạy local

- Backend: `dotnet run` → `http://localhost:5136` hoặc `https://localhost:7264`
- Mobile: `EXPO_PUBLIC_API_BASE_URL=http://10.0.2.2:5136` (Android emulator)
- Admin: `NEXT_PUBLIC_API_BASE_URL=http://localhost:5136`
