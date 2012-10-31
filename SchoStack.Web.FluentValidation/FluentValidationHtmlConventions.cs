using System.Collections.Generic;
using System.Linq;
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
                AddRequiredClass(propertyValidators, h);
                AddCreditCardClass(propertyValidators, h);
                AddEqualToDataAttr(propertyValidators, h, r);
            });
        }

        private static void AddEqualToDataAttr(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag, RequestData request)
        {
            var equal = propertyValidators.OfType<EqualValidator>().FirstOrDefault();
            if (equal != null)
            {
                if (request.Accessor.PropertyNames.Length > 1)
                {
                    htmlTag.Data("val-equalTo", request.Id.Replace("_" + request.Accessor.Name, "") + "_" + equal.MemberToCompare.Name);
                }
                else
                {
                    htmlTag.Data("val-equalTo", equal.MemberToCompare.Name);
                }
            }
        }

        private static void AddRequiredClass(IEnumerable<IPropertyValidator> propertyValidators, HtmlTag htmlTag)
        {
            var notEmpty = propertyValidators.FirstOrDefault(x => x.GetType() == typeof (NotEmptyValidator)
                                                               || x.GetType() == typeof (NotNullValidator));
            if (notEmpty != null)
            {
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
    }
}