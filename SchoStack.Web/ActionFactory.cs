using System;
using System.Collections.Generic;

namespace SchoStack.Web
{
    public class ActionDictionary : Dictionary<Type, ActionInfo> {}

    public class ActionFactory
    {
        public static ActionDictionary Actions = new ActionDictionary();
    }
}
