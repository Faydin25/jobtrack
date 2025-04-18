using Microsoft.EntityFrameworkCore;
using MyApplication.Web.Data;
using MyApplication.Web.Services;
using Microsoft.Extensions.DependencyInjection;

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Veritaban� ba�lant� ayarlar�
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// EmailValidatorService'in Dependency Injection'a eklenmesi
builder.Services.AddScoped<EmailValidatorService>();

// Servisler
builder.Services.AddControllersWithViews();

// Session ayarlar�
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // Session s�resi 30 dakika
});

var app = builder.Build();

// Middleware yap�land�rmas�
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
app.UseHttpsRedirection(); // HTTPS y�nlendirme
app.UseStaticFiles();      // Statik dosyalar

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Uygulama �al��t�rma
app.Run();
