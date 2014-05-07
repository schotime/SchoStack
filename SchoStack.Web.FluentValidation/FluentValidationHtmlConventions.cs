using System;
using System.Collections.Generic;
using System.Linq;
using FluentValidation.Internal;
using FluentValidation.Validators;
using HtmlTags;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.FluentValidation;

namespace SchoStack.Web.Conventions
{
    public class FluentValidationHtmlConventions : HtmlConvention
    {
        private readonly bool _msUnobtrusive;
        public List<Action<IEnumerable<IPropertyValidator>, HtmlTag, RequestData>> RuleProviders = new List<Action<IEnumerable<IPropertyValidator>, HtmlTag, RequestData>>();

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
            RuleProviders.Add(AddMinLengthClasses);
            RuleProviders.Add(AddMaxLengthClasses);

            Inputs.Always.Modify((h, r) =>
            {
                var propertyValidators = validatorFinder.FindValidators(r);
                foreach (var ruleProvider in RuleProviders)
                {
                    ruleProvider.Invoke(propertyValidators, h, r);
                }
            });
        }

        public static string GetMessage(RequestData requestData, IPropertyValidator propertyValidator)
        {
            MessageFormatter formatter = new MessageFormatter().AppendPropertyName(requestData.Accessor.InnerProperty.Name.SplitPascalCase());
            string message = formatter.BuildMessage(propertyValidator.ErrorMessageSource.GetString());
            return message;
        }

        public void AddEqualToDataAttr(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData request)
        {
            var equal = propertyValidators.OfType<EqualValidator>().FirstOrDefault();
            if (equal != null)
            {
                MessageFormatter formatter = new MessageFormatter()
                    .AppendPropertyName(request.Accessor.InnerProperty.Name.SplitPascalCase())
                    .AppendArgument("PropertyValue", equal.MemberToCompare.Name.SplitPascalCase());
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

        public void AddRequiredClass(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var notEmpty = propertyValidators.FirstOrDefault(x => x.GetType() == typeof (NotEmptyValidator)
                                                               || x.GetType() == typeof (NotNullValidator));
            if (notEmpty != null)
            {
                if (requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
                {
                    if (_msUnobtrusive) 
                        htmlTag.Data("val", true).Data("val-required", GetMessage(requestData, notEmpty) ?? string.Empty);
                    else    
                        htmlTag.Data("rule-required", true).Data("msg-required", GetMessage(requestData, notEmpty) ?? string.Empty);
                }
                else
                    htmlTag.AddClass("required");
            }
        }

        public void AddLengthClasses(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var lengthValidator = propertyValidators.OfType<LengthValidator>().FirstOrDefault();
            if (lengthValidator != null)
            {
                htmlTag.Attr("maxlength", lengthValidator.Max);

                if (!_msUnobtrusive && requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
                {
                    htmlTag.Data("rule-range", "[" + lengthValidator.Min + "," + lengthValidator.Max + "]");
                    htmlTag.Data("msg-range", GetMessage(requestData, lengthValidator) ?? string.Empty);
                }
            }
        }

        public void AddMaxLengthClasses(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var maxlengthValidator = propertyValidators.OfType<MaximumLengthValidator>().FirstOrDefault();
            if (maxlengthValidator != null)
            {
                htmlTag.Attr("maxlength", maxlengthValidator.Max);

                if (!_msUnobtrusive && requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
                {
                    htmlTag.Data("rule-maxlength", maxlengthValidator.Max);
                    htmlTag.Data("msg-maxlength", GetMessage(requestData, maxlengthValidator) ?? string.Empty);
                }
            }
        }

        public void AddMinLengthClasses(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var minlengthValidator = propertyValidators.OfType<MinimumLengthValidator>().FirstOrDefault();
            if (minlengthValidator != null)
            {
                if (!_msUnobtrusive && requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
                {
                    htmlTag.Data("rule-minlength", minlengthValidator.Min);
                    htmlTag.Data("msg-minlength", GetMessage(requestData, minlengthValidator) ?? string.Empty);
                }
            }
        }

        public void AddCreditCardClass(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var lengthValidator = propertyValidators.OfType<CreditCardValidator>().FirstOrDefault();
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

        public void AddRegexData(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var regex = propertyValidators.OfType<RegularExpressionValidator>().FirstOrDefault();
            if (regex != null && requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
            {
                var msg = GetMessage(requestData, regex) ?? string.Format("The value did not match the regular expression '{0}'", regex.Expression);
                if (_msUnobtrusive)
                    htmlTag.Data("val", true).Data("val-regex", msg).Data("val-regex-pattern", regex.Expression);
                else
                    htmlTag.Data("rule-regex", regex.Expression).Data("msg-regex", msg);
            }
        }

        public void AddEmailData(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var emailValidator = propertyValidators.OfType<EmailValidator>().FirstOrDefault();
            if (emailValidator != null)
            {
                if (requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
                {
                    var msg = GetMessage(requestData, emailValidator) ?? string.Format("The value is not a valid email address");
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