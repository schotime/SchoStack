using System;

namespace SchoStack.Web.ActionControllers
{
    public interface INamingConventions
    {
        string BuildKeyFromType(Type type);
        string BuildKeyFromControllerAndAction(string controllerName, string actionName);
        string BuildControllerFromType(Type type);
        string BuildActionFromType(Type type);
    }
}