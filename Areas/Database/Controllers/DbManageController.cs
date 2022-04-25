using App.Data;
using App.Models;
using App.Models.Blog;
using Bogus;
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
      var useradmin = await _userManager.FindByEmailAsync("vinhtq.dev@gmail.com");
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

      SeedPostCategory();

      StatusMessage = "Vừa seed Database";
      return RedirectToAction("Index");
    }

    private void SeedPostCategory()
    {
      _dbContext.Categories.RemoveRange(_dbContext.Categories.Where(c => c.Description.Contains("[fakeData]")));
      _dbContext.Posts.RemoveRange(_dbContext.Posts.Where(p => p.Content.Contains("[fakeDate]")));

      var fakerCategory = new Faker<Category>();
      int cm = 1;
      fakerCategory.RuleFor(c => c.Title, fk => $"CM{cm++} " + fk.Lorem.Sentence(1, 2).Trim('.'));
      fakerCategory.RuleFor(c => c.Description, fk => fk.Lorem.Sentence(5) + "[fakeData]");
      fakerCategory.RuleFor(c => c.Slug, fk => fk.Lorem.Slug());

      var cate1 = fakerCategory.Generate();
      var cate11 = fakerCategory.Generate();
      var cate12 = fakerCategory.Generate();
      var cate2 = fakerCategory.Generate();
      var cate21 = fakerCategory.Generate();
      var cate211 = fakerCategory.Generate();


      cate11.ParentCategory = cate1;
      cate12.ParentCategory = cate1;
      cate21.ParentCategory = cate2;
      cate211.ParentCategory = cate21;

      var categories = new Category[] { cate1, cate2, cate12, cate11, cate21, cate211 };
      _dbContext.Categories.AddRange(categories);

      // POST
      var rCateIndex = new Random();
      int bv = 1;

      var user = _userManager.GetUserAsync(this.User).Result;
      var fakerPost = new Faker<Post>();
      fakerPost.RuleFor(p => p.AuthorId, f => user.Id);
      fakerPost.RuleFor(p => p.Content, f => f.Lorem.Paragraphs(7) + "[fakeData]");
      fakerPost.RuleFor(p => p.DateCreated, f => f.Date.Between(new DateTime(2021, 1, 1), new DateTime(2021, 7, 1)));
      fakerPost.RuleFor(p => p.Description, f => f.Lorem.Sentences(3));
      fakerPost.RuleFor(p => p.Published, f => true);
      fakerPost.RuleFor(p => p.Slug, f => f.Lorem.Slug());
      fakerPost.RuleFor(p => p.Title, f => $"Bài {bv++} " + f.Lorem.Sentence(3, 4).Trim('.'));

      List<Post> posts = new List<Post>();
      List<PostCategory> post_categories = new List<PostCategory>();

      for (int i = 0; i < 40; i++)
      {
        var post = fakerPost.Generate();
        post.DateUpdated = post.DateCreated;
        posts.Add(post);
        post_categories.Add(new PostCategory()
        {
          Post = post,
          Category = categories[rCateIndex.Next(5)]
        });
      }

      _dbContext.AddRange(posts);
      _dbContext.AddRange(post_categories);
      // END POST

      _dbContext.SaveChanges();
    }
  }
}