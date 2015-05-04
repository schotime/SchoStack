using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FubuCore.Reflection;

namespace SchoStack.Web.Conventions.Core
{
    public class TagConventions : ITagConventions, IConventionAccessor
    {
        public bool IsAll { get; private set; }
        public IList<Modifier> Modifiers { get; private set; }
        public IList<Builder> Builders { get; private set; }

        public TagConventions() : this(false) { }
        public TagConventions(bool isAll)
        {
            IsAll = isAll;
            Modifiers = new List<Modifier>();
            Builders = new List<Builder>();
        }

        public IConventionAction Always
        {
            get { return new ConventionAction(x => true, Builders, Modifiers); }
        }

        public IConventionAction If(Func<RequestData, bool> condition)
        {
            return new ConventionAction(condition, Builders, Modifiers);
        }

        public IConventionAction If<T>()
        {
            return new ConventionAction(req => IsAssignable<T>(req), Builders, Modifiers);
        }

        public IConventionActionAttribute<TAttribute> IfAttribute<TAttribute>() where TAttribute : Attribute
        {
            return new ConventionActionAttribute<TAttribute>(Condition<TAttribute>(), Builders, Modifiers);
        }

        private static Func<RequestData, bool> Condition<TAttribute>() where TAttribute : Attribute
        {
            return req => GetPropertyInfo(req.Accessor).GetCustomAttributes(typeof (TAttribute), true).Length > 0;
        }
        
        public static PropertyInfo GetPropertyInfo(Accessor accessor)
        {
            if (accessor.InnerProperty != null)
                return accessor.InnerProperty;
            return accessor.Getters().OfType<PropertyValueGetter>().Last().PropertyInfo;
        }

        public static bool IsAssignable<TProperty>(RequestData x)
        {
            if (x.Accessor == null)
                return false;
            var type = typeof(TProperty);
            var assignable = type.IsAssignableFrom(x.GetPropertyType());
            //if (!assignable && type.IsValueType)
            //{
            //    assignable = typeof(Nullable<>).MakeGenericType(type).IsAssignableFrom(x.Accessor.PropertyType);
            //}
            return assignable;
        }
    }
}