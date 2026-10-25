using System.IdentityModel.Tokens.Jwt;
using System.Text;
using BillLocket.Application.Interfaces;
using BillLocket.Infrastructure.Authentication;
using BillLocket.Infrastructure.Identity;
using BillLocket.Infrastructure.Persistence;
using BillLocket.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var jwtSection = builder.Configuration.GetRequiredSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is missing.");
jwtOptions.Validate();

// Đăng ký DbContext vào Dependency Injection (DI).
// Sử dụng Entity Framework Core với SQL Server.
// Chuỗi kết nối được lấy từ "ConnectionStrings:DefaultConnection"
// trong appsettings.json.
builder.Services.AddDbContext<BillLocketDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Cấu hình ASP.NET Core Identity để quản lý người dùng,
// xác thực tài khoản và thiết lập chính sách mật khẩu.
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Chính sách bảo mật mật khẩu
        options.Password.RequiredLength = 8;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;
        options.Password.RequireNonAlphanumeric = true;

        // Yêu cầu email đăng ký phải duy nhất
        options.User.RequireUniqueEmail = true;
    })
    // Sử dụng EF Core và BillLocketDbContext để lưu dữ liệu Identity
    .AddEntityFrameworkStores<BillLocketDbContext>()

    // Đăng ký các token provider mặc định của Identity,
    // phục vụ xác nhận email, đặt lại mật khẩu, v.v.
    .AddDefaultTokenProviders();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ITokenService, JwtTokenService>();
builder.Services.Configure<JwtOptions>(jwtSection);

builder.Services
    .AddAuthentication(options =>
    {
        // AddIdentity configures its application cookie as the default scheme.
        // API authorization must explicitly replace both defaults with Bearer.
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Key)),
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero,
            NameClaimType = JwtRegisteredClaimNames.Sub
        };
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
