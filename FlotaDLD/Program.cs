#pragma warning disable CA1416
using FlotaLuchitoWeb.Data;
using FlotaLuchitoWeb.Services;

var builder = WebApplication.CreateBuilder(args);

var appDataPath = Path.Combine(builder.Environment.ContentRootPath, "App_Data");
Directory.CreateDirectory(appDataPath);
AppDomain.CurrentDomain.SetData("DataDirectory", appDataPath);

builder.Services.AddRazorPages();

// Usamos sesión para recordar que el usuario ya inició sesión.
// Sin esto, cualquiera podría entrar directo a /Menu, /Vehiculos, etc.
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Registramos el DAO/DataHelper que se usa para hablar con la base de datos.
builder.Services.AddSingleton<DataBaseHelper>();

// Registramos el servicio del asistente de chat
builder.Services.AddSingleton<ChatAssistantService>();

// Registramos el servicio de códigos QR
builder.Services.AddSingleton<QrCodeService>();

// Registramos el servicio de síntesis de voz (Text-to-Speech)
builder.Services.AddSingleton<SpeechService>();
var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();