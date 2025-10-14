using BE.REPOs.Implementation;
using BE.REPOs.Interface;
using BE.REPOs.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Net.Http.Headers;

var builder = WebApplication.CreateBuilder(args);
var cfg = builder.Configuration;

// ===== Auth =====
var auth = builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
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
});


var googleId = cfg["OAuth:Google:ClientId"];
var googleSecret = cfg["OAuth:Google:ClientSecret"];
if (!string.IsNullOrWhiteSpace(googleId) && !string.IsNullOrWhiteSpace(googleSecret))
{
    auth.AddGoogle(options =>
    {
        options.ClientId = googleId!;
        options.ClientSecret = googleSecret!;
        options.CallbackPath = "/api/User/google-callback"; 
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
    });
}

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireClaim(ClaimTypes.Role, "1"));
    options.AddPolicy("MemberOnly", p => p.RequireClaim(ClaimTypes.Role, "2"));
});

// ===== Swagger (một block duy nhất) =====
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

builder.Services.AddControllers();

// ===== Repos/Services DI của bạn giữ nguyên =====
builder.Services.AddScoped<IUserRepo, UserRepo>();
builder.Services.AddScoped<IFavoriteRepo, FavoriteRepo>();
builder.Services.AddScoped<IProductRepo, ProductRepo>();
builder.Services.AddScoped<IPaymentRepo, PaymentRepo>();
builder.Services.AddScoped<IOrderRepo, OrderRepo>();
builder.Services.AddScoped<IProductImageRepo, ProductImageRepo>();
builder.Services.AddScoped<IUserRoleRepo, UserRoleRepo>();
builder.Services.AddScoped<IReviewsRepo, ReviewsRepo>();
builder.Services.AddScoped<IReportedListingsRepo, ReportedListingsRepo>();
builder.Services.AddScoped<IFeeSettings, FeeSettingsRepo>();
builder.Services.AddScoped<INotificationsRepo, NotificationsRepo>();
builder.Services.AddScoped<CloudinaryService>();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IOTPService, OTPService>();

// ===== AI HttpClient (giữ nguyên cách bạn set) =====
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

builder.Services.AddCors(o => o.AddPolicy("AllowAll", p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

var app = builder.Build();

// ===== giữ Swagger như trước: bật khi Development =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
