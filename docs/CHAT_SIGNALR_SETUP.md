# Cấu hình SignalR Chat cho Azure App Service

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

- Hub endpoint: `https://<your-app>.azurewebsites.net/hubs/support-chat`
- Mobile/Admin dùng `EXPO_PUBLIC_API_BASE_URL` / `NEXT_PUBLIC_API_BASE_URL` trỏ đúng backend

### 4. User trong Database

- User (mobile) và Staff/Admin phải có trong bảng **Users** với `ClerkId` khớp tài khoản Clerk
- Role phải là Customer (user) hoặc Staff/Admin (nhân viên)

## Chạy local

- Backend: `dotnet run` → `http://localhost:5136` hoặc `https://localhost:7264`
- Mobile: `EXPO_PUBLIC_API_BASE_URL=http://10.0.2.2:5136` (Android emulator)
- Admin: `NEXT_PUBLIC_API_BASE_URL=http://localhost:5136`
