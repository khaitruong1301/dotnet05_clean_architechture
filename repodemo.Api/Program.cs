using Microsoft.EntityFrameworkCore;
using repodemo.Infrastructure;
using repodemo.Infrastructure.Models;
var builder = WebApplication.CreateBuilder(args);
//DI các nghiệp vụ


//Đăng ký entity framework với sql server
builder.Services.AddDbContext<CybersoftMarketplaceContext>(options=>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("connectionCybersoftMarketplace"));
});

//DI controller
builder.Services.AddControllers();

//DISwagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


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

//Service
builder.Services.AddScoped<IProductService, ProductService>();


var app = builder.Build();


//Apply các middleware  
app.MapControllers();
app.UseHttpsRedirection();




//swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();

