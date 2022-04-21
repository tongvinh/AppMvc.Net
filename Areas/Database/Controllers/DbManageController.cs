using App.Data;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Database.Controllers
{
  [Area("Database")]
  [Route("/database-manage/[action]")]
  public class DbManageController : Controller
  {
    private readonly AppDbContext _dbContext;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;
    public DbManageController(AppDbContext dbContext, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
    {
      _userManager = userManager;
      _roleManager = roleManager;
      _dbContext = dbContext;

    }
    public IActionResult Index()
    {
      return View();
    }
    [HttpGet]
    public IActionResult DeleteDb()
    {
      return View();
    }
    [TempData]
    public string StatusMessage { set; get; }

    [HttpPost]
    public async Task<IActionResult> DeleteDbAsync()
    {
      var success = await _dbContext.Database.EnsureDeletedAsync();
      StatusMessage = success ? "Xoá Database thành công" : "Không xoá được Db";
      return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> Migrate()
    {
      await _dbContext.Database.MigrateAsync();
      StatusMessage = "Cập nhật Database thành công";
      return RedirectToAction(nameof(Index));
    }
    public async Task<IActionResult> SeedDataAsync()
    {
      var rolenames = typeof(RoleName).GetFields().ToList();
      foreach (var r in rolenames)
      {
        var rolename = (string)r.GetRawConstantValue();
        var rfound = await _roleManager.FindByNameAsync(rolename);
        if (rfound == null)
        {
          await _roleManager.CreateAsync(new IdentityRole(rolename));
        }
      }
      // admin, pass=admin123
      var useradmin = await _userManager.FindByEmailAsync("admin");
      if (useradmin == null)
      {
        useradmin = new AppUser
        {
          UserName = "admin",
          Email = "vinhtq.dev@gmail.com",
          EmailConfirmed = true
        };
        await _userManager.CreateAsync(useradmin, "admin123");
        await _userManager.AddToRoleAsync(useradmin, RoleName.Administrator);
      }
      StatusMessage = "Vừa seed Database";
      return RedirectToAction("Index");
    }
  }
}