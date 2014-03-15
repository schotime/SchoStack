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

        public virtual Type GetPropertyType()
        {
            return Accessor.PropertyType;
        }

        public Type InputType { get; set; }

        public virtual T GetValue<T>()
        {
            if (ViewContext.ViewData.Model == null)
                return default(T);

            var val = Accessor.GetValue(ViewContext.ViewData.Model);
            if (TagConventions.IsAssignable<T>(this))
                return (T) val;
            
            if (typeof (T) == typeof (string))
            {
                return (T)(val != null ? (object)val.ToString() : (object)string.Empty);
            }

            try
            {
                return (T) Convert.ChangeType(val, typeof (T));
            }
            catch (Exception ex)
            {
                throw new InvalidCastException(string.Format("Cannot convert '{0}' to '{1}'", (val == null ? "null" : val.GetType().ToString()), typeof(T)) ,ex);
            }
        }

        public string GetAttemptedValue()
        {
            return !ViewContext.ViewData.ModelState.IsValid && ViewContext.ViewData.ModelState.ContainsKey(Name)
                       ? ViewContext.ViewData.ModelState[Name].Value.AttemptedValue
                       : null;
        }
    }

    public class ValueRequestData<T> : RequestData
    {
        private readonly object _value;

        public ValueRequestData(RequestData requestData, T value)
        {
            _value = value;
            this.Accessor = requestData.Accessor;
            this.ViewContext = requestData.ViewContext;
            this.InputType = requestData.InputType;
        }

        public override Type GetPropertyType()
        {
            return typeof(T);
        }

        public override TModel GetValue<TModel>()
        {
            return (TModel)_value;
        }
    }
}