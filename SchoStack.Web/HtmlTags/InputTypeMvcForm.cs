using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace SchoStack.Web.HtmlTags
{
    public class InputTypeMvcForm : MvcForm
    {
        private readonly ViewContext _viewContext;

        public InputTypeMvcForm(ViewContext viewContext) : base(viewContext)
        {
            _viewContext = viewContext;
        }

        protected override void Dispose(bool disposing)
        {
            _viewContext.HttpContext.Items.Remove(TagGenerator.FORMINPUTTYPE);
            base.Dispose(disposing);
        }
    }
}