# 📧 EMAIL SETUP GUIDE

## 🔐 BẢO MẬT QUAN TRỌNG

**KHÔNG BAO GIỜ** commit password thật vào Git!

## 🚀 SETUP CHO DEVELOPMENT

### 1. Cấu hình User Secrets (Username PHẢI bằng FromEmail)
```bash
cd BE.API
dotnet user-secrets set "Email:Username" "your-email@gmail.com"
dotnet user-secrets set "Email:Password" "your-16-char-app-password"
dotnet user-secrets set "Email:FromEmail" "your-email@gmail.com"
```

### 2. Tạo Gmail App Password
1. Vào [Google Account Settings](https://myaccount.google.com/)
2. Security → 2-Step Verification → App passwords
3. Tạo password cho "Mail"
4. Copy 16-char password và dùng cho User Secrets

### 3. Test Email
```bash
POST /api/user/forgot-password
{
  "email": "test@example.com"
}
```

## 🏭 SETUP CHO PRODUCTION

### Option 1: Environment Variables (Username PHẢI bằng FromEmail)
```bash
export Email__Username="your-email@gmail.com"
export Email__Password="your-app-password"
export Email__FromEmail="your-email@gmail.com"
```

### Option 2: Azure Key Vault
```bash
az keyvault secret set --vault-name "your-vault" --name "Email--Password" --value "your-app-password"
```

## ⚠️ LƯU Ý BẢO MẬT

- ✅ Sử dụng User Secrets cho development
- ✅ Sử dụng Environment Variables cho production
- ✅ Sử dụng Azure Key Vault cho cloud
- ❌ KHÔNG commit password vào Git
- ❌ KHÔNG hardcode password trong code
- ❌ KHÔNG share password qua chat/email

## ❗ Quy tắc FromEmail = Username

- Gmail/SMTP thông thường chỉ cho phép gửi từ chính tài khoản đã đăng nhập.
- Không đặt `FromEmail` khác `Username` (sẽ bị chặn, vào spam, hoặc vi phạm chính sách).
- Nếu cần dùng `noreply@yourcompany.com`: hãy dùng SMTP của domain đó hoặc dịch vụ gửi mail chuyên dụng (SendGrid, Amazon SES, Mailgun) và xác thực domain (SPF/DKIM/DMARC).

## 🔍 KIỂM TRA CẤU HÌNH

```bash
# Xem User Secrets
dotnet user-secrets list

# Test email service
dotnet run --project BE.API
# Gọi API forgot-password và kiểm tra console logs
```
