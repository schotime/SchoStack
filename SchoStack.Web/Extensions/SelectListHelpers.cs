using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace SchoStack.Web.Extensions
{
    public static class SelectListHelpers
    {
        public static IEnumerable<SelectListItem> ToSelectItemList<T>(this IEnumerable<T> list, Func<T, object> text, Func<T, object> value, object selected)
        {
            var selectList = new List<SelectListItem>();
            AddtoList(list, text, value, selected, selectList);
            return selectList;
        }

        public static MultiSelectList ToMultiSelectList<T>(this IEnumerable<T> list, Func<T, object> text, Func<T, object> value, IEnumerable selectedItems)
        {
            var selectList = new List<SelectListItem>();
            AddtoList(list, text, value, selectedItems, selectList);
            return new MultiSelectList(selectList, selectedItems);
        }

        private static void AddtoList<T>(IEnumerable<T> list, Func<T, object> text, Func<T, object> value, object selected, List<SelectListItem> selectList)
        {
            var selectedItems = (selected is string) ? new[] { selected } : (selected as IEnumerable) ?? new[] { selected };

            selectList.AddRange(list.Select(x =>
            {
                var selects = selectedItems.Cast<object>().Select(y => Convert.ToString(y));
                var item = new SelectListItem
                {
                    Text = Convert.ToString(text(x)),
                    Value = Convert.ToString(value(x)),
                    Selected = selects.Contains(Convert.ToString(value(x)))
                };
                return item;
            }));
        }
    }
}
