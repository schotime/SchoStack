using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FubuCore.Reflection;
using HtmlTags;

namespace SchoStack.Web.Conventions.Core
{
    public class ConventionActionAttribute<TAttribute> : ConventionAction, IConventionActionAttribute<TAttribute>
    {
        public ConventionActionAttribute(Func<RequestData, bool> condition, IList<Builder> builders, IList<Modifier> modifiers) 
            : base(condition, builders, modifiers)
        {
        }

        public void BuildBy(Func<RequestData, TAttribute, HtmlTag> builder)
        {
            Builders.Add(new Builder(Condition, req =>
            {
                var attribute = (TAttribute)TagConventions.GetPropertyInfo(req.Accessor).GetCustomAttributes(typeof(TAttribute), true).First();
                return builder(req, attribute);
            }));
        }

        public void BuildBy(Func<RequestData, TAttribute, IConventionPipeline, HtmlTag> builder)
        {
            Builders.Add(new Builder(Condition, (req, pipe) =>
            {
                var attribute = (TAttribute)TagConventions.GetPropertyInfo(req.Accessor).GetCustomAttributes(typeof(TAttribute), true).First();
                return builder(req, attribute, pipe);
            }));
        }

        public void Modify(Action<HtmlTag, RequestData, TAttribute> modifier)
        {
            Modifiers.Add(new Modifier(Condition, (tag, req) =>
            {
                var attribute = (TAttribute)TagConventions.GetPropertyInfo(req.Accessor).GetCustomAttributes(typeof(TAttribute), true).First();
                modifier(tag, req, attribute);
            }));
        }
    }
}