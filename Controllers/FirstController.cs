using App.AppMVC;
using App.Services;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers
{
  public class FirstController : Controller
  {
    private readonly ILogger<FirstController> _logger;
    private readonly ProductService _productService;

    public FirstController(ILogger<FirstController> logger, ProductService productService)
    {
      _productService = productService;
      _logger = logger;
    }
    public string Index()
    {
      // this.HttpContext
      // this.Request
      // this.Response
      // this.RouteData

      // this.User
      // this.ModelState
      // this.ViewData
      // this.ViewBag
      // this.Url
      // this.TempData

      // logger.Log(LogLevel.Warning, "Thong bao abc");
      _logger.LogWarning("Thong bao");
      _logger.LogError("Thong bao");
      _logger.LogDebug("Thong bao");
      _logger.LogCritical("Thong bao");
      _logger.LogInformation("Index Action");
      // Serilog

      return "Toi la Index cua First";
    }
    public void Nothing()
    {
      _logger.LogInformation("Nothing Action");
      Response.Headers.Add("hi", "Xin chao cac ban");
    }
    public object Anything() => new int[] { 1, 2, 3 };

    //IActionResult
    // ContentResult               | Content()
    // EmptyResult                 | new EmptyResult()
    // FileResult                  | File()
    // ForbidResult                | Forbid()
    // JsonResult                  | Json()
    // LocalRedirectResult         | LocalRedirect()
    // RedirectResult              | Redirect()
    // RedirectToActionResult      | RedirectToAction()
    // RedirectToPageResult        | RedirectToRoute()
    // RedirectToRouteResult       | RedirectToPage()
    // PartialViewResult           | PartialView()
    // ViewComponentResult         | ViewComponent()
    // StatusCodeResult            | StatusCode()
    // ViewResult                  | View()
    public IActionResult Bird()
    {
      return Content("test path" + StartUp.ContentRootPath);
    }
    public IActionResult IphonePrice()
    {
      return Json(
        new
        {
          productName = "Iphone X",
          Price = 1000
        }
      );
    }
    public IActionResult Privacy()
    {
      var url = Url.Action("Privacy", "Home");
      _logger.LogInformation("Chuyen huong den " + url);
      return LocalRedirect(url!);
    }

    public IActionResult ViewProduct(int? id)
    {
      var product = _productService.Where(p => p.Id == id).FirstOrDefault();
      if (product == null)
        return NotFound();
        
      // Model
      // return View(product);
      // ViewData
      // this.ViewData ["product"] = product;
      // ViewData ["Title"] = product.Name;
      // return View("ViewProduct2");
      ViewBag.product = product;
      return View("ViewProduct3");

    }
  }
}