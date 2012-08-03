using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using HtmlTags;
using SchoStack.Web.HtmlTags;

namespace SchoStack.Web.Conventions
{
    public class DataAnnotationHtmlConventions : HtmlConvention
    {
        public DataAnnotationHtmlConventions()
        {
            Inputs.IfAttribute<DataTypeAttribute>().BuildBy((r, a) =>
            {
                if (a.DataType == DataType.Text)
                    return new HtmlTag("textarea").Text(r.GetAttemptedValue() ?? r.GetValue<string>());
                
                return null;
            });

            Inputs.IfAttribute<DataTypeAttribute>().Modify((h, r, a) =>
            {
                if (a.DataType == DataType.Password)
                    h.Attr("type", "password").Attr("value", null).Attr("autocomplete", "off");
            });

            Inputs.IfAttribute<HiddenInputAttribute>().Modify((h, r, a) => h.Attr("type", "hidden"));

            Inputs.IfAttribute<StringLengthAttribute>().Modify((h, r, a) => h.Attr("maxlength", a.MaximumLength));
        }
    }
}