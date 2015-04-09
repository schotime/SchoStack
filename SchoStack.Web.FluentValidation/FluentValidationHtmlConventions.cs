using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation;
using FluentValidation.Internal;
using FluentValidation.Validators;
using FubuCore;
using HtmlTags;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.FluentValidation;

namespace SchoStack.Web.Conventions
{
    public class FluentValidationHtmlConventions : HtmlConvention
    {
        private readonly bool _msUnobtrusive;
        public List<Action<IEnumerable<PropertyValidatorResult>, HtmlTag, RequestData>> RuleProviders = new List<Action<IEnumerable<PropertyValidatorResult>, HtmlTag, RequestData>>();

        public FluentValidationHtmlConventions() 
            : this(new FluentValidatorFinder(), true) 
        {
        }

        public FluentValidationHtmlConventions(IValidatorFinder validatorFinder)
            : this(validatorFinder, true)
        {
        }

        public FluentValidationHtmlConventions(IValidatorFinder validatorFinder, bool msUnobtrusive)
        {
            _msUnobtrusive = msUnobtrusive;
            RuleProviders.Add(AddLengthClasses);
            RuleProviders.Add(AddRequiredClass);
            RuleProviders.Add(AddCreditCardClass);
            RuleProviders.Add(AddEqualToDataAttr);
            RuleProviders.Add(AddRegexData);
            RuleProviders.Add(AddEmailData);

            Inputs.Always.Modify((h, r) =>
            {
                var propertyValidators = validatorFinder.FindValidators(r);
                foreach (var ruleProvider in RuleProviders)
                {
                    ruleProvider.Invoke(propertyValidators, h, r);
                }
            });
        }

        public static string GetMessage(RequestData requestData, PropertyValidatorResult propertyValidator)
        {
            MessageFormatter formatter = new MessageFormatter().AppendPropertyName(propertyValidator.DisplayName);
            string message = formatter.BuildMessage(propertyValidator.PropertyValidator.ErrorMessageSource.GetString());
            return message;
        }

        public void AddEqualToDataAttr(IEnumerable<PropertyValidatorResult> propertyValidators, HtmlTag htmlTag, RequestData request)
        {
            var result = propertyValidators.FirstOrDefault(x => x.PropertyValidator is EqualValidator);
            if (result != null)
            {
                var equal = result.PropertyValidator.As<EqualValidator>();
                MessageFormatter formatter = new MessageFormatter()
                    .AppendPropertyName(result.DisplayName)
                    .AppendArgument("ComparisonValue", equal.ValueToCompare);

                string message = formatter.BuildMessage(equal.ErrorMessageSource.GetString());

                if (_msUnobtrusive)
                {
                    htmlTag.Data("val", true);
                    htmlTag.Data("val-equalto", message);
                    if (request.Accessor.PropertyNames.Length > 1)
                        htmlTag.Data("val-equalto-other", request.Id.Replace("_" + request.Accessor.Name, "") + "_" + equal.MemberToCompare.Name);
                    else
                        htmlTag.Data("val-equalto-other", "*." + equal.MemberToCompare.Name);
                }
                else
                {
                    htmlTag.Data("msg-equalto", message);
                    if (request.Accessor.PropertyNames.Length > 1)
                        htmlTag.Data("rule-equalto", "#" + request.Id.Replace("_" + request.Accessor.Name, "") + "_" + equal.MemberToCompare.Name);
                    else
                        htmlTag.Data("rule-equalto", "#" + equal.MemberToCompare.Name);
                }
            }
        }

        public void AddRequiredClass(IEnumerable<PropertyValidatorResult> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var result = propertyValidators.FirstOrDefault(x => x.PropertyValidator is NotEmptyValidator
                                                             || x.PropertyValidator is NotNullValidator);

            if (result != null)
            {
                if (requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
                {
                    if (_msUnobtrusive) 
                        htmlTag.Data("val", true).Data("val-required", GetMessage(requestData, result) ?? string.Empty);
                    else    
                        htmlTag.Data("rule-required", true).Data("msg-required", GetMessage(requestData, result) ?? string.Empty);
                }
                else
                    htmlTag.AddClass("required");
            }
        }

        public void AddLengthClasses(IEnumerable<PropertyValidatorResult> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var result = propertyValidators.FirstOrDefault(x => x.PropertyValidator is LengthValidator);
            if (result != null)
            {
                htmlTag.Attr("maxlength", result.PropertyValidator.As<LengthValidator>().Max);
            }
        }

        public void AddCreditCardClass(IEnumerable<PropertyValidatorResult> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var lengthValidator = propertyValidators.Select(x => x.PropertyValidator).OfType<CreditCardValidator>().FirstOrDefault();
            if (lengthValidator != null)
            {
                if (requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
                {
                    if (!_msUnobtrusive)
                    {
                        htmlTag.Data("rule-creditcard", true);
                    }
                }
                else
                {
                    htmlTag.AddClass("creditcard");    
                }
            }
        }

        public void AddRegexData(IEnumerable<PropertyValidatorResult> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var result = propertyValidators.FirstOrDefault(x => x.PropertyValidator is RegularExpressionValidator);

            if (result != null && requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
            {
                var regex = result.As<RegularExpressionValidator>();
                var msg = GetMessage(requestData, result) ?? string.Format("The value did not match the regular expression '{0}'", regex.Expression);
                if (_msUnobtrusive)
                    htmlTag.Data("val", true).Data("val-regex", msg).Data("val-regex-pattern", regex.Expression);
                else
                    htmlTag.Data("rule-regex", regex.Expression).Data("msg-regex", msg);
            }
        }

        public void AddEmailData(IEnumerable<PropertyValidatorResult> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var result = propertyValidators.FirstOrDefault(x => x.PropertyValidator is EmailValidator);
            if (result != null)
            {
                if (requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
                {
                    var msg = GetMessage(requestData, result) ?? string.Format("The value is not a valid email address");
                    if (_msUnobtrusive) 
                        htmlTag.Data("val", true).Data("val-email", msg);
                    else
                        htmlTag.Data("rule-email", true).Data("msg-email", msg);
                }
                else
                    htmlTag.AddClass("email");
            }
        }
    }
}