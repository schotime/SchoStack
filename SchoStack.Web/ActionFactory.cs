using System;
using System.Collections.Generic;

namespace SchoStack.Web
{
    public class ActionDictionary : Dictionary<Type, ActionInfo> {}

    public class ActionFactory
    {
        public static ActionDictionary Actions = new ActionDictionary();

        public static Dictionary<Type, Func<object, string>> TypeFormatters = new Dictionary<Type, Func<object, string>>();
        public static Dictionary<Type, Func<string, object>> StringToTypeConverters = new Dictionary<Type, Func<string, object>>();

        public static Func<string, string> PropertyNameModifier = x => x;

        static void AddTypeFormatter<T>(Func<object, string> formatter)
        {
            TypeFormatters.Add(typeof(T), formatter);
        }
    }
}
