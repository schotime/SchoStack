using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using HtmlTags;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.Html;

namespace SchoStack.Web.Conventions
{
    public class DataAnnotationValidationHtmlConventions : HtmlConvention
    {
        public DataAnnotationValidationHtmlConventions()
        {
            Inputs.Always.Modify((h, r) =>
            {
                var propertyValidators = new AttribValidatorFinder().FindAttributeValidators(r);
                AddLengthClasses(propertyValidators, h);
                AddRequiredClass(propertyValidators, h, r);
                AddRegexData(propertyValidators, h, r);
                AddEqualToDataAttr(propertyValidators, h, r);
            });
        }

        private static void AddRegexData(IEnumerable<ValidationAttribute> propertyValidators, HtmlTag htmlTag, RequestData request)
        {
            var regex = propertyValidators.OfType<RegularExpressionAttribute>().FirstOrDefault();
            if (regex != null)
            {
                htmlTag.Data("val", true).Data("val-regex", regex.ErrorMessage ?? string.Format("The field '{0}' did not match the regular expression '{1}'", request.Accessor.InnerProperty.Name, regex.Pattern)).Data("val-regex-pattern", regex.Pattern);
            }
        }

        private static void AddEqualToDataAttr(IEnumerable<ValidationAttribute> propertyValidators, HtmlTag htmlTag, RequestData request)
        {
            var equal = propertyValidators.OfType<CompareAttribute>().FirstOrDefault();
            if (equal != null)
            {
                htmlTag.Data("val", true);
                htmlTag.Data("val-equalto", equal.FormatErrorMessage(request.Accessor.Name));
                if (request.Accessor.PropertyNames.Length > 1)
                {
                    htmlTag.Data("val-equalto-other", request.Id.Replace("_" + request.Accessor.Name, "") + "_" + equal.OtherProperty);
                }
                else
                {
                    htmlTag.Data("val-equalto-other", "*." + equal.OtherProperty);
                }
            }
        }

        private static void AddRequiredClass(IEnumerable<ValidationAttribute> propertyValidators, HtmlTag htmlTag, RequestData request)
        {
            var notEmpty = propertyValidators.OfType<RequiredAttribute>().FirstOrDefault();
            if (notEmpty != null)
            {
                if (request.ViewContext.UnobtrusiveJavaScriptEnabled)
                    htmlTag.Data("val", true).Data("val-required", notEmpty.ErrorMessage ?? string.Format("The field '{0}' is required", request.Accessor.InnerProperty.Name));
                else 
                    htmlTag.AddClass("required");
            }
        }

        private static void AddLengthClasses(IEnumerable<ValidationAttribute> propertyValidators, HtmlTag htmlTag)
        {
            var lengthValidator = propertyValidators.OfType<StringLengthAttribute>().FirstOrDefault();
            if (lengthValidator != null)
            {
                htmlTag.Attr("maxlength", lengthValidator.MaximumLength);
                if (lengthValidator.MinimumLength > 0)
                    htmlTag.Attr("minlength", lengthValidator.MinimumLength);
            }
        }
    }
}