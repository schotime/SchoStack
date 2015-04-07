using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FluentValidation;
using FluentValidation.Mvc;
using SchoStack.Web;
using SchoStack.Web.ActionControllers;
using SchoStack.Web.Conventions;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.FluentValidation;
using StructureMap;

namespace SchoStack.Example
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            ObjectFactory.Initialize(x => x.Scan(scan =>
            {
                scan.TheCallingAssembly();
                scan.ConnectImplementationsToTypesClosing(typeof(IValidator<>));
                scan.ConnectImplementationsToTypesClosing(typeof(IHandler<>));
                scan.ConnectImplementationsToTypesClosing(typeof(IHandler<,>));
                scan.ConnectImplementationsToTypesClosing(typeof(ICommandHandler<>));
            }));

            var resolver = new SmDependencyResolver(ObjectFactory.Container);
            DependencyResolver.SetResolver(resolver);

            ModelValidatorProviders.Providers.Add(new FluentValidationModelValidatorProvider(new FluentValidatorFactory(resolver)));
            ActionControllers.Setup(x=>
            {
                x.FindActionsFromCurrentAssembly();
                x.ResolveActionsBy(DependencyResolver.Current.GetService);
            });

            ValidatorOptions.DisplayNameResolver = (type, info, arg3) =>
            {
                return info.Name + "_i18n";
            };

            HtmlConventionFactory.Add(new DefaultHtmlConventions());
            HtmlConventionFactory.Add(new DataAnnotationHtmlConventions());
            HtmlConventionFactory.Add(new FluentValidationHtmlConventions(new FluentValidatorFinder(resolver)));

            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new FeatureCsRazorViewEngine("Controllers"));
           
        }
    }
}