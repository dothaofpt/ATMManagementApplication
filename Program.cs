
using Microsoft.EntityFrameworkCore;
using ATMManagementApplication.Data;
using Microsoft.Extensions.DependencyInjection;
using ATMManagementApplication.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add service to container => thiết lập cấu hình data model
builder.Services.AddControllers();
builder.Services.AddDbContext<ATMContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 33))));

// Thêm cấu hình EmailSettings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
