using System.Collections.Generic;
using System.Linq;
using FluentValidation.Validators;
using HtmlTags;
using SchoStack.Web.HtmlTags;

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
                AddNumberClasses(r, h);
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

        private static void AddNumberClasses(RequestData r, HtmlTag h)
        {
            if (r.Accessor.PropertyType == typeof(int) || r.Accessor.PropertyType == typeof(int?))
            {
                h.AddClass("digits");
            }

            if (r.Accessor.PropertyType == typeof(double) || r.Accessor.PropertyType == typeof(double?)
                || r.Accessor.PropertyType == typeof(decimal) || r.Accessor.PropertyType == typeof(decimal?)
                || r.Accessor.PropertyType == typeof(float) || r.Accessor.PropertyType == typeof(float?))
            {
                h.AddClass("number");
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


    //public static class ex
    //{
    //    public static IRuleBuilderOptions<T, TProperty> PropertyEqual<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, Expression<Func<T, TProperty>> toCompare, IEqualityComparer comparer = null)
    //    {
    //        var func = toCompare.Compile();
    //        return ruleBuilder.SetValidator(new PropertyEqualValidator(Extensions.CoerceToNonGeneric(func), toCompare.GetMember(), ReflectionHelper.GetAccessor(toCompare)));
    //    }
    //}

    //public class PropertyEqualValidator: PropertyValidator, IComparisonValidator
    //{
    //    readonly Func<object, object> func;
    //    public readonly Accessor Accessor;
    //    readonly IEqualityComparer comparer;

    //    public PropertyEqualValidator(Func<object, object> property, MemberInfo member, Accessor accessor)
    //        : base(() => Messages.equal_error)
    //    {
    //        func = property;
    //        Accessor = accessor;
    //        MemberToCompare = member;
    //    }

    //    protected override bool IsValid(PropertyValidatorContext context)
    //    {
    //        var comparisonValue = GetComparisonValue(context);
    //        bool success = Compare(comparisonValue, context.PropertyValue);

    //        if (!success)
    //        {
    //            context.MessageFormatter.AppendArgument("PropertyValue", comparisonValue);
    //            return false;
    //        }

    //        return true;
    //    }

    //    private object GetComparisonValue(PropertyValidatorContext context)
    //    {
    //        return func(context.Instance);
    //    }

    //    public Comparison Comparison
    //    {
    //        get { return Comparison.Equal; }
    //    }

    //    public MemberInfo MemberToCompare { get; private set; }

    //    public object ValueToCompare
    //    {
    //        get { throw new NotImplementedException(); }
    //    }

    //    protected bool Compare(object comparisonValue, object propertyValue)
    //    {
    //        if (comparer != null)
    //        {
    //            return comparer.Equals(comparisonValue, propertyValue);
    //        }

    //        return Equals(comparisonValue, propertyValue);
    //    }
    //}
}