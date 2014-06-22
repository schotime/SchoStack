using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace SchoStack.Web
{
    public class ActionDictionary : Dictionary<Type, ActionInfo> {}

    public class ActionFactory
    {
        public static ActionDictionary Actions = new ActionDictionary();

        public static Dictionary<Type, Func<object, UrlHelper, string>> TypeFormatters = new Dictionary<Type, Func<object, UrlHelper, string>>();
        public static Dictionary<Type, Func<string, object>> StringToTypeConverters = new Dictionary<Type, Func<string, object>>();

        public static Func<string, string> PropertyNameModifier = x => x;

        static void AddTypeFormatter<T>(Func<object, UrlHelper, string> formatter)
        {
            TypeFormatters.Add(typeof(T), formatter);
        }
    }
}
