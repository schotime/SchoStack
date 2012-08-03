using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using FubuCore.Reflection;
using HtmlTags;

namespace SchoStack.Web.HtmlTags
{
    public class TagGenerator
    {
        public const string FORMINPUTTYPE = "__formType";
        private readonly List<HtmlConvention> _htmlConventions;

        public TagGenerator(List<HtmlConvention> htmlConventions)
        {
            _htmlConventions = htmlConventions;
        }

        public HtmlTag GenerateTagFor<TModel, TProperty>(ViewContext viewContext,
            Expression<Func<TModel, TProperty>> expression,
            Func<RequestData, HtmlTag> builder,
            Func<HtmlConvention, ITagConventions> getTagConvention)
        {
            var accessor = ReflectionHelper.GetAccessor(expression);
            var req = new RequestData() { 
                ViewContext = viewContext, 
                Accessor = accessor, 
                InputType = viewContext.HttpContext.Items[FORMINPUTTYPE] as Type 
            };
            var tag = builder(req);
            ModifyTag(tag, req, getTagConvention);
            return tag;
        }

        public T GenerateTagFor<T>(ViewContext viewContext, Func<T> builder) where T : HtmlTag
        {
            var req = new RequestData() { 
                ViewContext = viewContext, 
                InputType = viewContext.HttpContext.Items[FORMINPUTTYPE] as Type 
            };
            var tag = builder();
            ModifyTag(tag, req, x => x.All);
            return tag;
        }

        public HtmlTag BuildTag(RequestData requestData, Func<HtmlConvention, ITagConventions> getTagConvention)
        {
            return BuildTag(requestData, getTagConvention, () => new HtmlTag("span").Text(requestData.GetValue<string>()));
        }
        
        public HtmlTag BuildTag(RequestData requestData, Func<HtmlConvention, ITagConventions> getTagConvention, Func<HtmlTag> defaultBuilder)
        {
            for (int i = _htmlConventions.Count - 1; i >= 0; i--)
            {
                var conv = (IConventionAccessor) getTagConvention(_htmlConventions[i]);
                for (int j = conv.Builders.Count - 1; j >= 0; j--)
                {
                    if (conv.Builders[j].Condition(requestData))
                    {
                        var tag = conv.Builders[j].BuilderFunc(requestData);
                        if (tag != null)
                        {
                            return tag;
                        }
                    }
                }
            }

            return defaultBuilder();
        }
        
        private void ModifyTag(HtmlTag tag, RequestData requestData, Func<HtmlConvention, ITagConventions> getTagConvention)
        {
            foreach (var htmlConvention in _htmlConventions)
            {
                foreach (var convention in ((IConventionAccessor) htmlConvention.All).Modifiers.Where(x => x.Condition(requestData)))
                {
                    convention.Modification(tag, requestData);
                }

                var conv = (IConventionAccessor) getTagConvention(htmlConvention);
                if (conv != null && !conv.IsAll)
                {
                    foreach (var convention in conv.Modifiers.Where(x => x.Condition(requestData)))
                    {
                        convention.Modification(tag, requestData);
                    }
                }
            }
        }
    }
}
