using System.Web.Mvc;
using SchoStack.Web.Conventions.Core;

namespace SchoStack.Web
{
    public class FormWebViewPage : WebViewPage
    {
        public override void Execute()
        {
        }
    }

    public class FormWebViewPage<T> : WebViewPage<T>
    {
        public FormWebViewPage()
        {
            Form = new FormBuilder<T>(this);
        }

        public override void Execute()
        {
        }

        public FormBuilder<T> Form { get; set; }
    }
}