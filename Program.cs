using FHIR_app_Challenge.Models;
using FHIR_app_Challenge.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
var fhirOptions = builder.Configuration.GetSection("FHIR").Get<FhirOptions>() ?? new FhirOptions();
builder.Services.AddHttpClient<FhirPatientClient>();
builder.Services.AddSingleton(fhirOptions);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
