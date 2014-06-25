using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;
using FubuCore.Reflection;

namespace SchoStack.Web
{
    public class ActionDictionary : Dictionary<Type, ActionInfo> {}

    public class ActionFactory
    {
        public static ActionDictionary Actions = new ActionDictionary();

        public static Dictionary<Type, Func<object, RequestContext, string>> TypeFormatters = new Dictionary<Type, Func<object, RequestContext, string>>();

        public static DefaultPropertyNameModfier PropertyNameModifier = new DefaultPropertyNameModfier();

        static void AddTypeFormatter<T>(Func<object, RequestContext, string> formatter)
        {
            TypeFormatters.Add(typeof(T), formatter);
        }
    }

    public class DefaultPropertyNameModfier
    {
        public virtual string GetModifiedPropertyName(PropertyInfo propertyInfo)
        {
            if ((ModelBinders.Binders.DefaultBinder as AliasModelBinder) != null)
            {
                var bindAlias = propertyInfo.GetAttribute<BindAliasAttribute>();
                return bindAlias != null ? bindAlias.Alias : propertyInfo.Name;
            }
            return propertyInfo.Name;
        }
    }
}
