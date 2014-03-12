using System.Linq.Expressions;
using System.Reflection;
using FubuCore.Reflection;

namespace SchoStack.Web.Reflection
{
    public class ReflectionUtil
    {
        public static Accessor GetAccessor(PropertyInfo property)
        {
            return ReflectionHelper.GetAccessor(Expression.Property(Expression.Parameter(property.DeclaringType), property));
        }
    }
}