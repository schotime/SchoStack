using System;
using System.Collections.Generic;
using HtmlTags;

namespace SchoStack.Web.Conventions.Core
{
    public class ConventionAction : IConventionAction
    {
        protected readonly Func<RequestData, bool> Condition;
        protected readonly IList<Builder> Builders;
        protected readonly IList<Modifier> Modifiers;

        public ConventionAction(Func<RequestData, bool> condition, IList<Builder> builders, IList<Modifier> modifiers)
        {
            Condition = condition;
            Builders = builders;
            Modifiers = modifiers;
        }

        public void BuildBy(Func<RequestData, HtmlTag> builder)
        {
            Builders.Add(new Builder(Condition, builder));
        }

        public void Modify(Action<HtmlTag, RequestData> modifier)
        {
            Modifiers.Add(new Modifier(Condition, modifier));
        }

    }
}