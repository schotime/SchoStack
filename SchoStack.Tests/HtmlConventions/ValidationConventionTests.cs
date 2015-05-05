using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using FluentValidation;
using FubuCore.Reflection;
using Moq;
using NUnit.Framework;
using Promaster.Tests;
using SchoStack.Web;
using SchoStack.Web.Conventions;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.FluentValidation;
using SchoStack.Web.Html;
using SchoStack.Web.Html.UrlForm;
using Shouldly;

namespace SchoStack.Tests.HtmlConventions
{
    public class ValidationConventionTests
    {
        private Mock<IDependencyResolver> _dep;

        public ValidationConventionTests()
        {
            HtmlConventionFactory.Add(new DefaultHtmlConventions());
            HtmlConventionFactory.Add(new DataAnnotationHtmlConventions());
            _dep = new Mock<IDependencyResolver>();
            var dict = new Dictionary<Type, IValidator>();
            dict.Add(typeof(IValidator<TestInputModel>), new TestInputValidator());
            _dep.Setup(x => x.GetService(It.IsAny<Type>())).Returns<Type>(x => dict[x]);
            HtmlConventionFactory.Add(new FluentValidationHtmlConventions(new FluentValidatorFinder(_dep.Object)));

            HtmlConventionFactory.Add(new CombinationConventions());
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
        public void PropertyWithNotEmptyFluentValidationDefinedShouldHaveUnObtrusiveDataAttributesWithDefaultMessage()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.CreditCard);
            tag.Attr("data-val-required").ShouldBe("'Credit Card' should not be empty.");
        }

        [Test]
        public void PropertyWithRegexDefinedShouldHaveUnObtrusiveDataAttributesWithMessageIfEnabled()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.UnobtrusiveJavaScriptEnabled = true;
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
            tag.Data("val-equalto").ShouldBe("'Password Confirm' should be equal to 'Password'.");
            tag.Data("val-equalto-other").ShouldBe("*.Password");
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

        [Test]
        public void PropertyInViewModelButNotInInputModel()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            Assert.DoesNotThrow(() =>
                                    {
                                        var tag = helper.Input(x => x.NotInInputModel);
                                    });
        }

        [Test]
        public void RenderTagConditionally()
        {
            
            var selectListItems = new List<SelectListItem>() {new SelectListItem() {Text = "testtext", Value = "testvalue"}};
            var model = new TestViewModel()
            {
                CombinationType = new CombinationType()
                {
                    Items = selectListItems
                },
                Dropdown = selectListItems
            };
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag1 = helper.Input(x => x.CombinationType);
            var tag2 = helper.Input(x => x.Dropdown).Attr("id", "CombinationType").Attr("name", "CombinationType");
            Assert.AreEqual(tag1.ToHtmlString(), tag2.ToHtmlString());
        }

        [Test]
        public void RenderTagConditionally2()
        {

            var selectListItems = new List<SelectListItem>() { new SelectListItem() { Text = "testtext", Value = "testvalue" } };
            var model = new TestViewModel()
            {
                CombinationType = new CombinationType()
                {
                    Items = selectListItems,
                    IsSingle = true,
                    Name = "Text",
                    Value = "Value"
                },
                Dropdown = selectListItems
            };
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.CombinationType);
            Assert.AreEqual("<div>Text<input type=\"hidden\" id=\"CombinationType\" name=\"CombinationType\" value=\"Value\" /></div>", tag.ToHtmlString());
        }

        [Test, RequiresSTA]
        public void EnsureAllCanBeUsedWithNonExpressionBasedConventions()
        {
            ActionFactory.Actions.Clear();
            ActionFactory.Actions.Add(typeof(TestInputModel), new ActionInfo());
            RouteTable.Routes.Clear();
            RouteTable.Routes.Add(typeof(TestInputModel).FullName, new Route("fakeUrl", null));

            var model = new TestViewModel() { CreatedAt = new DateTime(2005, 03, 04) };
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            helper.Form<TestInputModel>();
            var tag = helper.Input(x => x.CreatedAt);
            tag.Text().ShouldBe("20050304");
        }

        [Test]
        public void PropertyWithNumber()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.NameWithNumber2);
            tag.HasAttr("maxlength").ShouldBe(true);
        }

        [Test]
        public void ArrayProperty()
        {
            var model = new TestViewModel()
            {
                ArrayTypes = new[] { new ArrayType() { IntProp = 0, StringProp = "s"} }
            };
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.ArrayTypes[0].IntProp);
            tag.HasClass("required").ShouldBe(true);
        }

        [Test]
        public void FindNestedNames()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);

            var finder = new FluentValidatorFinder(_dep.Object);
            var vals = finder.FindValidators(RequestData.BuildRequestData(helper.ViewContext, ReflectionHelper.GetAccessor<TestViewModel>(x => x.Nested.ReallyLongName), typeof(TestInputModel)));

            vals.Count().ShouldBe(1);
            vals.First().DisplayName.ShouldBe("ReallyLongLongName");
        }

        [Test]
        public void FindNestedNamesWithWhen()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);

            var finder = new FluentValidatorFinder(_dep.Object);
            var vals = finder.FindValidators(RequestData.BuildRequestData(helper.ViewContext, ReflectionHelper.GetAccessor<TestViewModel>(x => x.Nested1.ReallyLongName), typeof(TestInputModel)));

            vals.Count().ShouldBe(1);
            vals.First().DisplayName.ShouldBe("ReallyLongLongNameWhen");
        }
    }
}
