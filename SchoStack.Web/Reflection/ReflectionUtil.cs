using System;
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

        public static Accessor GetAccessor<TModel>(Expression<Func<TModel, object>> expression)
        {
            if (expression.Body.NodeType == ExpressionType.Convert)
            {
                return ReflectionHelper.GetAccessor(((UnaryExpression)expression.Body).Operand);
            }
            return ReflectionHelper.GetAccessor(expression);
        }
    }
}