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
        public FluentValidationHtmlConventions(IValidatorFinder validatorFinder)
        {
            Inputs.Always.Modify((h, r) =>
            {
                var propertyValidators = validatorFinder.FindValidators(r);
                AddLengthClasses(propertyValidators, h);
                AddRequiredClass(propertyValidators, h, r);
                AddCreditCardClass(propertyValidators, h);
                AddEqualToDataAttr(propertyValidators, h, r);
                AddRegexData(propertyValidators, h, r);
                AddEmailData(propertyValidators, h, r);
            });
        }

        private static string GetMessage(RequestData requestData, IPropertyValidator notEmpty)
        {
            MessageFormatter formatter = new MessageFormatter().AppendPropertyName(requestData.Accessor.InnerProperty.Name.SplitPascalCase());
            string message = formatter.BuildMessage(notEmpty.ErrorMessageSource.GetString());
            return message;
        }

        private static void AddEqualToDataAttr(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData request)
        {
            var equal = propertyValidators.OfType<EqualValidator>().FirstOrDefault();
            if (equal != null)
            {
                htmlTag.Data("val", true);

                MessageFormatter formatter = new MessageFormatter()
                    .AppendPropertyName(request.Accessor.InnerProperty.Name.SplitPascalCase())
                    .AppendArgument("PropertyValue", equal.MemberToCompare.Name.SplitPascalCase());
                string message = formatter.BuildMessage(equal.ErrorMessageSource.GetString());

                htmlTag.Data("val-equalto", message);

                if (request.Accessor.PropertyNames.Length > 1)
                {
                    htmlTag.Data("val-equalto-other", request.Id.Replace("_" + request.Accessor.Name, "") + "_" + equal.MemberToCompare.Name);
                }
                else
                {
                    htmlTag.Data("val-equalto-other", "*." + equal.MemberToCompare.Name);
                }
            }
        }

        private static void AddRequiredClass(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var notEmpty = propertyValidators.FirstOrDefault(x => x.GetType() == typeof (NotEmptyValidator)
                                                               || x.GetType() == typeof (NotNullValidator));
            if (notEmpty != null)
            {
                if (requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
                    htmlTag.Data("val", true).Data("val-required", GetMessage(requestData, notEmpty) ?? string.Empty);
                else
                    htmlTag.AddClass("required");
            }
        }

        private static void AddLengthClasses(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag)
        {
            var lengthValidator = propertyValidators.OfType<LengthValidator>().FirstOrDefault();
            if (lengthValidator != null)
            {
                htmlTag.Attr("maxlength", lengthValidator.Max);
                if (lengthValidator.Min > 0)
                    htmlTag.Attr("minlength", lengthValidator.Min);
            }
        }

        private static void AddCreditCardClass(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag)
        {
            var lengthValidator = propertyValidators.OfType<CreditCardValidator>().FirstOrDefault();
            if (lengthValidator != null)
            {
                htmlTag.AddClass("creditcard");
            }
        }

        private static void AddRegexData(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var regex = propertyValidators.OfType<RegularExpressionValidator>().FirstOrDefault();
            if (regex != null)
            {
                htmlTag.Data("val", true).Data("val-regex", GetMessage(requestData, regex) ?? string.Format("The value did not match the regular expression '{0}'", regex.Expression)).Data("val-regex-pattern", regex.Expression);
            }
        }

        private static void AddEmailData(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData requestData)
        {
            var emailValidator = propertyValidators.OfType<EmailValidator>().FirstOrDefault();
            if (emailValidator != null)
            {
                if (requestData.ViewContext.UnobtrusiveJavaScriptEnabled)
                    htmlTag.Data("val", true).Data("val-email", GetMessage(requestData, emailValidator) ?? string.Format("The value is not a valid email address"));
                else
                    htmlTag.AddClass("email");
            }
        }
    }
}