namespace App.AppMVC
{
  public class StartUp
  {
    public static string ContentRootPath {get;set;}
    public  StartUp(IWebHostEnvironment env)
    {
      ContentRootPath = env.ContentRootPath;
    }
  }
}