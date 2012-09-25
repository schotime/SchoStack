using System;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using HtmlTags;

namespace SchoStack.Web.HtmlTags
{
    public class FormBuilder<T>
    {
        private readonly WebViewPage<T> _webViewPage;

        public FormBuilder(WebViewPage<T> webViewPage)
        {
            _webViewPage = webViewPage;
        }

        //public FormTagBegin ForNested<TInput>(TInput model, Func<Helper<TInput>, HelperResult> template)
        //{
        //    var url = _webViewPage.Url.Url(model);
        //    var tag = new FormTagBegin(_webViewPage.ViewContext, url);
        //    var helperresult = template(new Helper<TInput>(_webViewPage));
        //    tag.Text(new HelperResult(helperresult.WriteTo).ToHtmlString()).Encoded(false);
        //    tag.AppendHtml("</form>");
        //    return tag;
        //}

        public MvcForm For<TInput>() where TInput : new()
        {
            return For(new TInput());
        }

        public MvcForm For<TInput>(Action<FormTag> modifier) where TInput : new()
        {
            return For(new TInput(), modifier);
        }
        
        public MvcForm For<TInput>(TInput model)
        {
            return For(model, begin => { });
        }

        public MvcForm For<TInput>(TInput model, Action<FormTag> modifier)
        {
            var url = _webViewPage.Url.For(model);
            return GenerateForm<TInput>(modifier, url);
        }

        public MvcForm For<TInput>(string controller, string action, object routesValues = null, Action<FormTag> modifier = null)
        {
            modifier = modifier ?? (x => {});
            var url = _webViewPage.Url.Action(action, controller, routesValues);
            return GenerateForm<TInput>(modifier, url);
        }

        private MvcForm GenerateForm<TInput>(Action<FormTag> modifier, string url)
        {
            _webViewPage.Context.Items[TagGenerator.FORMINPUTTYPE] = typeof (TInput);
            var tagGenerator = new TagGenerator(HtmlConventionFactory.HtmlConventions);
            var tag = tagGenerator.GenerateTagFor(_webViewPage.ViewContext, () => new FormTag(url));
            modifier(tag);
            _webViewPage.ViewContext.Writer.WriteLine(tag);
            return new InputTypeMvcForm(_webViewPage.ViewContext);
        }

        public HtmlTag End()
        {
            _webViewPage.ViewContext.HttpContext.Items.Remove(TagGenerator.FORMINPUTTYPE);
            return new LiteralTag("</form>");
        }
    }
}