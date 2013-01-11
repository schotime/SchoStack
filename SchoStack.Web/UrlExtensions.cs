using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Web.Routing;

namespace SchoStack.Web.Url
{
    public static class UrlExtensions
    {
        public static string For<T>(this UrlHelper urlHelper) where T : new()
        {
            return For(urlHelper, new T());
        }

        public static string For<T>(this UrlHelper urlHelper, T model)
        {
            ActionInfo actionInfo;
            if (ActionFactory.Actions.TryGetValue(model.GetType(), out actionInfo))
            {
                var dict = CreateRouteValueDictionary(model);
                var url = urlHelper.RouteUrl(model.GetType().FullName, dict);
                return url;
            }
            throw new Exception("Type specified cannot be found to generate Url");
        }

        public static RouteValueDictionary CreateRouteValueDictionary(object model)
        {
            var dict = new RouteValueDictionary();
            foreach (var prop in model.GetType().GetProperties())
            {
                if (prop.PropertyType != typeof(string) && prop.PropertyType.IsClass)
                    continue;
                var val = prop.GetValue(model, null);
                if (val == null || Equals(val, GetDefault(val.GetType())))
                    continue;

                if (ActionFactory.TypeFormatters.ContainsKey(prop.PropertyType))
                    val = ActionFactory.TypeFormatters[prop.PropertyType](val);
                
                dict.Add(ActionFactory.PropertyNameModifier(prop.Name), val);
            }
            return dict;
        }

        private static readonly Dictionary<Type, object> Defaults = new Dictionary<Type, object>()
                                                                    {
                                                                        {typeof (bool), false},
                                                                        {typeof (int), new int()},
                                                                        {typeof (DateTime), new DateTime()},
                                                                        {typeof (decimal), new decimal()},
                                                                        {typeof (float), new float()},
                                                                        {typeof (double), new double()},
                                                                    };
        static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                object val;
                if (Defaults.TryGetValue(type, out val))
                    return val;
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}