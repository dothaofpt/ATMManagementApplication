using Microsoft.EntityFrameworkCore;
using ATMManagementApplication.Data;

var builder = WebApplication.CreateBuilder(args);

//Add service to container => thiet lap cau hinh data model
builder.Services.AddControllers();
builder.Services.AddDbContext<ATMContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 33))));

var app = builder.Build();

if(app.Environment.IsDevelopment()){
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();