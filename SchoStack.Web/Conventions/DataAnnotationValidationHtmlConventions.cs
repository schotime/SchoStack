using System;
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
        private readonly bool _msUnobtrusive;
        public List<Action<IEnumerable<ValidationAttribute>, HtmlTag, RequestData>> RuleProviders = new List<Action<IEnumerable<ValidationAttribute>, HtmlTag, RequestData>>();

        public DataAnnotationValidationHtmlConventions() : this(true)
        {
            
        }

        public DataAnnotationValidationHtmlConventions(bool msUnobtrusive)
        {
            _msUnobtrusive = msUnobtrusive;
            RuleProviders.Add(AddLengthClasses);
            RuleProviders.Add(AddRequiredClass);
            RuleProviders.Add(AddRegexData);
            RuleProviders.Add(AddEqualToDataAttr);

            Inputs.Always.Modify((h, r) =>
            {
                var propertyValidators = new AttribValidatorFinder().FindAttributeValidators(r);
                foreach (var ruleProvider in RuleProviders)
                {
                    ruleProvider.Invoke(propertyValidators, h, r);
                }
            });
        }

        public void AddRegexData(IEnumerable<ValidationAttribute> propertyValidators, HtmlTag htmlTag, RequestData request)
        {
            var regex = propertyValidators.OfType<RegularExpressionAttribute>().FirstOrDefault();
            if (regex != null)
            {
                var msg = regex.ErrorMessage ?? string.Format("The field '{0}' did not match the regular expression '{1}'", request.Accessor.InnerProperty.Name, regex.Pattern);
                if (_msUnobtrusive)
                    htmlTag.Data("val", true).Data("val-regex", msg).Data("val-regex-pattern", regex.Pattern);
                else
                    htmlTag.Data("rule-regex", regex.Pattern).Data("msg-regex", msg);
            }
        }

        public void AddEqualToDataAttr(IEnumerable<ValidationAttribute> propertyValidators, HtmlTag htmlTag, RequestData request)
        {
            var equal = propertyValidators.OfType<CompareAttribute>().FirstOrDefault();
            if (equal != null)
            {
                var formatErrorMessage = equal.FormatErrorMessage(request.Accessor.Name);
                if (_msUnobtrusive)
                {
                    htmlTag.Data("val", true);
                    htmlTag.Data("val-equalto", formatErrorMessage);
                    if (request.Accessor.PropertyNames.Length > 1)
                    {
                        htmlTag.Data("val-equalto-other", request.Id.Replace("_" + request.Accessor.Name, "") + "_" + equal.OtherProperty);
                    }
                    else
                    {
                        htmlTag.Data("val-equalto-other", "*." + equal.OtherProperty);
                    }
                }
                else
                {
                    htmlTag.Data("msg-equalto", formatErrorMessage);
                    if (request.Accessor.PropertyNames.Length > 1)
                        htmlTag.Data("rule-equalto", "#" + request.Id.Replace("_" + request.Accessor.Name, "") + "_" + equal.OtherProperty);
                    else
                        htmlTag.Data("rule-equalto", "#" + equal.OtherProperty);
                }
            }
        }

        public void AddRequiredClass(IEnumerable<ValidationAttribute> propertyValidators, HtmlTag htmlTag, RequestData request)
        {
            var required = propertyValidators.OfType<RequiredAttribute>().FirstOrDefault();
            if (required != null)
            {
                if (request.ViewContext.UnobtrusiveJavaScriptEnabled)
                {
                    var msg = required.ErrorMessage ?? string.Format("The field '{0}' is required", request.Accessor.InnerProperty.Name);
                    if (_msUnobtrusive)
                        htmlTag.Data("val", true).Data("val-required", msg);
                    else
                        htmlTag.Data("rule-required", true).Data("msg-required", msg);
                }
                else 
                    htmlTag.AddClass("required");
            }
        }

        public void AddLengthClasses(IEnumerable<ValidationAttribute> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var lengthValidator = propertyValidators.OfType<StringLengthAttribute>().FirstOrDefault();
            if (lengthValidator != null)
            {
                htmlTag.Attr("maxlength", lengthValidator.MaximumLength);

                if (!_msUnobtrusive && requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
                {
                    htmlTag.Data("rule-range", "[" + lengthValidator.MinimumLength + "," + lengthValidator.MaximumLength + "]");
                    htmlTag.Data("msg-range", lengthValidator.FormatErrorMessage(requestData.Accessor.Name));
                }
            }
        }
    }
}