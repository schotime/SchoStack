using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FubuCore.Reflection;
using SchoStack.Web.Conventions.Core;

namespace SchoStack.Web
{
    public class InputPropertyMatcher
    {
        public static List<PropertyInfo> FindPropertyData(RequestData r)
        {
            if (r.InputType == null)
                return new List<PropertyInfo>();

            var properties = new List<PropertyInfo>();
            if (r.Accessor is SingleProperty)
            {
                var prop = r.InputType.GetProperty(r.Accessor.InnerProperty.Name);
                if (prop != null)
                {
                    properties.Add(prop);   
                }
            }
            else if (r.Accessor is PropertyChain)
            {
                var chain = r.Accessor as PropertyChain;
                Type type = r.InputType;
                PropertyInfo prop = r.Accessor.InnerProperty;

                foreach (var valueGetter in chain.ValueGetters)
                {
                    if (valueGetter.DeclaringType.IsArray)
                    {
                        type = prop.PropertyType.GetElementType();
                    }
                    else if (IsGenericList(valueGetter.DeclaringType))
                    {
                        type = prop.PropertyType.GetGenericArguments().First();
                    }
                    else
                    {
                        prop = type.GetProperty(valueGetter.Name);
                        if (prop == null)
                            return new List<PropertyInfo>();
                        type = prop.PropertyType;
                        properties.Add(prop);
                    }
                }
            }

            return properties;
        }

        private static bool IsGenericList(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            foreach (Type @interface in type.GetInterfaces())
            {
                if (@interface.IsGenericType)
                {
                    if (@interface.GetGenericTypeDefinition() == typeof(ICollection<>))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}