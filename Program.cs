
using System.Net;
using App.AppMVC;
using App.Data;
using App.ExtendMethods;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Identity;
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

//Dang ky Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
  .AddEntityFrameworkStores<AppDbContext>()
  .AddDefaultTokenProviders();
// Truy cập IdentityOptions
builder.Services.Configure<IdentityOptions>(options =>
{
  // Thiết lập về Password
  options.Password.RequireDigit = false; // Không bắt phải có số
  options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
  options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
  options.Password.RequireUppercase = false; // Không bắt buộc chữ in
  options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
  options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

  // Cấu hình Lockout - khóa user
  options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
  options.Lockout.MaxFailedAccessAttempts = 3; // Thất bại 5 lầ thì khóa
  options.Lockout.AllowedForNewUsers = true;

  // Cấu hình về User.
  options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
      "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
  options.User.RequireUniqueEmail = true; // Email là duy nhất

  // Cấu hình đăng nhập.
  options.SignIn.RequireConfirmedEmail = true; // Cấu hình xác thực địa chỉ email (email phải tồn tại)
  options.SignIn.RequireConfirmedPhoneNumber = false; // Xác thực số điện thoại
  options.SignIn.RequireConfirmedAccount = true;

});

builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
  //Trên 30 giây truy cập lại sẽ nạp lại thông tin User (Role)
  //SercurityStamp trong bảng User đổi -> nạp lại thông tin Security
  options.ValidationInterval = TimeSpan.FromSeconds(30);
});

builder.Services.ConfigureApplicationCookie(options =>
{
  options.LoginPath = "/login/";
  options.LogoutPath = "/logout/";
  options.AccessDeniedPath = "/khongduoctruycap.html";
});

//Đăng nhập bên thứ 3
builder.Services.AddAuthentication()
  .AddGoogle(googleOptions =>
  {
    // Đọc thông tin Authentication:Google từ appsettings.json
    IConfigurationSection googleAuthNsection = configuration.GetSection("Authentication:Google");

    // Thiết lập ClientID và ClientSecret để truy cập API google
    googleOptions.ClientId = googleAuthNsection["ClientId"];
    googleOptions.ClientSecret = googleAuthNsection["ClientSecret"];
    // Cấu hình Url callback lại từ Google (không thiết lập thì mặc định là /signin-google)
    googleOptions.CallbackPath = "/dang-nhap-tu-google";
  });

builder.Services.AddOptions();  //Kích hoat Options
var mailsettings = configuration.GetSection("MailSettings");  // Đọc config
builder.Services.Configure<MailSettings>(mailsettings);       //Đăng ký đê inject
// Đăng ký SendMailService với kiểu Transient, mỗi lần gọi dịch
// vụ ISendMailService một đới tượng SendMailService tạo ra (đã inject config)
builder.Services.AddSingleton<IEmailSender, SendMailService>();
builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();

builder.Services.AddAuthorization(options =>
{
  options.AddPolicy("ViewManageMenu", builder =>
  {
    builder.RequireAuthenticatedUser();
    builder.RequireRole(RoleName.Administrator);
  });
});

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

  // endpoints.MapControllers
  // endpoints.MapControllerRoute
  // endpoints.MapDefaultControllerRoute
  // endpoints.MapAreaControllerRoute

  // URL = start-here
  // controller =>
  // action =>
  // area =>

  // URL: /{controller}/{action}/{id?}
  // First/Index
  endpoints.MapControllerRoute(
      name: "default",
      pattern: "{controller=Home}/{action=Index}/{id?}");

  endpoints.MapRazorPages();
});

app.MapRazorPages();

app.Run();
