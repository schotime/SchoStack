using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace SchoStack.Web.ActionControllers
{
    public class ActionControllerBuilder : IActionControllerBuilder
    {
        public bool CurrentAssembly;
        protected Type FindAssemblyFromType { get; set; }
        public List<Func<Type, bool>> Predicates = new List<Func<Type, bool>>();
        public Func<Type, object> Resolver { get; set; }

        public INamingConventions NamingConventions { get; set; }

        public ActionControllerBuilder()
        {
            NamingConventions = new DefaultNamingConventions();
            Resolver = Activator.CreateInstance;
        }

        public void UsingNamingConventions(INamingConventions namingConventions)
        {
            NamingConventions = namingConventions;
        }

        public void FindActionsFromCurrentAssembly()
        {
            CurrentAssembly = true;
        }

        public void FindActionsFromAssemblyContaining<T>()
        {
            FindAssemblyFromType = typeof (T);
        }

        public void IncludeActionsWhere(Func<Type, bool> predicate)
        {
            Predicates.Add(predicate);
        }

        public void ResolveActionsBy(Func<Type, object> actionResolver)
        {
            Resolver = actionResolver;
        }

        public Dictionary<string, Type> Build()
        {
            var locator = new ControllerActionLocator(NamingConventions);

            if (FindAssemblyFromType != null)
                locator = locator.FindActionsFromAssemblyContaining(FindAssemblyFromType);

            if (CurrentAssembly)
                locator = locator.FindActionsFromAssembly(FindTheCallingAssembly());

            foreach (var predicate in Predicates)
            {
                locator.Where(predicate);
            }

            return locator.Build();
        }

        private static Assembly FindTheCallingAssembly()
        {
            var trace = new StackTrace(false);

            Assembly thisAssembly = Assembly.GetExecutingAssembly();
            Assembly callingAssembly = null;
            for (int i = 0; i < trace.FrameCount; i++)
            {
                StackFrame frame = trace.GetFrame(i);
                Assembly assembly = frame.GetMethod().DeclaringType.Assembly;
                if (assembly != thisAssembly)
                {
                    callingAssembly = assembly;
                    break;
                }
            }
            return callingAssembly;
        }
    }
}