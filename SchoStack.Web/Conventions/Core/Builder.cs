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

        public Builder(Func<RequestData, bool> condition, Func<RequestData, IConventionPipeline, HtmlTag> builder)
        {
            Condition = condition;
            BuilderFuncPipeline = builder;
        }

        public Func<RequestData, bool> Condition { get; private set; }
        public Func<RequestData, HtmlTag> BuilderFunc { get; private set; }
        public Func<RequestData, IConventionPipeline, HtmlTag> BuilderFuncPipeline { get; private set; }
    }
}