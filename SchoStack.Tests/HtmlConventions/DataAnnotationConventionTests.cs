using System;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using FubuCore.Reflection;
using NUnit.Framework;
using Promaster.Tests;
using SchoStack.Web.Conventions;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.Html;
using Shouldly;

namespace SchoStack.Tests.HtmlConventions
{
    public class DataAnnotationConventionTests
    {
        public DataAnnotationConventionTests()
        {
            HtmlConventionFactory.Add(new DefaultHtmlConventions());
            HtmlConventionFactory.Add(new DataAnnotationHtmlConventions());
        }

        [Test]
        public void PropertyWithDataTypePasswordAttributeShouldHaveTheTypePassword()
        {
            var model = new TestViewModel();
            var tag = MvcMockHelpers.GetHtmlHelper(model).Input(x => x.Password);
            tag.TagName().ShouldBe("input");
            tag.Attr("type").ShouldBe("password");
        }

        [Test]
        public void PropertyWithDataTypePasswordAttributeShouldHaveNoValue()
        {
            var model = new TestViewModel() { Password = "password"};
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.Password);
            tag.TagName().ShouldBe("input");
            tag.Attr("value").ShouldBe("");
        }

        [Test]
        public void PropertyWithDataTypeTextAttributeShouldMakeTheTagATextArea()
        {
            var model = new TestViewModel();
            var tag = MvcMockHelpers.GetHtmlHelper(model).Input(x => x.Text);
            tag.TagName().ShouldBe("textarea");
        }

        [Test]
        public void PropertyWithHiddenInputAttributeShouldMakeTheTypeHidden()
        {
            var model = new TestViewModel();
            var tag = MvcMockHelpers.GetHtmlHelper(model).Input(x => x.Hidden);
            tag.TagName().ShouldBe("input");
            tag.Attr("type").ShouldBe("hidden");
        }

        [Test]
        public void PropertyWithStringLengthAttributeShouldAddAMaxLengthAttributeSetToValueSpecified()
        {
            var model = new TestViewModel();
            Expression<Func<TestViewModel, string>> expression = x => x.Name;
            var tag = MvcMockHelpers.GetHtmlHelper(model).Input(expression);
            var max = model.GetType().GetProperty(expression.GetMemberExpression(false).Member.Name).GetAttribute<StringLengthAttribute>().MaximumLength;
            tag.TagName().ShouldBe("input");
            tag.Attr("maxlength").ShouldNotBeEmpty();
            tag.Attr("maxlength").ShouldBe(max.ToString());
        }
    }
}
