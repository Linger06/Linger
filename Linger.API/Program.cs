using Linger.AspNetCore.Jwt;
using Linger.AspNetCore.Jwt.Contracts;

var builder = WebApplication.CreateBuilder(args);

// 添加服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 使用ConfigureJwt扩展方法配置JWT认证
builder.Services.ConfigureJwt(builder.Configuration);
// 注册基本JWT服务（不含刷新功能）
builder.Services.AddScoped<IJwtService, JwtService>();

// 添加CORS策略
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// 配置HTTP请求管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// 确保认证中间件在授权中间件之前
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
