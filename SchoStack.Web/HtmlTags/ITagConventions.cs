using System;

namespace SchoStack.Web.HtmlTags
{
    public interface ITagConventions
    {
        IConventionAction Always { get; }
        IConventionAction If(Func<RequestData, bool> condition);
        IConventionAction If<T>();
        IConventionActionAttribute<TAttribute> IfAttribute<TAttribute>() where TAttribute : Attribute;
    }
}