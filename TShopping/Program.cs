using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using TShopping.Data;
using TShopping.Helpers;
using TShopping.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<TshoppingContext>(options =>
{
	var connectionString = builder.Configuration.GetConnectionString("TShopping_DB");
	options.UseSqlServer(connectionString);
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
	options.IOTimeout = TimeSpan.FromMinutes(10);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});
builder.Services.AddAutoMapper(typeof(AutoMapperProfile));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;
        options.LoginPath = "/DangNhap";
		options.LogoutPath = "/DangXuat";
		options.AccessDeniedPath = "/AccessDenied";
	});
// Đăng ký PaypalClient Service
builder.Services.AddSingleton(new PaypalClient(
		builder.Configuration["PaypalOptions:AppId"] ?? "",
		builder.Configuration["PaypalOptions:AppSecret"] ?? "",
		builder.Configuration["PaypalOptions:Mode"] ?? ""
    ));
builder.Services.AddSingleton<IVnPayService, VnPayService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}
app.UseCookiePolicy();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();

app.UseAuthorization();
app.MapControllerRoute(
	name: "areas",
	pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);
app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
