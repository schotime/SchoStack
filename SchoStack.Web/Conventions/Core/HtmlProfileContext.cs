using System;
using System.Web;
using System.Web.Mvc;

namespace SchoStack.Web.Conventions.Core
{
    public class HtmlProfileContext : IDisposable, IHtmlString
    {
        public const string SchostackWebProfile = "__schostack.web.profile";
        private readonly HtmlHelper _htmlHelper;

        public HtmlProfileContext(HtmlHelper htmlHelper, IHtmlProfile htmlProfile, HtmlProfileContext parentHtmlProfileContext)
        {
            HtmlProfile = htmlProfile;
            ParentHtmlProfileContext = parentHtmlProfileContext;
            _htmlHelper = htmlHelper;
        }

        public IHtmlProfile HtmlProfile { get; private set; }
        public HtmlProfileContext ParentHtmlProfileContext { get; set; }

        public void Dispose()
        {
            if (ParentHtmlProfileContext == null)
                _htmlHelper.ViewContext.HttpContext.Items.Remove(SchostackWebProfile);
            else
                _htmlHelper.ViewContext.HttpContext.Items[SchostackWebProfile] = ParentHtmlProfileContext;
        }

        public string ToHtmlString()
        {
            return null;
        }
    }
}