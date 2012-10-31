using System;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using FubuCore.Reflection;

namespace SchoStack.Web.Conventions.Core
{
    public class RequestData
    {
        private static readonly Regex IdRegex = new Regex(@"[\.\[\]]", RegexOptions.Compiled);

        public ViewContext ViewContext { get; set; }
        public Accessor Accessor { get; set; }

        public string Id
        {
            get { return Name == null ? null : IdRegex.Replace(Name, "_"); }
        }

        public string Name 
        {
            get { return Accessor == null ? null : string.Join(".", Accessor.PropertyNames).Replace(".[", "["); }
        }

        public Type InputType { get; set; }

        public T GetValue<T>()
        {
            var val = Accessor.GetValue(ViewContext.ViewData.Model);
            if (TagConventions.IsAssignable<T>(this))
                return (T) val;

            return (T) Convert.ChangeType(val, typeof (T));
        }

        public string GetAttemptedValue()
        {
            return !ViewContext.ViewData.ModelState.IsValid && ViewContext.ViewData.ModelState.ContainsKey(Name)
                       ? ViewContext.ViewData.ModelState[Name].Value.AttemptedValue
                       : null;
        }
    }
}