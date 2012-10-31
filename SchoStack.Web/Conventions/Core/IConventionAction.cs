using System;
using HtmlTags;

namespace SchoStack.Web.Conventions.Core
{
    public interface IConventionAction
    {
        void BuildBy(Func<RequestData, HtmlTag> builder);
        void Modify(Action<HtmlTag, RequestData> modifier);
    }
}