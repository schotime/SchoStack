using System.Globalization;
using System.Web.Mvc;

namespace SchoStack.Tests.HtmlConventions
{
    public static class ViewContextTestExtensions
    {
        public static void SetError(this ViewContext viewContext, string name, string errorValue)
        {
            var modelerror = new ModelState()
                             {
                                 Value = new ValueProviderResult(errorValue, errorValue, new CultureInfo("en-AU"))
                             };
            modelerror.Errors.Add("Error");
            viewContext.ViewData.ModelState.Add(name, modelerror);
        }
    }
}