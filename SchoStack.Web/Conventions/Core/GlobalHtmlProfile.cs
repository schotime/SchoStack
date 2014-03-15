using System.Collections.Generic;

namespace SchoStack.Web.Conventions.Core
{
    public class GlobalHtmlProfile : IHtmlProfile
    {
        public GlobalHtmlProfile(List<HtmlConvention> htmlConventions)
        {
            HtmlConventions = htmlConventions;
        }

        public List<HtmlConvention> HtmlConventions { get; private set; }
    }
}