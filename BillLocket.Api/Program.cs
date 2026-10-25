using BillLocket.Application.Interfaces;
using BillLocket.Infrastructure.Identity;
using BillLocket.Infrastructure.Persistence;
using BillLocket.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
