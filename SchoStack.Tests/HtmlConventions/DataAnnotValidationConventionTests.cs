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
            HtmlConventionFactory.Add(new DefaultHtmlConventions());
            HtmlConventionFactory.Add(new DataAnnotationHtmlConventions());
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
        public void PropertyWithComapreAttributeDefinedShouldHaveValEqualToPassword()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE] = typeof(TestInputModel);
            var tag = helper.Input(x => x.PasswordConfirm);
            tag.Data("val-equalTo").ShouldNotBe(null);
            tag.Data("val-equalTo").ShouldBe("Password");
        }
    }
}
