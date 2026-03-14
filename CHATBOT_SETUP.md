# Cấu hình AI Chatbot (Mega LLM)

## 1. Cấu hình API Key

Thêm `MegaLLM:ApiKey` vào cấu hình backend. Có 2 cách:

### Cách 1: appsettings.json (chỉ dev)

```json
"MegaLLM": {
  "ApiKey": "your_megallm_api_key_here",
  "BaseUrl": "https://ai.megallm.io/v1",
  "ModelId": "openai-gpt-oss-20b",
  "MaxTokens": 5000,
  "Temperature": 1.5
}
```

### Cách 2: Biến môi trường (khuyến nghị cho production)

```bash
MegaLLM__ApiKey=your_megallm_api_key_here
MegaLLM__BaseUrl=https://ai.megallm.io/v1
MegaLLM__ModelId=openai-gpt-oss-20b
```

Lấy API key tại: https://megallm.io/dashboard

## 2. Kiểm tra cấu hình

Sau khi chạy backend, gọi:

```
GET /api/chat/diagnostic
```

Response `status: "OK"` nghĩa là đã cấu hình đúng.

## 3. API Chat

- `POST /api/chat` — Gửi tin nhắn, nhận phản hồi từ AI
- Body: `{ "messages": [{ "role": "user", "content": "..." }], "systemPrompt": "..." }`
