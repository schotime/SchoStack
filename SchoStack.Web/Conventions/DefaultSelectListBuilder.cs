using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Baseline;
using HtmlTags;
using SchoStack.Web.Conventions.Core;

namespace SchoStack.Web.Conventions
{
    public interface IHtmlBuilder
    {
        HtmlTag Build(RequestData req);
    }

    public class DefaultSelectListBuilder<T> : IHtmlBuilder where T : SelectListItem
    {
        public HtmlTag Build(RequestData req)
        {
            var list = req.GetValue<IEnumerable<T>>();
            if (list == null)
                return null;

            var attemptedVal = req.GetAttemptedValue();
            var multiple = attemptedVal != null ? attemptedVal.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()) : null;
            var dropDown = BuildSelectTag();
            if (list is MultiSelectList)
            {
                dropDown.Attr("multiple", "multiple");
                list = list.As<MultiSelectList>().Items.As<IEnumerable<T>>();
            }
            foreach (var item in list)
            {
                bool selected = attemptedVal != null ? multiple.Contains(item.Value ?? item.Text) : item.Selected;
                var option = BuildOptionTag(item, selected);
                dropDown.Children.Add(option);
            }
            return dropDown;
        }

        protected virtual SelectTag BuildSelectTag()
        {
            return new SelectTag();
        }

        protected virtual HtmlTag BuildOptionTag(T item, bool selected)
        {
            var option = new HtmlTag("option").Attr("value", item.Value).Attr("selected", selected ? "selected" : null).Text(item.Text);
            return option;
        }
    }
}