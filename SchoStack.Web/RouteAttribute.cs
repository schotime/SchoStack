using System;
using System.Diagnostics.CodeAnalysis;

namespace SchoStack.Web
{

    /// <summary>
    /// Represents a URL route
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class RouteAttribute : Attribute
    {
        private string name;
        private string url;
        private int order = -1;

        /// <summary>
        /// Name of the route
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    throw new ArgumentException("value", "Name must not be null");
                }

                name = value;
            }
        }

        /// <summary>
        /// Route URL
        /// </summary>
        public string Url
        {
            get
            {
                return url;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", "Url must not be null");
                }

                url = value;
            }
        }

        /// <summary>
        /// A list of URL parameter defaults delimited by semicolon. Ex.: category=general;order=name
        /// </summary>
        public string Defaults { get; set; }

        /// <summary>
        /// A list of URL parameter constraints delimited by semicolon. Ex.: category=[a-z]+;id=[0-9]+
        /// </summary>
        public string Constraints { get; set; }

        /// <summary>
        /// Order number of the route. Default: -1
        /// </summary>
        public int Order
        {
            get
            {
                return order;
            }
            set
            {
                if (value < -1)
                {
                    throw new ArgumentOutOfRangeException("value", "Order must be greater than -1");
                }

                order = value;
            }
        }

        /// <param name="url">Route URL</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#",
            Justification = "This is not a regular URL as it may contain special routing characters.")]
        public RouteAttribute(string url)
        {
            Url = url;
        }

        /// <param name="url">Route URL</param>
        /// <param name="order">Order number. Default: -1</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#",
           Justification = "This is not a regular URL as it may contain special routing characters.")]
        public RouteAttribute(string url, int order)
        {
            Url = url;
            Order = order;
        }

        /// <param name="url">Route URL</param>
        /// <param name="defaults">A list of URL parameter defaults delimited by semicolon. Ex.: category=general;order=name</param>
        /// <param name="constraints">A list of URL parameter constraints delimited by semicolon. Ex.: category=[a-z]+;id=[0-9]+</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#",
            Justification = "This is not a regular URL as it may contain special routing characters.")]
        public RouteAttribute(string url, string defaults, string constraints)
        {
            Url = url;
            Defaults = defaults;
            Constraints = constraints;
        }

        /// <param name="url">Route URL</param>
        /// <param name="defaults">A list of URL parameter defaults delimited by semicolon. Ex.: category=general;order=name</param>
        /// <param name="constraints">A list of URL parameter constraints delimited by semicolon. Ex.: category=[a-z]+;id=[0-9]+</param>
        /// <param name="rank">Order number. Default: -1</param>
        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "0#",
            Justification = "This is not a regular URL as it may contain special routing characters.")]
        public RouteAttribute(string url, string defaults, string constraints, int order)
        {
            Url = url;
            Defaults = defaults;
            Constraints = constraints;
            Order = order;
        }
    }
}