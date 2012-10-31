using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using FluentValidation;

namespace SchoStack.Web
{
    public class FluentValidatorFactory : ValidatorFactoryBase
    {
        private readonly IDependencyResolver _dependencyResolver;

        public FluentValidatorFactory(IDependencyResolver dependencyResolver)
        {
            _dependencyResolver = dependencyResolver;
        }

        public override IValidator CreateInstance(Type validatorType)
        {
            return _dependencyResolver.GetService(validatorType) as IValidator;
        }
    }
}
