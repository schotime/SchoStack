using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
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
                var dict = GenerateDict(model);
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
                var controllerContext = new ControllerContext(urlHelper.RequestContext, new AController());
                var modelBound = (T)new DefaultModelBinder().BindModel(controllerContext, new ModelBindingContext()
                {
                    ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => (object) model, typeof (T)),
                    ModelName = null,
                    ModelState = new ModelStateDictionary(),
                    PropertyFilter = null,
                    ValueProvider = new NameValueCollectionValueProvider(urlHelper.RequestContext.HttpContext.Request.QueryString, CultureInfo.InvariantCulture)
                });

                foreach (var modifier in modifiers)
                {
                    modifier(modelBound);
                }

                var dict = GenerateDict(modelBound);
                var url = urlHelper.RouteUrl(typeof(T).FullName, dict);
                return url;
            }
            throw new Exception("Type specified cannot be found to generate Url");
        }
        
        private class AController : ControllerBase
        {
            protected override void ExecuteCore()
            {
                
            }
        }

        public static RouteValueDictionary GenerateDict(object o, string prefix = "", RouteValueDictionary dict = null)
        {
            if (o == null)
                return dict;
            
            Type t = o.GetType();

            dict = dict ?? new RouteValueDictionary();
            var sb = new StringBuilder();

            foreach (PropertyInfo p in t.GetProperties())
            {
                if (p.PropertyType.IsEnum || IsConvertible(p.PropertyType))
                {
                    var val = p.GetValue(o, null);
                    if (val == null || Equals(val, GetDefault(p.PropertyType)))
                        continue;

                    if (ActionFactory.TypeFormatters.ContainsKey(p.PropertyType))
                        val = ActionFactory.TypeFormatters[p.PropertyType](val);

                    var key = ActionFactory.PropertyNameModifier(prefix + p.Name);
                    dict.Add(key, Convert.ToString(val));
                }
                else if (IsEnumerable(p.PropertyType))
                {
                    var i = 0;
                    foreach (object sub in (IEnumerable)p.GetValue(o, null) ?? new object[0])
                    {
                        GenerateDict(sub, prefix + p.Name + "[" + (i++) + "]" + ".", dict);
                    }
                }
                else if (SimpleGetter(p))
                {
                    GenerateDict(p.GetValue(o, null), prefix + p.Name + ".", dict);
                }
            }

            if (IsEnumerable(t))
            {
                foreach (object sub in (IEnumerable)o)
                {
                    sb.Append(GenerateDict(sub, dict: dict));
                }
            }

            return dict;
        }

        internal static Type[] ConvertibleTypes =
        {
            typeof (bool), typeof (byte), typeof (char),
            typeof (DateTime), typeof (decimal), typeof (double), typeof (float), typeof (int),
            typeof (long), typeof (sbyte), typeof (short), typeof (string), typeof (uint),
            typeof (ulong), typeof (ushort)
        };

        /// <summary>
        /// Returns true if this Type matches any of a set of Types.
        /// </summary>
        /// <param name="type">This type.</param>
        /// <param name="types">The Types to compare this Type to.</param>
        public static bool In(Type type, params Type[] types)
        {
            foreach (Type t in types)
            {
                if (t.IsAssignableFrom(type) || (Nullable.GetUnderlyingType(type) != null && t.IsAssignableFrom(Nullable.GetUnderlyingType(type))))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if this Type is one of the types accepted by Convert.ToString() 
        /// (other than object).
        /// </summary>
        public static bool IsConvertible(Type t)
        {
            return In(t, ConvertibleTypes);
        }

        /// <summary>
        /// Gets whether this type is enumerable.
        /// </summary>
        public static bool IsEnumerable(Type t)
        {
            return typeof(IEnumerable).IsAssignableFrom(t);
        }

        /// <summary>
        /// Returns true if this property's getter is public, has no arguments, and has no 
        /// generic type parameters.
        /// </summary>
        public static bool SimpleGetter(PropertyInfo info)
        {
            MethodInfo method = info.GetGetMethod(false);
            return method != null && method.GetParameters().Length == 0 && method.GetGenericArguments().Length == 0;
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