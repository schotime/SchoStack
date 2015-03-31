using System;
using System.Diagnostics;
using System.Web.Mvc;
using SchoStack.Web;

namespace SchoStack.Example.Controllers.Home
{
    [Route("/about")]
    public class About : ActionController
    {
        public ActionResult Get(HomeAboutQueryModel query)
        {
            //Invoker.Execute<HomeAboutViewModel>(new HomeAboutQueryModel());
            //Invoker.Execute(new HomeAboutQueryModel());
            Invoker.Execute<HomeAboutViewModel>();

            return View();
        }
    }

    public class HomeAboutQueryModel
    {
    }

    public class HomeAboutViewModel
    {
    }

    public class HomeAboutHandler : ICommandHandler<HomeAboutQueryModel>
    {
        public void Handle(HomeAboutQueryModel model)
        {
            throw new Exception("Command Exception");
        }
    }

    public class HomeAboutHandler2 : IHandler<HomeAboutQueryModel, HomeAboutViewModel>
    {
        public HomeAboutViewModel Handle(HomeAboutQueryModel input)
        {
            throw new Exception("Handler with Input and Output");
        }
    }

    public class HomeAboutHandler3 : IHandler<HomeAboutViewModel>
    {
        public HomeAboutViewModel Handle()
        {
            throw new Exception("Handler with Output only");
        }
    }
}
