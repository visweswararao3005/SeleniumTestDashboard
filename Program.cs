using Microsoft.EntityFrameworkCore;
using TestDashboard.Data;

var builder = WebApplication.CreateBuilder(args);

// Register DbContextFactory (for dynamic DB selection)
builder.Services.AddSingleton<DbContextFactory>();

// We don’t register ApplicationDbContext here with a fixed connection string
builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
