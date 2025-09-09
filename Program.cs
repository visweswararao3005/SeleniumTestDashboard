using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<TestDashboard.Data.ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
var app = builder.Build();

if (!app.Environment.IsDevelopment()) { app.UseExceptionHandler("/Home/Error"); }
app.UseStaticFiles();
app.UseRouting();
app.MapControllerRoute("default", "{controller=Dashboard}/{action=Index}/{id?}");
app.Run();

