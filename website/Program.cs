using Stripe;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{    
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// In production scenario, make sure to move this to the appsettings.json for easy configuration
StripeConfiguration.ApiKey = "sk_test_51PEwawBYQqea3LZFnARTWx0YOFopVH6NyFXp8ZOf0yBTH3ROGTyKt7ZtzpSTP9wRZsCGk9J5IhvFUxCDnf8MHEoV009sKRnoQD";

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
