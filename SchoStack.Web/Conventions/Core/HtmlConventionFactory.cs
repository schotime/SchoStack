using System.Collections.Generic;
using System.Web.Mvc;

namespace SchoStack.Web.Conventions.Core
{
    public class HtmlConventionFactory
    {
        public static List<HtmlConvention> HtmlConventions = new List<HtmlConvention>();

        public static void Add(HtmlConvention htmlConventions)
        {
            HtmlConventions.Add(htmlConventions);
        }

        public static void Add<T>() where T : HtmlConvention
        {
            HtmlConventions.Add(DependencyResolver.Current.GetService<T>());
        }
    }
}