using App.Models.Blog;
using Microsoft.AspNetCore.Mvc;

namespace App.Components
{
  [ViewComponent]
  public class CategorySideBar : ViewComponent
  {
    public class CategorySidebarData
    {
      public List<Category> Categories { get; set; }
      public int level {get;set;}
      public string categorySlug { get; set; }
    }
    public IViewComponentResult Invoke(CategorySidebarData data)
    {
      return View(data);
    }
  }
}