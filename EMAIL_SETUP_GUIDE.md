# Email Configuration Guide

## 📧 Cấu hình Email cho EV & Battery Trading Platform

### 🔧 Development Environment

Để test chức năng forgot/reset password trong môi trường development:

1. **Tạo Gmail App Password:**
   - Đăng nhập Gmail của bạn
   - Vào Settings → Security → 2-Step Verification (bật nếu chưa)
   - Tạo App Password cho "Mail"
   - Copy App Password (16 ký tự)

2. **Cập nhật `appsettings.Development.json`:**
   ```json
   {
     "Email": {
       "SmtpHost": "smtp.gmail.com",
       "SmtpPort": "587",
       "Username": "your-dev-email@gmail.com",
       "Password": "your-16-char-app-password",
       "FromEmail": "your-dev-email@gmail.com",
       "FromName": "EV & Battery Trading Platform (Dev)",
       "BaseUrl": "http://localhost:3000"
     }
   }
   ```

### 🚀 Production Environment

Cho production, bạn cần:

1. **Tạo email business:**
   - `noreply@evtrading.com` (hoặc domain của bạn)
   - Hoặc sử dụng email service như SendGrid, Mailgun

2. **Cấu hình SMTP server:**
   - Sử dụng SMTP server của hosting provider
   - Hoặc email service provider

3. **Environment Variables (Khuyến nghị):**
   ```bash
   EMAIL__USERNAME=noreply@evtrading.com
   EMAIL__PASSWORD=your-production-password
   EMAIL__FROMEMAIL=noreply@evtrading.com
   ```

### 🔒 Security Notes

- **Không commit** password vào git
- Sử dụng **Environment Variables** cho production
- **App Password** chỉ dùng cho development
- **2FA** phải được bật cho Gmail

### 🧪 Testing

1. **Development:** Sử dụng Gmail cá nhân với App Password
2. **Production:** Sử dụng email service chuyên nghiệp
3. **Staging:** Có thể dùng email test riêng

### 📝 Troubleshooting

**Lỗi 535 Authentication failed:**
- Kiểm tra App Password (không phải password Gmail)
- Đảm bảo 2-Step Verification đã bật
- Kiểm tra Username đúng format

**Email không nhận được:**
- Kiểm tra Spam folder
- Kiểm tra SMTP settings
- Test với email khác
