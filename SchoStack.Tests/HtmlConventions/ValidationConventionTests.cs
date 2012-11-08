using System;
using System.Collections.Generic;
using System.Web.Mvc;
using FluentValidation;
using Moq;
using NUnit.Framework;
using Promaster.Tests;
using SchoStack.Web;
using SchoStack.Web.Conventions;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.FluentValidation;
using SchoStack.Web.Html;
using Shouldly;

namespace SchoStack.Tests.HtmlConventions
{
    public class ValidationConventionTests
    {
        public ValidationConventionTests()
        {
            HtmlConventionFactory.Add(new DefaultHtmlConventions());
            HtmlConventionFactory.Add(new DataAnnotationHtmlConventions());
            var dep = new Mock<IDependencyResolver>();
            var dict = new Dictionary<Type, IValidator>();
            dict.Add(typeof(IValidator<TestInputModel>), new TestInputValidator());
            dep.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(x => dict[x]);
            HtmlConventionFactory.Add(new FluentValidationHtmlConventions(new FluentValidatorFinder(dep.Object)));
        }

        [Test]
        public void PropertyWithNotEmptyFluentValidationDefinedShouldHaveRequiredClass()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof (TestInputModel);
            var tag = helper.Input(x => x.Name);
            tag.HasClass("required").ShouldBe(true);
        }

        [Test]
        public void PropertyWithNotEmptyFluentValidationDefinedShouldHaveUnObtrusiveDataAttributesIfEnabled()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.Name);
            tag.HasAttr("data-val").ShouldBe(true);
            tag.HasAttr("data-val-required").ShouldBe(true);
        }

        [Test]
        public void PropertyWithNotEmptyFluentValidationDefinedShouldHaveUnObtrusiveDataAttributesWithMessageIfEnabled()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.Name);
            tag.Attr("data-val-required").ShouldBe("Test Message");
        }

        [Test]
        public void PropertyWithRegexDefinedShouldHaveUnObtrusiveDataAttributesWithMessageIfEnabled()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.DisplayName);
            tag.Attr("data-val-regex").ShouldBe("Regex No Match");
            tag.Attr("data-val-regex-pattern").ShouldBe("[a-zA-Z]");
        }

        [Test]
        public void PropertyWithLengthFluentValidationDefinedShouldHaveMaxLengthAttribute()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.Name);
            tag.HasAttr("maxlength").ShouldBe(true);
            tag.Attr("maxlength").ShouldBe(TestInputValidator.NAME_MAXLENGTH.ToString());
        }

        [Test]
        public void PropertyWithLengthFluentValidationDefinedShouldHaveMinLengthAttribute()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.Name);
            tag.HasAttr("minlength").ShouldBe(true);
            tag.Attr("minlength").ShouldBe(TestInputValidator.NAME_MINLENGTH.ToString());
        }

        [Test]
        public void PropertyWithCreditCardFluentValidationDefinedShouldHaveCreditCardClass()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.CreditCard);
            tag.HasClass("creditcard").ShouldBe(true);
        }

        [Test]
        public void PropertyWithEqualFluentValidationShouldHaveDataAttributeValEqualTo()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.PasswordConfirm);
            tag.Data("val-equalTo").ShouldNotBe(null);
            tag.Data("val-equalTo").ShouldBe("Password");
        }

        [Test]
        public void PropertyOfTypeIntShouldHaveDigitsClass()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.Int);
            tag.HasClass("digits").ShouldBe(true);
        }

        [Test]
        public void PropertyOfTypeDecimalShouldHaveNumberClass()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.Decimal);
            tag.HasClass("number").ShouldBe(true);
        }

        [Test]
        public void PropertyOfTypeDoubleShouldHaveNumberClass()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.Double);
            tag.HasClass("number").ShouldBe(true);
        }
    }
}
