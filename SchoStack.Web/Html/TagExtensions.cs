using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using FubuCore.Reflection;
using HtmlTags;
using SchoStack.Web.HtmlTags;

namespace SchoStack.Web.Html
{
    public static class TagExtensions
    {
        public static HtmlTag Input<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression)
        {
            var tag = new TagGenerator(HtmlConventionFactory.HtmlConventions);
            Func<HtmlConvention, ITagConventions> conv = x => x.Inputs;
            return tag.GenerateTagFor(helper.ViewContext, expression, (x) => tag.BuildTag(x, conv), conv);
        }

        public static HtmlTag Display<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression)
        {
            var tag = new TagGenerator(HtmlConventionFactory.HtmlConventions);
            Func<HtmlConvention, ITagConventions> conv = x => x.Displays;
            return tag.GenerateTagFor(helper.ViewContext, expression, (x) => tag.BuildTag(x, conv), conv);
        }

        public static HtmlTag Label<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression)
        {
            var tag = new TagGenerator(HtmlConventionFactory.HtmlConventions);
            Func<HtmlConvention, ITagConventions> conv = x => x.Labels;
            return tag.GenerateTagFor(helper.ViewContext, expression, (x) => tag.BuildTag(x, conv), conv);
        }

        public static LiteralTag ValidationSummary(this HtmlHelper htmlHelper)
        {
            var val = ValidationExtensions.ValidationSummary(htmlHelper);
            if (val != null)
                return new LiteralTag(val.ToHtmlString());
            return new LiteralTag(new DivTag().AddClass(HtmlHelper.ValidationSummaryCssClassName).ToHtmlString());
        }

        public static LiteralTag ValidationMessage<TModel, TProperty>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, TProperty>> expression)
        {
            var req = new RequestData() { Accessor = ReflectionHelper.GetAccessor(expression) };
            var val = ValidationExtensions.ValidationMessage(htmlHelper, req.Name);
            if (val != null)
                return new LiteralTag(val.ToHtmlString());
            return new LiteralTag("");
        }

        public static HtmlTag Submit(this HtmlHelper htmlHelper, string text)
        {
            var tag = TagGen().GenerateTagFor(htmlHelper.ViewContext, () => new HtmlTag("input").Attr("type", "submit").Attr("value", text));
            return tag;
        }

        public static LinkTag Link<T>(this HtmlHelper htmlHelper, T model, string text)
        {
            var urlHelper = new UrlHelper(htmlHelper.ViewContext.RequestContext);
            var tag = TagGen().GenerateTagFor(htmlHelper.ViewContext, () => new LinkTag(text, urlHelper.For(model)));
            return tag;
        }

        public static MvcHtmlString Action<T>(this HtmlHelper htmlHelper, T model)
        {
            var factory = ActionFactory.Actions[typeof (T)];
            return ChildActionExtensions.Action(htmlHelper, factory.Action, factory.Controller, UrlExtensions.CreateRouteValueDictionary(model));
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partial)
        {
            return PartialExtensions.Partial(htmlHelper, partial);
        }

        public static MvcHtmlString Partial(this HtmlHelper htmlHelper, string partial, object model)
        {
            return PartialExtensions.Partial(htmlHelper, partial, model);
        }

        public static string Class(this HtmlHelper htmlHelper, bool condition, string className)
        {
            return condition ? className : null;
        }

        public static TagGenerator TagGen()
        {
            return new TagGenerator(HtmlConventionFactory.HtmlConventions);
        }

    }
}