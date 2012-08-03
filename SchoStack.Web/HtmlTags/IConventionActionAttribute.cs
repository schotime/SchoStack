using System;
using HtmlTags;

namespace SchoStack.Web.HtmlTags
{
    public interface IConventionActionAttribute<TAttribute>
    {
        void BuildBy(Func<RequestData, TAttribute, HtmlTag> builder);
        void Modify(Action<HtmlTag, RequestData, TAttribute> modifier);
    }
}