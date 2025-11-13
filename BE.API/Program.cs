using BE.REPOs.Implementation;
using BE.REPOs.Interface;
using BE.REPOs.Service;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;                  // CookieSecurePolicy, SameSiteMode
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using BE.API.Hubs;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// =================== Authentication ===================
var auth = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    // options.DefaultSignInScheme = "External"; // chỉ cần nếu bạn dùng cookie ngoài
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = cfg["JWT:Issuer"],
        ValidAudience = cfg["JWT:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(cfg["JWT:SecretKey"] ?? "default-secret-key"))
    };
})
// Cookie scheme dùng cho OAuth external
.AddCookie("External", options =>
{
    options.Cookie.Name = "EVTrading.External";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // đổi thành Always khi deploy HTTPS
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.IsEssential = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(10);
});

// Đăng ký Google/Facebook **chỉ khi** có client id/secret
var googleId = cfg["OAuth:Google:ClientId"];
var googleSecret = cfg["OAuth:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleId) && !string.IsNullOrWhiteSpace(googleSecret))
{
    auth.AddGoogle(options =>
    {
        options.ClientId = googleId!;
        options.ClientSecret = googleSecret!;
        options.CallbackPath = "/api/User/google-callback";
        options.SignInScheme = "External";
        options.SaveTokens = false;
    });
}

var fbId = cfg["OAuth:Facebook:AppId"];
var fbSecret = cfg["OAuth:Facebook:AppSecret"];
if (!string.IsNullOrWhiteSpace(fbId) && !string.IsNullOrWhiteSpace(fbSecret))
{
    auth.AddFacebook(options =>
    {
        options.AppId = fbId!;
        options.AppSecret = fbSecret!;
        options.CallbackPath = "/api/User/facebook-callback";
        options.SignInScheme = "External";
        options.SaveTokens = false;
    });
}

// =================== Authorization ===================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireClaim(ClaimTypes.Role, "1"));
    options.AddPolicy("MemberOnly", p => p.RequireClaim(ClaimTypes.Role, "2"));
    options.AddPolicy("StaffOnly", p => p.RequireClaim(ClaimTypes.Role, "3"));
    // Policy cho phép cả Admin (RoleId = 1) và Staff (RoleId = 3)
    options.AddPolicy("AdminOrStaff", p => 
        p.RequireAssertion(context => 
        {
            var roleClaim = context.User.FindFirst(ClaimTypes.Role);
            return roleClaim != null && (roleClaim.Value == "1" || roleClaim.Value == "3");
        }));
});

// =================== Swagger ===================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "EV And Battery Trading Platform", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Bearer. Ví dụ: 'Bearer {token}'",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder =
            System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.WriteIndented = false;
    });

// =================== DbContext ===================
builder.Services.AddDbContext<BE.BOs.Models.EvandBatteryTradingPlatformContext>(options =>
{
    options.UseSqlServer(cfg.GetConnectionString("DefaultConnectionString"), sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 3,
            maxRetryDelay: TimeSpan.FromSeconds(5),
            errorNumbersToAdd: null);
        sqlOptions.CommandTimeout(60);
    });
});

// =================== DI (Repos/Services) ===================
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IFavoriteRepo, FavoriteRepo>();
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.Services.AddScoped<IPaymentRepo, PaymentRepo>();
builder.Services.AddScoped<IOrderRepo, OrderRepo>();
builder.Services.AddScoped<IProductImageRepo, ProductImageRepo>();
builder.Services.AddScoped<IUserRoleRepo, UserRoleRepo>();
builder.Services.AddScoped<IReviewsRepo, ReviewsRepo>();
builder.Services.AddScoped<IReportedListingsRepo, ReportedListingsRepo>();
builder.Services.AddScoped<IFeeSettingsRepo, FeeSettingsRepo>();
builder.Services.AddScoped<INotificationsRepo, NotificationsRepo>();
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOTPService, OTPService>();
builder.Services.AddScoped<BE.DAOs.ChatDAO>();
builder.Services.AddScoped<BE.DAOs.MessageDAO>();
builder.Services.AddScoped<IChatRepo, ChatRepo>();
builder.Services.AddScoped<IMessageRepo, MessageRepo>();

// =================== AI HttpClient ===================
var apiBase = cfg["OpenAI:ApiBase"]
             ?? Environment.GetEnvironmentVariable("OPENAI_API_BASE")
             ?? Environment.GetEnvironmentVariable("OPENROUTER_API_BASE")
             ?? "https://openrouter.ai/api";
var apiKey = cfg["OpenAI:ApiKey"]
            ?? cfg["OpenRouter:ApiKey"]
            ?? Environment.GetEnvironmentVariable("OPENAI_API_KEY")
            ?? Environment.GetEnvironmentVariable("OPENROUTER_API_KEY")
            ?? string.Empty;
var model = cfg["OpenAI:Model"]
            ?? cfg["OpenRouter:Model"]
            ?? Environment.GetEnvironmentVariable("OPENAI_MODEL")
            ?? Environment.GetEnvironmentVariable("OPENROUTER_MODEL")
            ?? "gpt-oss-20b";

builder.Services.AddHttpClient("OpenRouter", client =>
{
    client.BaseAddress = new Uri($"{apiBase.TrimEnd('/')}/v1/");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    if (!string.IsNullOrEmpty(apiKey))
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    client.DefaultRequestHeaders.TryAddWithoutValidation("HTTP-Referer", "http://localhost:5044");
    client.DefaultRequestHeaders.TryAddWithoutValidation("X-Title", "EV & Battery Trading Platform");
});

builder.Services.AddSingleton(new OpenAIOptions
{
    Model = model,
    DefaultMaxTokens = int.TryParse(cfg["OpenAI:DefaultMaxTokens"], out var mx) ? mx : 1024,
    DefaultTemperature = double.TryParse(cfg["OpenAI:DefaultTemperature"], out var tp) ? tp : 0.3
});
builder.Services.AddScoped<IAIChatService, OpenRouterChatService>();

// =================== CORS ===================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "https://evtrading-frontend.vercel.app",
                    "http://localhost:5174"
                )
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials() // ✅ Cho phép gửi cookie/token và SignalR handshake
    );
});

// Thêm SignalR
builder.Services.AddSignalR();

var app = builder.Build();

// Map hub endpoint
app.MapHub<ChatHub>("/chatHub");

// =================== Pipeline ===================
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "EV Battery API v1");
    c.RoutePrefix = string.Empty; // Swagger ở root (/) thay vì /swagger
});

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();
