using System;
using HtmlTags;

namespace SchoStack.Web.Conventions.Core
{
    public class Modifier
    {
        public Modifier(Func<RequestData, bool> condition, Action<HtmlTag, RequestData> modification)
        {
            Condition = condition;
            Modification = modification;
        }

        public Func<RequestData, bool> Condition { get; private set; }
        public Action<HtmlTag, RequestData> Modification { get; private set; }
    }
}