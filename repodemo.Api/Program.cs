using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using repodemo.Infrastructure;
using repodemo.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
//DI các nghiệp vụ


//Đăng ký entity framework với sql server

//Tắt tham chiếu vòng lặp giữa project Api và Infrastructure
builder.Services.AddDbContext<CybersoftMarketplaceContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("connectionCybersoftMarketplace"));
    //Lấy dữ liệu tham chiếu trên khoá ngoại của bảng liên quan mà không cần phải Include() (lazy loading)
    options.UseLazyLoadingProxies(true);
    //Nếu bị tham chiếu vòng thì sử .ToList().Select() để lấy dữ liệu thay vì trả về đối tượng trực tiếp (VD: trả về product => trả về product.ToList().Select(p => new ProductDTO { Id = p.Id, Name = p.Name, ... }))

});

// Microsoft.EntityFrameworkCore.Proxies


//DI controller
builder.Services.AddControllers();

//DISwagger
builder.Services.AddEndpointsApiExplorer();
//cấu hình kích hoạt authorize của swagger jwt (hiện ổ khoá đăng nhập ở góc phải trên cùng của swagger)
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "nhập vào token JWT định dạng Bearer {token của bạn}."
    });

    //hiện button đăng nhập(ổ khoá nhập token) trên từng api endpoint của swagger
    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});



//DI Repository và service
//Repository
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<RoleRepository>();
builder.Services.AddScoped<RatingRepository>();
builder.Services.AddScoped<CategoryRepository>();
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<CartRepository>();
builder.Services.AddScoped<CartItemRepository>();
builder.Services.AddScoped<ConversationRepository>();
builder.Services.AddScoped<CustomerRepository>();
builder.Services.AddScoped<MessageRepository>();
builder.Services.AddScoped<OrderItemRepository>();
builder.Services.AddScoped<ProductImageRepository>();
builder.Services.AddScoped<ProductVariantRepository>();
builder.Services.AddScoped<ShopRepository>();
builder.Services.AddScoped<VGetAllProductsDetailRepository>();


//UnitOfWork
builder.Services.AddScoped<UnitOfWork>();

//Service
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IUserService,UserService>();
builder.Services.AddScoped<JwtService>();



//JWT service
//Thêm middleware authentication
var privateKey = builder.Configuration["jwt:Serect-Key"];
var Issuer = builder.Configuration["jwt:Issuer"];
var Audience = builder.Configuration["jwt:Audience"];
// Thêm dịch vụ Authentication vào ứng dụng, sử dụng JWT Bearer làm phương thức xác thực
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    // Thiết lập các tham số xác thực token
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        // Kiểm tra và xác nhận Issuer (nguồn phát hành token)
        ValidateIssuer = true,
        ValidIssuer = Issuer, // Biến `Issuer` chứa giá trị của Issuer hợp lệ
                              // Kiểm tra và xác nhận Audience (đối tượng nhận token)
        ValidateAudience = true,
        ValidAudience = Audience, // Biến `Audience` chứa giá trị của Audience hợp lệ
                                  // Kiểm tra và xác nhận khóa bí mật được sử dụng để ký token
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(privateKey)),
        // Sử dụng khóa bí mật (`privateKey`) để tạo SymmetricSecurityKey nhằm xác thực chữ ký của token
        // Giảm độ trễ (skew time) của token xuống 0, đảm bảo token hết hạn chính xác
        ClockSkew = TimeSpan.Zero,
        // Xác định claim chứa vai trò của user (để phân quyền)
        RoleClaimType = ClaimTypes.Role,
        // Xác định claim chứa tên của user
        NameClaimType = ClaimTypes.Name,
        // Kiểm tra thời gian hết hạn của token, không cho phép sử dụng token hết hạn
        ValidateLifetime = true
    };
});

//di phân quyền
builder.Services.AddAuthorization();

//Tạo cors cho http://localhost:5188/ (client blazor) để client có thể gọi api (đặt tên client là "AllowBlazorClient")
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", builder =>
    {
        builder.WithOrigins("http://localhost:5188")
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

var app = builder.Build();


//Apply các middleware  
app.UseAuthentication(); //xác thực [Authorize]
app.UseAuthorization(); //phân quyền [Authorize(Roles = "ADMIN")]
app.MapControllers();
app.UseHttpsRedirection();

//linkedin https://www.linkedin.com/in/khai-pham-van-9bba25236/

//Sử dụng cors 
app.UseCors("AllowBlazorClient");


//swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

