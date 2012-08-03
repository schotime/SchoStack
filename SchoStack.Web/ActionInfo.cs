using System;

namespace SchoStack.Web
{
    public class ActionInfo
    {
        public string Name { get; set; }
        public Type ClassType { get; set; }
        public string HttpMethod { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
    }
}
