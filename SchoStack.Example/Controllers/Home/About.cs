using System.Web.Mvc;
using SchoStack.Web;

namespace SchoStack.Example.Controllers.Home
{
    [Route("/about")]
    public class About : ActionController
    {
        public ActionResult Get(HomeAboutQueryModel query)
        {
            return View();
        }
    }

    public class HomeAboutQueryModel
    {
    }
}
