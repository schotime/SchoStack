using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Validators;
using SchoStack.Web.Conventions.Core;

namespace SchoStack.Web.FluentValidation
{
    public interface IValidatorFinder
    {
        IEnumerable<IPropertyValidator> FindValidators(RequestData r);
    }

    public class FluentValidatorFinder : IValidatorFinder
    {
        private readonly IDependencyResolver _resolver;

        public FluentValidatorFinder() : this(DependencyResolver.Current) { }

        public FluentValidatorFinder(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        public IEnumerable<IPropertyValidator> FindValidators(RequestData requestData)
        {
            if (requestData.InputType == null)
                return new List<IPropertyValidator>();

            var baseValidator = ResolveValidator(requestData.InputType);
            if (baseValidator == null)
                return new List<IPropertyValidator>();

            var properties = InputPropertyMatcher.FindPropertyData(requestData);
            var validators = GetPropertyValidators(baseValidator, properties);
            return validators;
        }

        private IEnumerable<IPropertyValidator> GetPropertyValidators(IValidator baseValidator, List<PropertyInfo> properties)
        {
            var desc = baseValidator.CreateDescriptor();
            var validators = GetNestedPropertyValidators(desc, properties, 0).ToList();
            return validators;
        }

        private IEnumerable<IPropertyValidator> GetNestedPropertyValidators(IValidatorDescriptor desc, List<PropertyInfo> propertyInfo, int i)
        {
            if (i == propertyInfo.Count)
                return new List<IPropertyValidator>();

            var vals = desc.GetValidatorsForMember(propertyInfo[i].Name);
            var propertyValidators = new List<IPropertyValidator>();

            foreach (var inlineval in vals)
            {
                var valtype = inlineval.GetType();
                IValidator val = null;
                if (valtype == typeof(ChildCollectionValidatorAdaptor))
                    val = ((ChildCollectionValidatorAdaptor)inlineval).Validator;
                else if (valtype == typeof(ChildValidatorAdaptor))
                    val = ((ChildValidatorAdaptor)inlineval).Validator;

                if (i == propertyInfo.Count - 1)
                    propertyValidators.Add(inlineval);

                if (val == null)
                    continue;

                var morevals = GetNestedPropertyValidators(val.CreateDescriptor(), propertyInfo, i + 1);
                propertyValidators.AddRange(morevals);
            }

            return propertyValidators;
        }

        private IValidator ResolveValidator(Type modelType)
        {
            var gentype = typeof(IValidator<>).MakeGenericType(modelType);
            var validator = (IValidator)_resolver.GetService(gentype);
            return validator;
        }
    }
}
