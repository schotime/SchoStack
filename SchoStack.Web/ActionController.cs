using System;
using System.Web.Mvc;
using SchoStack.Web.ActionControllers;
using SchoStack.Web.Url;

namespace SchoStack.Web
{
    public class ActionController : Controller, IHaveNamingConventions
    {
        protected override void ExecuteCore()
        {
            if (!ControllerContext.IsChildAction)
            {
                TempData.Load(ControllerContext, TempDataProvider);
            }
            try
            {
                string httpMethod = ControllerContext.IsChildAction ? "Get" : ControllerContext.HttpContext.Request.HttpMethod;
                if (!ActionInvoker.InvokeAction(ControllerContext, httpMethod) && !ActionInvoker.InvokeAction(ControllerContext, "Execute"))
                {
                    HandleUnknownAction(httpMethod);
                }
            }
            finally
            {
                if (!ControllerContext.IsChildAction)
                {
                    TempData.Save(ControllerContext, TempDataProvider);
                }
            }
        }

        public INamingConventions NamingConventions { get; set; }

        public RedirectResult RedirectTo(object model)
        {
            return Redirect(Url.For(model));
        }

        public RedirectToRouteResult RedirectToGet()
        {
            return RedirectToAction(NamingConventions.BuildActionFromType(GetType()), NamingConventions.BuildControllerFromType(GetType()));
        }

        public ActionResult GetActionResult<T>(T input, Func<HandleActionBuilder<T>, IReturnActionResult> action)
        {
            var builder1 = new HandleActionBuilder<T>(input, Invoker);
            var result = action(builder1);
            return result.Result()(ControllerContext);
        }

        public ActionResult GetActionResult(Func<HandleActionBuilder, IReturnActionResult> action)
        {
            var builder1 = new HandleActionBuilder(Invoker);
            var result = action(builder1);
            return result.Result()(ControllerContext);
        }
        
        public IInvoker Invoker { get; set; }
        public TOutput Invoke<TOutput>(object inputModel)
        {
            return Invoker.Execute<TOutput>(inputModel);
        }

        public TOutput Invoke<TOutput>()
        {
            return Invoker.Execute<TOutput>();
        }

        public void Invoke(object inputModel)
        {
            Invoker.Execute(inputModel);
        }
    }
}