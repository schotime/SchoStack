using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;
using FubuCore.Reflection;

namespace SchoStack.Web
{
    public static class RouteCollectionExtensions
    {
        public static RouteCollection MapRoutes(this RouteCollection routes)
        {
            return MapRoutes(routes, Assembly.GetCallingAssembly(), ActionFactory.Actions);
        }

        public static RouteCollection MapRoutes(this RouteCollection routes, Assembly assembly, ActionDictionary actions)
        {
            if (routes == null)
            {
                throw new ArgumentNullException("routes");
            }

            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            assembly.GetTypes()
                // Find all non abstract classes of type Controller and whose names end with "Controller"
                .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(Controller)) && x.Name.EndsWith("Controller"))

                // Find all public methods from those controller classes
                .SelectMany(x => x.GetMethods(), (x, y) => new { Controller = x.Name, Method = y, Namespace = x.Namespace })

                // Find all route attributes from those methods
                .SelectMany(x => x.Method.GetCustomAttributes(typeof(RouteAttribute), false),
                            (x, y) => new { Controller = x.Controller.Substring(0, x.Controller.Length - 10), Action = x.Method.Name, Method = x.Method, Namespace = x.Namespace, Route = (RouteAttribute)y })

                // Order selected entires by rank number and iterate through each of them
                .OrderBy(x => x.Route.Order == -1).ThenBy(x => x.Route.Order).ToList().ForEach(x =>
                {
                    // Set Defautls
                    var defaults = ParseRouteValues(x.Route.Defaults);
                    defaults.Add("controller", x.Controller);
                    defaults.Add("action", x.Action);

                    // Set Optional Parameters and remove '?' mark from the url
                    Match m;

                    while ((m = Regex.Match(x.Route.Url, @"\{([^\}]+?)\?\}")) != null && m.Success)
                    {
                        var p = m.Groups[1].Value;
                        defaults.Add(p, UrlParameter.Optional);
                        x.Route.Url = x.Route.Url.Replace("{" + p + "?}", "{" + p + "}");
                    }

                    // Set Defautls
                    var constraints = ParseRouteValues(x.Route.Constraints);

                    // Set Data Tokens
                    var dataTokens = new RouteValueDictionary();
                    dataTokens.Add("Namespaces", new string[] { x.Namespace });

                    var route = new Route(x.Route.Url, new MvcRouteHandler())
                    {
                        Defaults = defaults,
                        Constraints = constraints,
                        DataTokens = dataTokens
                    };

                    //Setup Actions
                    var par = x.Method.GetParameters().FirstOrDefault();
                    if (par == null)
                        throw new Exception("No Parameter found for action - " + x.Method.DeclaringType + ", " + x.Method.Name);

                    if (actions.ContainsKey(par.ParameterType))
                    {
                        throw new Exception("Parameter type already exists on another action - " + x.Method.DeclaringType + ", " + x.Method.Name + ", " + par.ParameterType);
                    }

                    actions.Add(par.ParameterType, new ActionInfo()
                    {
                        ClassType = x.Method.DeclaringType,
                        HttpMethod = x.Method.GetAttribute<HttpPostAttribute>() != null ? "post" : "get",
                        Name = x.Method.Name,
                        Controller = x.Controller,
                        Action = x.Action
                    });

                    x.Route.Name = par.ParameterType.FullName;
                    routes.Add(x.Route.Name, route);
                });

            return routes;
        }

        /// <summary>
        /// Parse route values string
        /// </summary>
        /// <param name="values">Route values string. Ex.: id=[0-9]+;name=[a-z]+;category=books</param>
        /// <returns><see cref="System.Web.Routing.RouteValueDictionary"/></returns>
        internal static RouteValueDictionary ParseRouteValues(string values)
        {
            var routeValues = new RouteValueDictionary();

            if (String.IsNullOrEmpty(values))
            {
                return routeValues;
            }

            foreach (var value in values.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var args = value.Split('=');
                if (args.Length == 2)
                {
                    routeValues.Add(args[0], args[1]);
                }
            }

            return routeValues;
        }
    }
}
