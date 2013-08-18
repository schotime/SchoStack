using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Web.Mvc;
using FubuCore.Reflection;
using HtmlTags;

namespace SchoStack.Web.Conventions.Core
{
    public class TagGenerator
    {
        public const string FORMINPUTTYPE = "__formType";
        private readonly List<HtmlConvention> _htmlConventions;

        public TagGenerator(List<HtmlConvention> htmlConventions)
        {
            _htmlConventions = htmlConventions;
        }

        public HtmlTag GenerateInputFor<TModel, TProperty>(ViewContext viewContext, Expression<Func<TModel, TProperty>> expression)
        {
            var tag = GenerateTag(viewContext, expression, x => x.Inputs);
            return tag;
        }

        public HtmlTag GenerateDisplayFor<TModel, TProperty>(ViewContext viewContext, Expression<Func<TModel, TProperty>> expression)
        {
            var tag = GenerateTag(viewContext, expression, x => x.Displays);
            return tag;
        }

        public HtmlTag GenerateLabelFor<TModel, TProperty>(ViewContext viewContext, Expression<Func<TModel, TProperty>> expression)
        {
            var tag = GenerateTag(viewContext, expression, x => x.Labels);
            return tag;
        }

        private HtmlTag GenerateTag<TModel, TProperty>(ViewContext viewContext, 
            Expression<Func<TModel, TProperty>> expression,
            Func<HtmlConvention, ITagConventions> getTagConvention)
        {
            var req = BuildRequestData(viewContext, expression);
            var tag = BuildTag(req, getTagConvention);
            ModifyTag(tag, req, getTagConvention);
            return tag;
        }

        public static RequestData BuildRequestData<TModel, TProperty>(ViewContext viewContext, Expression<Func<TModel, TProperty>> expression)
        {
            var accessor = ReflectionHelper.GetAccessor(expression);
            var req = new RequestData()
                      {
                          ViewContext = viewContext,
                          Accessor = accessor,
                          InputType = viewContext.HttpContext.Items[FORMINPUTTYPE] as Type
                      };
            return req;
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

        private HtmlTag BuildTag(RequestData requestData, Func<HtmlConvention, ITagConventions> getTagConvention)
        {
            return BuildTag(requestData, getTagConvention, () => new HtmlTag("span").Text(requestData.GetValue<string>()));
        }
        
        private HtmlTag BuildTag(RequestData requestData, Func<HtmlConvention, ITagConventions> getTagConvention, Func<HtmlTag> defaultBuilder)
        {
            var builders = new List<Builder>();
            for (int i = _htmlConventions.Count - 1; i >= 0; i--)
            {
                var conv = (IConventionAccessor) getTagConvention(_htmlConventions[i]);
                for (int j = conv.Builders.Count - 1; j >= 0; j--)
                {
                    builders.Add(conv.Builders[j]);
                }
            }

            var pipeline = new ConventionPipeline(requestData, builders);
            var tag = pipeline.BuildHtmlTag();
            return tag ?? defaultBuilder();
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
