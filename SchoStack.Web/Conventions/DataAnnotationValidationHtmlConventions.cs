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
                AddRequiredClass(propertyValidators, h, r.ViewContext.UnobtrusiveJavaScriptEnabled);
                AddRegexData(propertyValidators, h);
                AddEqualToDataAttr(propertyValidators, h, r);
            });
        }

        private static void AddRegexData(IEnumerable<ValidationAttribute> propertyValidators, HtmlTag htmlTag)
        {
            var regex = propertyValidators.OfType<RegularExpressionAttribute>().FirstOrDefault();
            if (regex != null)
            {
                htmlTag.Data("val", true).Data("val-regex", regex.ErrorMessage ?? string.Format("The value did not match the regular expression '{0}'", regex.Pattern)).Data("val-regex-pattern", regex.Pattern);
            }
        }

        private static void AddEqualToDataAttr(IEnumerable<ValidationAttribute> propertyValidators, HtmlTag htmlTag, RequestData request)
        {
            var equal = propertyValidators.OfType<CompareAttribute>().FirstOrDefault();
            if (equal != null)
            {
                htmlTag.Data("val", true);
                if (request.Accessor.PropertyNames.Length > 1)
                {
                    htmlTag.Data("val-equalTo", request.Id.Replace("_" + request.Accessor.Name, "") + "_" + equal.OtherProperty);
                }
                else
                {
                    htmlTag.Data("val-equalTo", equal.OtherProperty);
                }
            }
        }

        private static void AddRequiredClass(IEnumerable<ValidationAttribute> propertyValidators, HtmlTag htmlTag, bool unobtrusiveJavaScriptEnabled)
        {
            var notEmpty = propertyValidators.OfType<RequiredAttribute>().FirstOrDefault();
            if (notEmpty != null)
            {
                if (unobtrusiveJavaScriptEnabled) 
                    htmlTag.Data("val", true).Data("val-required", notEmpty.ErrorMessage ?? "The value is required");
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