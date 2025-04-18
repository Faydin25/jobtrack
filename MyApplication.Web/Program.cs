using Microsoft.EntityFrameworkCore;
using MyApplication.Web.Data;
using MyApplication.Web.Services;
using Microsoft.Extensions.DependencyInjection;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Veritabanı bağlantı ayarları
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// EmailValidatorService'in Dependency Injection'a eklenmesi
builder.Services.AddScoped<EmailValidatorService>();

// Servisler
builder.Services.AddControllersWithViews();

// Session ayarları
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session süresi 30 dakika
});

var app = builder.Build();

// Middleware yapılandırması
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();           // Session middleware
app.UseHttpsRedirection(); // HTTPS yönlendirme
app.UseStaticFiles();      // Statik dosyalar

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Uygulama çalıştırma
app.Run();
