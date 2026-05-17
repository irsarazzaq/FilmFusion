using FilmFusion.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// FIXED: Registering active gateway network client pipeline for automated API seeding
builder.Services.AddHttpClient();

// DB Context Integration configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session state storage setup injection - FIXED Capitalization
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;   // Fixed: Cookie property ke andar hota hai
    options.Cookie.IsEssential = true; // Fixed: Cookie property ke andar hota hai
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Essential session state activator configuration middleware
app.UseSession();
app.UseAuthorization();

// Starting flow controller mapping routing target sequence
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Splash}/{id?}");

app.Run();