using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Promaster.Tests;
using SchoStack.Web;
using SchoStack.Web.Conventions;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.Reflection;

namespace SchoStack.Tests
{
    public class LessStronglyTypedConventions
    {
        public LessStronglyTypedConventions()
        {
            HtmlConventionFactory.Add(new DefaultHtmlConventions());
        }

        [Test]
        public void ShouldBeAbleToGenerateTagForLooselyTypedProperty()
        {
            var setting = new Settings();

            var accessor = ReflectionUtil.GetAccessor(setting.GetType().GetProperty("Name"));

            var tagGenerator = new TagGenerator(HtmlConventionFactory.HtmlConventions);
            var html = tagGenerator.GenerateInputFor(MvcMockHelpers.MockViewContext(), accessor);

            Assert.AreEqual("<input type=\"text\" id=\"Name\" name=\"Name\" />", html.ToString());
        }
    }

    public class Settings
    {
        public string Name { get; set; }
    }
}
