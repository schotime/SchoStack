using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Baseline.Reflection;
using SchoStack.Web.Conventions.Core;

namespace SchoStack.Web.Html
{
    public class AttribValidatorFinder
    {
        public IEnumerable<ValidationAttribute> FindAttributeValidators(RequestData requestData)
        {
            if (requestData.InputType == null)
                return new List<ValidationAttribute>();

            var properties = InputPropertyMatcher.FindPropertyData(requestData);

            return properties.SelectMany(propertyInfo => propertyInfo.GetAllAttributes<ValidationAttribute>());
        }
    }
}
