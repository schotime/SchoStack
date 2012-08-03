using System.Collections.Generic;

namespace SchoStack.Web.HtmlTags
{
    public interface IConventionAccessor
    {
        IList<Modifier> Modifiers { get; }
        IList<Builder> Builders { get; }
        bool IsAll { get; }
    }
}