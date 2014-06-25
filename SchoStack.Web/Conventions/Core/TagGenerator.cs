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
        private readonly IHtmlProfile _globalHtmlProfile;

        public TagGenerator(List<HtmlConvention> htmlConventions)
        {
            _globalHtmlProfile = new GlobalHtmlProfile(htmlConventions);
        }

        public TagGenerator(IHtmlProfile globalHtmlProfile)
        {
            _globalHtmlProfile = globalHtmlProfile;
        }

        private IHtmlProfile GetHtmlProfile(ViewContext viewContext)
        {
            var profileContext = viewContext.HttpContext.Items[HtmlProfileContext.SchostackWebProfile] as HtmlProfileContext;
            if (profileContext == null)
                return _globalHtmlProfile;

            return profileContext.HtmlProfile;
        }

        public HtmlTag GenerateInputFor<TModel, TProperty>(ViewContext viewContext, Expression<Func<TModel, TProperty>> expression)
        {
            var tag = GenerateTag(viewContext, expression, x => x.Inputs);
            return tag;
        }

        public HtmlTag GenerateInputFor(ViewContext viewContext, Accessor accessor)
        {
            var tag = GenerateTag(viewContext, accessor, x => x.Inputs);
            return tag;
        }

        public HtmlTag GenerateDisplayFor<TModel, TProperty>(ViewContext viewContext, Expression<Func<TModel, TProperty>> expression)
        {
            var tag = GenerateTag(viewContext, expression, x => x.Displays);
            return tag;
        }

        public HtmlTag GenerateDisplayFor(ViewContext viewContext, Accessor accessor)
        {
            var tag = GenerateTag(viewContext, accessor, x => x.Displays);
            return tag;
        }

        public HtmlTag GenerateLabelFor<TModel, TProperty>(ViewContext viewContext, Expression<Func<TModel, TProperty>> expression)
        {
            var tag = GenerateTag(viewContext, expression, x => x.Labels);
            return tag;
        }

        public HtmlTag GenerateLabelFor(ViewContext viewContext, Accessor accessor)
        {
            var tag = GenerateTag(viewContext, accessor, x => x.Labels);
            return tag;
        }

        private HtmlTag GenerateTag<TModel, TProperty>(ViewContext viewContext, Expression<Func<TModel, TProperty>> expression, Func<HtmlConvention, ITagConventions> getTagConvention)
        {
            var accessor = ReflectionHelper.GetAccessor(expression);
            return GenerateTag(viewContext, accessor, getTagConvention);
        }

        private HtmlTag GenerateTag(ViewContext viewContext, Accessor accessor, Func<HtmlConvention, ITagConventions> getTagConvention)
        {
            var profile = GetHtmlProfile(viewContext);
            var req = BuildRequestData(viewContext, accessor);
            var tag = BuildTag(req, profile.HtmlConventions, getTagConvention);
            ModifyTag(tag, req, profile.HtmlConventions, getTagConvention);
            return tag;
        }

        public static RequestData BuildRequestData(ViewContext viewContext, Accessor accessor)
        {
            return RequestData.BuildRequestData(viewContext, accessor);
        }

        public static RequestData BuildRequestData<TModel, TProperty>(ViewContext viewContext, Expression<Func<TModel, TProperty>> expression)
        {
            var accessor = ReflectionHelper.GetAccessor(expression);
            return BuildRequestData(viewContext, accessor);
        }

        public T GenerateTagFor<T>(ViewContext viewContext, Func<T> builder) where T : HtmlTag
        {
            var profile = GetHtmlProfile(viewContext);
            var req = BuildRequestData(viewContext, null);
            var tag = builder();
            ModifyTag(tag, req, profile.HtmlConventions, x => x.All);
            return tag;
        }

        private HtmlTag BuildTag(RequestData requestData, List<HtmlConvention> htmlConventions, Func<HtmlConvention, ITagConventions> getTagConvention)
        {
            return BuildTag(requestData, htmlConventions, getTagConvention, () => new HtmlTag("span").Text(requestData.GetValue<string>()));
        }

        private HtmlTag BuildTag(RequestData requestData, List<HtmlConvention> htmlConventions, Func<HtmlConvention, ITagConventions> getTagConvention, Func<HtmlTag> defaultBuilder)
        {
            var builders = new List<Builder>();
            for (int i = htmlConventions.Count - 1; i >= 0; i--)
            {
                var conv = (IConventionAccessor)getTagConvention(htmlConventions[i]);
                for (int j = conv.Builders.Count - 1; j >= 0; j--)
                {
                    builders.Add(conv.Builders[j]);
                }
            }

            var pipeline = new ConventionPipeline(requestData, builders);
            var tag = pipeline.BuildHtmlTag();
            return tag ?? defaultBuilder();
        }

        private void ModifyTag(HtmlTag tag, RequestData requestData, List<HtmlConvention> htmlConventions, Func<HtmlConvention, ITagConventions> getTagConvention)
        {
            foreach (var htmlConvention in htmlConventions)
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
