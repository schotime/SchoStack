using System;
using HtmlTags;

namespace SchoStack.Web.HtmlTags
{
    public interface IConventionAction
    {
        void BuildBy(Func<RequestData, HtmlTag> builder);
        void Modify(Action<HtmlTag, RequestData> modifier);
    }
}