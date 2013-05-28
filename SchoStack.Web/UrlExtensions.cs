using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

namespace SchoStack.Web.Url
{
    public static class UrlExtensions
    {
        //public static string For<T>(this UrlHelper urlHelper) where T : new()
        //{
        //    return For(urlHelper, new T());
        //}

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

        public static string For<T>(this UrlHelper urlHelper, params Action<T>[] modifiers) where T : new()
        {
            var model = new T();
            ActionInfo actionInfo;
            if (ActionFactory.Actions.TryGetValue(model.GetType(), out actionInfo))
            {
                var modelType = model.GetType();
                foreach (string key in urlHelper.RequestContext.HttpContext.Request.QueryString.AllKeys)
                {
                    var val = urlHelper.RequestContext.HttpContext.Request.QueryString[key];
                    var prop = modelType.GetProperty(key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        try
                        {
                            object conValue = val;
                            if (ActionFactory.StringToTypeConverters.ContainsKey(prop.PropertyType))
                                conValue = ActionFactory.StringToTypeConverters[prop.PropertyType](val);

                            var u = Nullable.GetUnderlyingType(prop.PropertyType);
                            object newval = Convert.ChangeType(conValue, u ?? prop.PropertyType);
                            prop.SetValue(model, newval, null);
                        }
                        catch {}
                    }
                }
                foreach (var modifier in modifiers)
                {
                    modifier(model);
                }
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
                if (val == null || Equals(val, GetDefault(prop.PropertyType)))
                {
                    continue;
                }
                var key = ActionFactory.PropertyNameModifier(prop.Name);

                if (ActionFactory.TypeFormatters.ContainsKey(prop.PropertyType))
                    val = ActionFactory.TypeFormatters[prop.PropertyType](val);
                
                dict[key] = val;
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