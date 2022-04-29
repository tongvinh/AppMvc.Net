using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Models;
using App.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using App.Utilities;
using AppMvc.Areas.Product.Models;
using App.Models.Product;

namespace AppMvc.Areas.Blog.Controllers
{
  [Area("Product")]
  [Route("admin/productmanage/[action]/{id?}")]
  [Authorize(Roles = RoleName.Administrator + "," + RoleName.Editor)]
  public class ProductManageController : Controller
  {
    private readonly AppDbContext _context;
    private readonly UserManager<AppUser> _userManager;

    public ProductManageController(AppDbContext context, UserManager<AppUser> userManager)
    {
      _context = context;
      _userManager = userManager;
    }
    [TempData]
    public string StatusMessage { get; set; }
    // GET: Blog/Product
    public async Task<IActionResult> Index([FromQuery(Name = "p")] int currentPage, int pagesize)
    {
      var products = _context.Products
        .Include(p => p.Author)
        .OrderByDescending(p => p.DateUpdated);

      int totalProducts = await products.CountAsync();
      if (pagesize <= 0) pagesize = 10;
      int countPages = (int)Math.Ceiling((double)totalProducts / pagesize);

      if (currentPage > countPages) currentPage = countPages;
      if (currentPage < 1) currentPage = 1;

      var pagingModel = new PagingModel()
      {
        countpages = countPages,
        currentpage = currentPage,
        generateUrl = (pageNumber) => Url.Action("Index", new
        {
          p = pageNumber,
          pagesize = pagesize
        })
      };

      ViewBag.pagingModel = pagingModel;
      ViewBag.totalProducts = totalProducts;

      ViewBag.productIndex = (currentPage - 1) * pagesize;

      var productsInPage = await products.Skip((currentPage - 1) * pagesize)
                       .Take(pagesize)
                       .Include(p => p.ProductCategoryProduct)
                       .ThenInclude(pc => pc.Category)
                       .ToListAsync();
      return View(productsInPage);
    }
    // GET: Blog/Product/Details/5
    public async Task<IActionResult> Details(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }

      var product = await _context.Products
                .Include(p => p.Author)
                .FirstOrDefaultAsync(m => m.ProductId == id);
      if (product == null) return NotFound();

      return View(product);
    }
    // GET: Blog/Product/Create
    public async Task<IActionResult> CreateAsync()
    {
      var categories = await _context.CategoryProducts.ToListAsync();
      ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

      return View();
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,Slug,Content,Published,CategoryIDs,Price")] CreateProductModel product)
    {
      var categories = await _context.CategoryProducts.ToListAsync();
      ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

      if (product.Slug == null)
        product.Slug = AppUtilities.GenerateSlug(product.Title);

      if (await _context.Products.AnyAsync(p => p.Slug == product.Slug))
      {
        ModelState.AddModelError("Slug", "Nhập chuổi Url khác");
        return View(product);
      }

      if (ModelState.IsValid)
      {
        var user = await _userManager.GetUserAsync(this.User);
        product.DateCreated = product.DateUpdated = DateTime.Now;
        product.AuthorId = user.Id;
        _context.Add(product);

        if (product.CategoryIDs != null)
        {
          foreach (var CateId in product.CategoryIDs)
          {
            _context.Add(new ProductCategoryProduct()
            {
              CategoryID = CateId,
              Product = product
            });
          }
        }

        await _context.SaveChangesAsync();
        StatusMessage = "Vừa tạo bài viết mới";
        return RedirectToAction(nameof(Index));
      }

      return View(product);
    }
    // GET: Blog/Product/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
      if (id == null)
        return NotFound();
      var product = await _context.Products.Include(p => p.ProductCategoryProduct).FirstOrDefaultAsync(p => p.ProductId == id);
      if (product == null)
      {
        return NotFound();
      }

      var productEdit = new CreateProductModel()
      {
        ProductId = product.ProductId,
        Title = product.Title,
        Content = product.Content,
        Description = product.Description,
        Slug = product.Slug,
        Published = product.Published,
        CategoryIDs = product.ProductCategoryProduct.Select(pc => pc.CategoryID).ToArray(),
        Price = product.Price
      };

      var categories = await _context.CategoryProducts.ToListAsync();
      ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

      return View(productEdit);
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("ProductId,Title,Description,Slug,Content,Published,CategoryIDs,Price")] CreateProductModel product)
    {
      if (id != product.ProductId)
      {
        return NotFound();
      }
      var categories = await _context.CategoryProducts.ToListAsync();
      ViewData["categories"] = new MultiSelectList(categories, "Id", "Title");

      if (product.Slug == null)
      {
        product.Slug = AppUtilities.GenerateSlug(product.Title);
      }
      if (await _context.Products.AnyAsync(p => p.Slug == product.Slug && p.ProductId != id))
      {
        ModelState.AddModelError("Slug", "Nhập chuỗi Url khác");
        return View(product);
      }
      if (ModelState.IsValid)
      {
        try
        {
          var productUpdate = await _context.Products.Include(p => p.ProductCategoryProduct).FirstOrDefaultAsync(p => p.ProductId == id);
          if (productUpdate == null)
          {
            return NotFound();
          }

          productUpdate.Title = product.Title;
          productUpdate.Description = product.Description;
          productUpdate.Content = product.Content;
          productUpdate.Published = product.Published;
          productUpdate.Slug = product.Slug;
          productUpdate.DateUpdated = DateTime.Now;
          productUpdate.Price = product.Price;

          //Update ProductCategory
          if (product.CategoryIDs == null) product.CategoryIDs = new int[] { };

          var oldCateIds = productUpdate.ProductCategoryProduct.Select(c => c.CategoryID).ToArray();
          var newCateIds = product.CategoryIDs;

          var removeCateProducts = from productCate in productUpdate.ProductCategoryProduct
                                   where (!newCateIds.Contains(productCate.CategoryID))
                                   select productCate;
          _context.ProductCategoryProducts.RemoveRange(removeCateProducts);

          var addCateIds = from CateId in newCateIds
                           where !oldCateIds.Contains(CateId)
                           select CateId;

          foreach (var CateId in addCateIds)
          {
            _context.ProductCategoryProducts.Add(new ProductCategoryProduct()
            {
              ProductID = id,
              CategoryID = CateId
            });
          }
          _context.Update(productUpdate);
          await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
          if (!ProductExists(product.ProductId))
          {
            return NotFound();
          }
          else
          {
            throw;
          }
        }
        StatusMessage = "Vừa cập nhật bài viết";
        return RedirectToAction(nameof(Index));
      }
      ViewData["AuthorId"] = new SelectList(_context.Users, "Id", "Id", product.AuthorId);
      return View(product);
    }

    // GET: Blog/Product/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
      if (id == null)
      {
        return NotFound();
      }
      var product = await _context.Products
      .Include(p => p.Author)
      .FirstOrDefaultAsync(m => m.ProductId == id);
      if (product == null)
      {
        return NotFound();
      }
      return View(product);
    }
    // POST: Blog/Product/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      var product = await _context.Products.FindAsync(id);

      if (product == null)
        return NotFound();

      _context.Products.Remove(product);
      await _context.SaveChangesAsync();
      StatusMessage = "Bạn vừa xoá bài viết: " + product.Title;

      return RedirectToAction(nameof(Index));
    }
    private bool ProductExists(int id)
    {
      return _context.Products.Any(e => e.ProductId == id);
    }
  }
}