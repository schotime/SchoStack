using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace SchoStack.Web.ActionControllers
{
    public class ActionControllerFactory : DefaultControllerFactory
    {
        private readonly Dictionary<string, Type> _actionTypes;
        private readonly INamingConventions _namingConventions;
        private readonly Func<Type, object> _resolver;

        public ActionControllerFactory(Dictionary<string, Type> actionTypes, INamingConventions namingConventions, Func<Type, object> resolver)
        {
            _actionTypes = actionTypes;
            _namingConventions = namingConventions;
            _resolver = resolver;
        }

        public override IController CreateController(RequestContext requestContext, string controllerName)
        {
            string actionName = requestContext.RouteData.GetRequiredString("action");
            string key = _namingConventions.BuildKeyFromControllerAndAction(controllerName, actionName);

            ActionController actionInstance = null;

            try
            {
                Type t;
                if (_actionTypes.TryGetValue(key, out t))
                    actionInstance = _resolver(t) as ActionController;
            }
            catch (Exception ex)
            {
                throw new HttpException(404, "Controller not found", ex);
            }

            if (actionInstance == null)
            {
                throw new HttpException(404, "Controller not found");
            }
            
            actionInstance.NamingConventions = _namingConventions;
            actionInstance.Invoker = new Invoker(_resolver);

            return actionInstance;
        }
    }
}