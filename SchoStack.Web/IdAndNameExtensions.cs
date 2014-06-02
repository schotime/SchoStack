using System;
using System.Linq.Expressions;
using System.Web.Mvc;
using SchoStack.Web.Conventions.Core;

namespace SchoStack.Web.Html
{
    public static class IdAndNameExtensions
    {
        public static string IdFor<TModel, T>(this LoopItem<TModel, T> loopItem, Expression<Func<T, object>> expression)
        {
            return TagGenerator.BuildRequestData(loopItem.Info.HtmlHelper.ViewContext, loopItem.Info.TranslateExpression(expression)).Id;
        }

        public static string NameFor<TModel, T>(this LoopItem<TModel, T> loopItem, Expression<Func<T, object>> expression)
        {
            return TagGenerator.BuildRequestData(loopItem.Info.HtmlHelper.ViewContext, loopItem.Info.TranslateExpression(expression)).Name;
        }

        public static string IdFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, object>> expression)
        {
            return TagGenerator.BuildRequestData(htmlHelper.ViewContext, expression).Id;
        }

        public static string NameFor<TModel>(this HtmlHelper<TModel> htmlHelper, Expression<Func<TModel, object>> expression)
        {
            return TagGenerator.BuildRequestData(htmlHelper.ViewContext, expression).Name;
        }
    }
}