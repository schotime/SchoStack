using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Web.Mvc;
using FluentValidation;
using SchoStack.Web;

namespace SchoStack.Tests.HtmlConventions
{
    public class TestViewModel
    {
        [StringLength(20)]
        public string Name { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string PasswordConfirm { get; set; }

        [DataType(DataType.Text)]
        public string Text { get; set; }

        [HiddenInput]
        public string Hidden { get; set; }

        [DisplayName("Display")]
        public string DisplayName { get; set; }

        public bool IsCorrect { get; set; }

        public List<SelectListItem> Dropdown { get; set; }
        public MultiSelectList MultiSelect { get; set; }
        public int Int { get; set; }
        public decimal Decimal { get; set; }
        public double Double { get; set; }
        public string CreditCard { get; set; }

        public bool NotInInputModel { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTimeOffset CreatedAtOffset { get; set; }

        public CombinationType CombinationType { get; set; }

        public string NameWithNumber1 { get; set; }
        public string NameWithNumber2 { get; set; }

        public ArrayType[] ArrayTypes { get; set; }
        
        public DateTime[] DateTimeArray { get; set; }

        [TestDateTimeArray]
        public DateTime[] DateTimeArrayAtt { get; set; }

        [BindAlias("VA")]
        public string Alias { get; set; }

        [BindAlias("VNEST")]
        public NestedAlias Nested { get; set; }

        [BindAlias("VNLIST")]
        public List<NestedAlias> NestedList { get; set; }

        public NestedAlias Nested1 { get; set; }
    }

    public class TestDateTimeArrayAttribute : Attribute
    {
        
    }

    public class NestedAlias
    {
        [BindAlias("RLN")]
        public string ReallyLongName { get; set; }
    }

    public class ArrayType
    {
        public string StringProp { get; set; }
        public int? IntProp { get; set; }
    }

    public class CombinationType
    {
        public IEnumerable<SelectListItem> Items { get; set; }
        public bool IsSingle { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class TestInputModel
    {
        [StringLength(20, MinimumLength = 3)]
        [Required]
        public string Name { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [System.ComponentModel.DataAnnotations.Compare("Password", ErrorMessage = "The Error")]
        public string PasswordConfirm { get; set; }

        [DataType(DataType.Text)]
        public string Text { get; set; }

        [HiddenInput]
        public string Hidden { get; set; }

        [DisplayName("Display")]
        public string DisplayName { get; set; }

        public bool IsCorrect { get; set; }

        public List<SelectListItem> Dropdown { get; set; }
        public MultiSelectList MultiSelect { get; set; }
        
        public int Int { get; set; }
        public decimal Decimal { get; set; }
        public double Double { get; set; }
        public string CreditCard { get; set; }

        public DateTime CreatedAt { get; set; }

        public string NameWithNumber1 { get; set; }
        public string NameWithNumber2 { get; set; }

        public ArrayType[] ArrayTypes { get; set; }

        [BindAlias("A")]
        public string Alias { get; set; }

        [BindAlias("NEST")]
        public NestedAlias Nested { get; set; }

        public NestedAlias Nested1 { get; set; }

        [BindAlias("NLIST")]
        public List<NestedAlias> NestedList { get; set; }
    }

    public class TestInputValidator : AbstractValidator<TestInputModel>
    {
        public const int NAME_MAXLENGTH = 50;
        public const int NAME_MINLENGTH = 10;

        public TestInputValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Test Message")
                .Length(NAME_MINLENGTH, NAME_MAXLENGTH);

            RuleFor(x => x.CreditCard)
                .CreditCard()
                .NotEmpty();

            RuleFor(x => x.PasswordConfirm)
                .Equal(x => x.Password);

            RuleFor(x => x.DisplayName)
                .Matches("[a-zA-Z]")
                .WithMessage("Regex No Match");

            RuleFor(x => x.NameWithNumber1)
                .Length(1, 100)
                .NotEmpty();
            RuleFor(x => x.NameWithNumber2)
                .Length(1, 100)
                .NotEmpty();

            RuleFor(x => x.ArrayTypes)
                .SetCollectionValidator(new InlineValidator<ArrayType>()
                {
                    v => v.RuleFor(y => y.IntProp).NotEmpty()
                });

            RuleFor(x => x.Nested)
                .SetValidator(new InlineValidator<NestedAlias>()
                {
                    x => x.RuleFor(z => z.ReallyLongName).NotEmpty().WithName("ReallyLongLongName")
                });

            RuleFor(x => x.Nested1)
                .SetValidator(new InlineValidator<NestedAlias>()
                {
                    x => x.RuleFor(z => z.ReallyLongName).NotEmpty().WithName("ReallyLongLongNameWhen")
                }).When(x => x.IsCorrect);
        }
    }
}