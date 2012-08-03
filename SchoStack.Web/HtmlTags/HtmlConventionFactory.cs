using System.Collections.Generic;
using StructureMap;

namespace SchoStack.Web.HtmlTags
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
            HtmlConventions.Add(ObjectFactory.GetInstance<T>());
        }
    }
}