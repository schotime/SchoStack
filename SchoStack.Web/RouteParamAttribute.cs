using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SchoStack.Web
{
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class RouteParamAttribute : Attribute
    {
    }
}
