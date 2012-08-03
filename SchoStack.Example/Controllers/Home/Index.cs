using System.Web.Mvc;
using SchoStack.Web;

namespace SchoStack.Example.Controllers.Home
{
    [Route("/")]
    public class Index : ActionController
    {
        public ActionResult Get(HomeIndexQueryModel query)
        {
            ViewBag.Message = "Welcome to ASP.NET MVC!";

            return View();
        }
    }

    public class HomeIndexQueryModel
    {
    }
}