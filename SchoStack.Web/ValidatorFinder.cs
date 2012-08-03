using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using FluentValidation;
using FluentValidation.Validators;
using FubuCore;
using FubuCore.Reflection;
using SchoStack.Web.HtmlTags;

namespace SchoStack.Web
{
    public interface IValidatorFinder
    {
        IEnumerable<IPropertyValidator> FindValidators(RequestData r);
    }

    public class ValidatorFinder : IValidatorFinder
    {
        private readonly IDependencyResolver _resolver;

        public ValidatorFinder(IDependencyResolver resolver)
        {
            _resolver = resolver;
        }

        private List<PropertyInfo> FindPropertyData(RequestData r)
        {
            var properties = new List<PropertyInfo>();

            if (r.Accessor is SingleProperty)
            {
                properties.Add(r.Accessor.InnerProperty);
            }
            else if (r.Accessor is PropertyChain)
            {
                var chain = r.Accessor as PropertyChain;
                Type type = r.InputType;
                PropertyInfo prop = r.Accessor.InnerProperty;

                foreach (IValueGetter valueGetter in chain.ValueGetters)
                {
                    if (IsGenericList(valueGetter.DeclaringType))
                    {
                        type = prop.PropertyType.GetGenericArguments().First();
                    }
                    else
                    {
                        prop = type.GetProperty(valueGetter.Name);
                        if (prop == null)
                            return new List<PropertyInfo>();
                        type = prop.PropertyType;
                        properties.Add(prop);
                    }
                }
            }

            return properties;
        }

        public IEnumerable<IPropertyValidator> FindValidators(RequestData requestData)
        {
            if (requestData.InputType == null)
                return new List<IPropertyValidator>();
            
            var baseValidator = ResolveValidator(requestData.InputType);
            if (baseValidator == null)
                return new List<IPropertyValidator>();

            var properties = FindPropertyData(requestData);
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
                if (valtype == typeof (ChildCollectionValidatorAdaptor))
                    val = inlineval.As<ChildCollectionValidatorAdaptor>().Validator;
                else if (valtype == typeof (ChildValidatorAdaptor))
                    val = inlineval.As<ChildValidatorAdaptor>().Validator;

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
            var gentype = typeof (IValidator<>).MakeGenericType(modelType);
            var validator = (IValidator) _resolver.GetService(gentype);
            return validator;
        }

        private bool IsGenericList(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            foreach (Type @interface in type.GetInterfaces())
            {
                if (@interface.IsGenericType)
                {
                    if (@interface.GetGenericTypeDefinition() == typeof(ICollection<>))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}