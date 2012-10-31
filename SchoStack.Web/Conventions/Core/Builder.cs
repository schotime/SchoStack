using System;
using HtmlTags;

namespace SchoStack.Web.Conventions.Core
{
    public class Builder
    {
        public Builder(Func<RequestData, bool> condition, Func<RequestData, HtmlTag> builder)
        {
            Condition = condition;
            BuilderFunc = builder;
        }

        public Func<RequestData, bool> Condition { get; private set; }
        public Func<RequestData, HtmlTag> BuilderFunc { get; private set; }
    }
}