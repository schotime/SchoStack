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
using SchoStack.Web.Html;
using Shouldly;

namespace SchoStack.Tests.HtmlConventions
{
    public class DataAnnotValidationConventionTests
    {
        public DataAnnotValidationConventionTests()
        {
            HtmlConventionFactory.Add(new DataAnnotationValidationHtmlConventions());
        }

        [Test]
        public void PropertyWithRequiredAttributeDefinedShouldHaveRequiredClass()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof (TestInputModel);
            var tag = helper.Input(x => x.Name);
            tag.HasClass("required").ShouldBe(true);
        }

        [Test]
        public void PropertyWithRequiredAttributeDefinedShouldHaveUnObtrusiveDataAttributesIfEnabled()
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
        public void PropertyWithStringLengthAttributeDefinedShouldHaveMaxLengthAttr()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.Name);
            tag.HasAttr("maxlength").ShouldBe(true);
            tag.Attr("maxlength").ShouldBe("20");
        }

        [Test]
        public void PropertyWithStringLengthAttributeDefinedShouldHaveMinLengthAttr()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.Name);
            tag.HasAttr("minlength").ShouldBe(true);
            tag.Attr("minlength").ShouldBe("3");
        }
        
        [Test]
        public void PropertyWithCompareAttributeDefinedShouldHaveValEqualToPassword()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.PasswordConfirm);
            tag.Data("val-equalto").ShouldBe("The Error");
            tag.Data("val-equalto-other").ShouldBe("*.Password");
        }
    }
}
