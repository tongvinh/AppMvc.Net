
using App.AppMVC;
using App.Services;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
// builder.Services.AddTransient(typeof(ILogger<>), typeof(Logger<>)); //Serilog
builder.Services.Configure<RazorViewEngineOptions>(options =>
{
  // /View/Controller/Action.cshtml
  // /MyView/Controller/Action.cshtml

  //{0} -> Ten Action
  //{1} -> Ten Controller
  //{2} -> Ten Areas
  // options.ViewLocationFormats.Add("/MyView/{1}/{0}.cshtml");
  options.ViewLocationFormats.Add("/MyView/{1}/{0}" + RazorViewEngine.ViewExtension);
});

// builder.Services.AddSingleton<ProductService,ProductService>();
// builder.Services.AddSingleton(typeof(ProductService));
builder.Services.AddSingleton(typeof(ProductService), typeof(ProductService));

var app = builder.Build();
var startUp = new StartUp(environment);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); //Xac thuc danh tinh
app.UseAuthorization(); //Xác thuc quyen truy cap

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
