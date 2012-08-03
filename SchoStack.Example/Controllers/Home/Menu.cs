using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SchoStack.Web;

namespace SchoStack.Example.Controllers.Home
{
    [Route("home/menu"), ChildActionOnly]
    public class Menu : ActionController
    {
        public ActionResult Execute(HomeMenuQueryModel query)
        {
            return PartialView();
        }
    }

    public class HomeMenuQueryModel
    {
        
    }
}
