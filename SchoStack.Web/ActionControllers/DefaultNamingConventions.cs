using System;
using System.Linq;

namespace SchoStack.Web.ActionControllers
{
    public class DefaultNamingConventions : INamingConventions
    {
        public string BuildKeyFromType(Type type)
        {
            return BuildKeyFromControllerAndAction(BuildControllerFromType(type), BuildActionFromType(type));
        }

        public string BuildKeyFromControllerAndAction(string controllerName, string actionName)
        {
            return controllerName.ToLowerInvariant() + "." + actionName.ToLowerInvariant();
        }

        public string BuildControllerFromType(Type type)
        {
            var key = type.Namespace.Split('.').Last();
            return key;
        }

        public string BuildActionFromType(Type type)
        {
            return type.Name;
        }
    }
}