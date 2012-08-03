using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace SchoStack.Web.ActionControllers
{
    public class ControllerActionLocator
    {
        private INamingConventions namingConventions;
        private List<Type> types = new List<Type>();
        private List<Func<Type, bool>> predicates = new List<Func<Type, bool>>();
        private List<Assembly> assemblies = new List<Assembly>();

        public INamingConventions NamingConventions
        {
            get { return namingConventions; }
        }

        public ControllerActionLocator(INamingConventions namingConventions)
        {
            this.namingConventions = namingConventions;
        }

        public ControllerActionLocator FindActionsFromAssemblyContaining(Type type)
        {
            if (!assemblies.Contains(type.Assembly))
            {
                FindActionsFromAssembly(type.Assembly);
                assemblies.Add(type.Assembly);
            }

            return this;
        }

        public ControllerActionLocator FindActionsFromAssembly(Assembly assembly)
        {
            if (!assemblies.Contains(assembly))
            {
                FindActionsFrom(assembly.GetExportedTypes());
                assemblies.Add(assembly);
            }

            return this;
        }

        public ControllerActionLocator FindActionsFrom(IEnumerable<Type> types)
        {
            this.types.AddRange(types);
            return this;
        }

        public ControllerActionLocator Where(Func<Type, bool> predicate)
        {
            predicates.Add(predicate);
            return this;
        }

        public Dictionary<string, Type> Build()
        {
            var actionTypes = (from type in types
                               where type.IsPublic
                               where type != typeof (ActionController)
                               where typeof (ActionController).IsAssignableFrom(type)
                               where !type.IsAbstract
                               where !type.IsInterface
                               select type);

            var located = actionTypes
                .Where(x => x.GetCustomAttributes(typeof(RouteAttribute), false).Any())
                .Where(x => predicates.All(func => func(x)))
                .Select(x =>
                {
                    var type = x;
                    var key = namingConventions.BuildKeyFromType(x);
                    var controller = namingConventions.BuildControllerFromType(x);
                    var action = namingConventions.BuildActionFromType(x);
                    var routeAttr = x.GetCustomAttributes(typeof(RouteAttribute), false).OfType<RouteAttribute>().First();

                    // Set Defautls
                    var defaults = ParseRouteValues(routeAttr.Defaults);
                    defaults.Add("controller", controller);
                    defaults.Add("action", action);

                    // Set Optional Parameters and remove '?' mark from the url
                    Match m;
                    while ((m = Regex.Match(routeAttr.Url, @"\{([^\}]+?)\?\}")) != null && m.Success)
                    {
                        var p = m.Groups[1].Value;
                        defaults.Add(p, UrlParameter.Optional);
                        routeAttr.Url = routeAttr.Url.Replace("{" + p + "?}", "{" + p + "}");
                    }

                    // Set Defautls
                    var constraints = ParseRouteValues(routeAttr.Constraints);

                    // Set Data Tokens
                    var dataTokens = new RouteValueDictionary();
                    dataTokens.Add("Namespaces", new[] {type.Namespace});

                    //Setup Actions
                    var methods = type.GetMethods().Where(y => y.Name == "Get" || y.Name == "Post" || y.Name == "Execute");
                    foreach (var meth in methods)
                    {
                        var par = meth.GetParameters().FirstOrDefault();
                        if (par == null)
                            throw new Exception("No Parameter found for action - " + meth.DeclaringType + ", " + meth.Name);

                        ActionFactory.Actions.Add(par.ParameterType, new ActionInfo()
                        {
                            ClassType = meth.DeclaringType,
                            HttpMethod = meth.Name,
                            Name = meth.Name,
                            Controller = controller,
                            Action = action
                        });

                        var route = new Route(routeAttr.Url.TrimStart('/'), new MvcRouteHandler())
                        {
                            Defaults = defaults,
                            Constraints = constraints,
                            DataTokens = dataTokens
                        };

                        RouteTable.Routes.Add(par.ParameterType.FullName, route);
                    }

                    return new { key, type };

                }).ToDictionary(x => x.key, x => x.type);
            
            return located;
        }

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