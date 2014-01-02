using System;
using HtmlTags;
using SchoStack.Web.Conventions.Core;

namespace SchoStack.Tests.HtmlConventions
{
    public class CombinationConventions : HtmlConvention
    {
        public CombinationConventions()
        {
            Inputs.If<CombinationType>().BuildBy((r,p) =>
            {
                var val = r.GetValue<CombinationType>();
                if (val.IsSingle)
                {
                    var div = new DivTag().Text(val.Name);
                    var hidden = new HiddenTag().Id(r.Id).Attr("name", r.Name).Attr("value", val.Value);
                    hidden.WrapWith(div);
                    return hidden.RenderFromTop();
                }
                return p.Build<CombinationType>(x=>x.Items);
            });

            All.If<DateTime>().Modify((h, r) =>
            {
                var val = r.GetValue<DateTime>();
                h.Text(val.ToString("yyyyMMdd"));
            });
        }
    }
}