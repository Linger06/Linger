using Linger.AspNetCore.Jwt;
using Linger.AspNetCore.Jwt.Contracts;

var builder = WebApplication.CreateBuilder(args);

// 添加服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.AddScoped<IJwtService, JwtService>();
//// 注册JWT服务
//builder.Services.AddSingleton<JwtService>();

//// 配置JWT身份验证
//var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSecretKeyHereMustBeLongEnoughForSecurity123456789";
//var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "LingerApiIssuer";
//var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "LingerApiAudience";

//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = jwtIssuer,
//        ValidAudience = jwtAudience,
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
//        ClockSkew = TimeSpan.Zero // 消除时间允差，更严格地验证令牌过期时间
//    };

//    options.Events = new JwtBearerEvents
//    {
//        OnAuthenticationFailed = context =>
//        {
//            Console.WriteLine($"认证失败: {context.Exception.Message}");
//            return Task.CompletedTask;
//        },
//        OnTokenValidated = context =>
//        {
//            Console.WriteLine($"令牌验证成功: {context.SecurityToken}");
//            return Task.CompletedTask;
//        },
//        OnChallenge = context =>
//        {
//            Console.WriteLine($"认证挑战: {context.AuthenticateFailure?.Message}");
//            return Task.CompletedTask;
//        }
//    };
//});

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

// 打印应用配置信息
//Console.WriteLine($"JWT配置 - 密钥长度: {jwtKey.Length}, 发行者: {jwtIssuer}, 受众: {jwtAudience}");

app.Run();
