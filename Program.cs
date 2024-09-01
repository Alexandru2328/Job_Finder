using Job_Finder.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Job_Finder.Models;
using WebAppScraping.Controllers;
using Job_Finder.Services.AutoApplyService;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(option =>
{
    var conectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    option.UseSqlServer(conectionString);
});

builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = false).AddEntityFrameworkStores<AppDbContext>();
builder.Services.AddTransient<AutoApplyLinkedinService>();
builder.Services.AddTransient<AutoApplyIndeed>();
builder.Services.AddTransient<AutoApplyEJobs>();
builder.Services.AddTransient<AutoApplyBestJobsService>();
builder.Services.AddTransient<WebScrapingService>();
builder.Services.AddTransient<Automation>();
builder.Services.AddTransient<SaveJobs>();
builder.Services.AddTransient<NotificationServices>();
builder.Services.AddSingleton<AutomationProgress>();

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
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
