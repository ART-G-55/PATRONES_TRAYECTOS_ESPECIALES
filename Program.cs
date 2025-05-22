using Npgsql;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicio para la conexión Npgsql
builder.Services.AddScoped<NpgsqlConnection>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("PostgresConnection");
    return new NpgsqlConnection(connectionString);
});

// Para usar sesiones:
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // tiempo de expiración
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Agregar servicios para controladores con vistas (¡IMPORTANTE que esté antes de Build!)
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
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
