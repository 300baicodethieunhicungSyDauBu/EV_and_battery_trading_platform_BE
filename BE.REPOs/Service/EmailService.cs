using BE.REPOs.Interface;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BE.REPOs.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email, string otp)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("EV & Battery Trading Platform", _configuration["Email:FromEmail"]));
                message.To.Add(new MailboxAddress("", email));
                message.Subject = "Reset Your Password - EV & Battery Trading Platform";

                // Tạo email body với OTP
                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = GetPasswordResetEmailTemplate(otp)
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_configuration["Email:SmtpHost"], 
                    int.Parse(_configuration["Email:SmtpPort"]), 
                    SecureSocketOptions.StartTls);
                
                await client.AuthenticateAsync(_configuration["Email:Username"], 
                    _configuration["Email:Password"]);
                
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Password reset email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send password reset email to {email}");
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string email, string userName)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("EV & Battery Trading Platform", _configuration["Email:FromEmail"]));
                message.To.Add(new MailboxAddress(userName, email));
                message.Subject = "Welcome to EV & Battery Trading Platform!";

                var bodyBuilder = new BodyBuilder
                {
                    HtmlBody = GetWelcomeEmailTemplate(userName)
                };

                message.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                await client.ConnectAsync(_configuration["Email:SmtpHost"], 
                    int.Parse(_configuration["Email:SmtpPort"]), 
                    SecureSocketOptions.StartTls);
                
                await client.AuthenticateAsync(_configuration["Email:Username"], 
                    _configuration["Email:Password"]);
                
                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation($"Welcome email sent successfully to {email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send welcome email to {email}");
                throw;
            }
        }

        private string GetPasswordResetEmailTemplate(string otp)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Reset Your Password</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .container {{
            background-color: #ffffff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 0 20px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 24px;
            font-weight: bold;
            color: #2c3e50;
            margin-bottom: 10px;
        }}
        .title {{
            color: #e74c3c;
            font-size: 28px;
            margin-bottom: 20px;
        }}
        .content {{
            margin-bottom: 30px;
        }}
        .button {{
            display: inline-block;
            background-color: #3498db;
            color: white;
            padding: 15px 30px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
            transition: background-color 0.3s;
        }}
        .button:hover {{
            background-color: #2980b9;
        }}
        .token-box {{
            background-color: #ecf0f1;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
            font-family: monospace;
            word-break: break-all;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #ecf0f1;
            color: #7f8c8d;
            font-size: 14px;
        }}
        .warning {{
            background-color: #fff3cd;
            border: 1px solid #ffeaa7;
            color: #856404;
            padding: 15px;
            border-radius: 5px;
            margin: 20px 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>🚗⚡ EV & Battery Trading Platform</div>
            <h1 class='title'>Reset Your Password</h1>
        </div>
        
        <div class='content'>
            <p>Hello,</p>
            <p>We received a request to reset your password for your EV & Battery Trading Platform account.</p>
            <p>Please use the following OTP (One-Time Password) to reset your password:</p>
            
            <div style='text-align: center; margin: 30px 0;'>
                <div style='background: #f8f9fa; border: 2px solid #007bff; border-radius: 10px; padding: 20px; display: inline-block; font-size: 32px; font-weight: bold; letter-spacing: 5px; color: #007bff;'>
                    {otp}
                </div>
            </div>
            
            <div class='warning'>
                <strong>⚠️ Important:</strong>
                <ul>
                    <li>This OTP will expire in 10 minutes</li>
                    <li>If you didn't request this, please ignore this email</li>
                    <li>For security, never share this OTP with anyone</li>
                    <li>Enter this OTP in the password reset form</li>
                </ul>
            </div>
        </div>
        
        <div class='footer'>
            <p>This email was sent from EV & Battery Trading Platform</p>
            <p>If you have any questions, please contact our support team.</p>
        </div>
    </div>
</body>
</html>";
        }

        private string GetWelcomeEmailTemplate(string userName)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Welcome to EV & Battery Trading Platform</title>
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            line-height: 1.6;
            color: #333;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #f4f4f4;
        }}
        .container {{
            background-color: #ffffff;
            padding: 30px;
            border-radius: 10px;
            box-shadow: 0 0 20px rgba(0,0,0,0.1);
        }}
        .header {{
            text-align: center;
            margin-bottom: 30px;
        }}
        .logo {{
            font-size: 24px;
            font-weight: bold;
            color: #2c3e50;
            margin-bottom: 10px;
        }}
        .title {{
            color: #27ae60;
            font-size: 28px;
            margin-bottom: 20px;
        }}
        .content {{
            margin-bottom: 30px;
        }}
        .button {{
            display: inline-block;
            background-color: #27ae60;
            color: white;
            padding: 15px 30px;
            text-decoration: none;
            border-radius: 5px;
            font-weight: bold;
            margin: 20px 0;
        }}
        .footer {{
            text-align: center;
            margin-top: 30px;
            padding-top: 20px;
            border-top: 1px solid #ecf0f1;
            color: #7f8c8d;
            font-size: 14px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <div class='logo'>🚗⚡ EV & Battery Trading Platform</div>
            <h1 class='title'>Welcome, {userName}!</h1>
        </div>
        
        <div class='content'>
            <p>Thank you for joining EV & Battery Trading Platform!</p>
            <p>You can now:</p>
            <ul>
                <li>🛒 Browse electric vehicles and batteries</li>
                <li>💰 List your own products for sale</li>
                <li>💬 Connect with other users</li>
                <li>🔍 Search by license plate</li>
            </ul>
            
            <div style='text-align: center;'>
                <a href='{_configuration["Email:BaseUrl"]}' class='button'>Start Exploring</a>
            </div>
        </div>
        
        <div class='footer'>
            <p>Happy trading! 🚗⚡</p>
        </div>
    </div>
</body>
</html>";
        }
    }
}
