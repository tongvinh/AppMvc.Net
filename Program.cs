
using System.Net;
using App.AppMVC;
using App.ExtendMethods;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

ConfigurationManager configuration = builder.Configuration;
IWebHostEnvironment environment = builder.Environment;

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(options =>
{
  string connectString = configuration.GetConnectionString("AppMvcConnectionString");
  options.UseSqlServer(connectString);
});
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

app.AddStatusCodePages(); // tuỳ biến thông tin lỗi Response : 400 -599

app.UseEndpoints(endpoints =>
{
  //
  endpoints.MapGet("/sayhi", async (context) =>
  {
    await context.Response.WriteAsync($"Hello ASP.NET MVC {DateTime.Now}");
  });

  // endpoints.MapControllers
  // endpoints.MapControllerRoute
  // endpoints.MapDefaultControllerRoute
  // endpoints.MapAreaControllerRoute

  // URL = start-here
  // controller =>
  // action =>
  // area =>

  endpoints.MapControllerRoute(
    name: "first",
    pattern: "{url:regex(^((xemsanpham)|(viewproduct))$)}/{id:range(2,4)}",
    defaults: new
    {
      controller = "First",
      action = "ViewProduct"
    }
  // constraints: new {
  // url = "xemsanpham",
  // url = new RegexRouteConstraint(@"^((xemsanpham)|(viewproduct))$"),
  // id = new RangeRouteConstraint(2,4)
  // }
  );
  endpoints.MapControllerRoute(
    name: "firstroute",
    pattern: "start-here/{controller=Home}/{action=Index}/{id?}" // start-here, start-here/2, start-here/...
                                                                 // defaults: new
                                                                 // {
                                                                 // controller = "First",
                                                                 // action = "ViewProduct",
                                                                 // id = 3
                                                                 // }
    );

  endpoints.MapRazorPages();
});

app.MapRazorPages();

app.Run();
