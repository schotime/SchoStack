using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HtmlTags;
using NUnit.Framework;
using Promaster.Tests;
using SchoStack.Web.Conventions;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.Html;
using Shouldly;

namespace SchoStack.Tests.HtmlConventions
{
    public class HtmlProfileTests
    {
        [Test]
        public void ProfileBasic()
        {
            HtmlConventionFactory.Add(new DefaultHtmlConventions());
            HtmlConventionFactory.Add(new DataAnnotationHtmlConventions());

            var model = new ProfileViewModel() { Name = "Test" };
            var helper = MvcMockHelpers.GetHtmlHelper(model);

            helper.Profile(new ProfileTest1());

            var tag = helper.Input(x => x.Name);
            tag.ToHtmlString().ShouldBe("<profile>Test</profile>");
        }

        [Test]
        public void ProfileWithUsing()
        {
            HtmlConventionFactory.Add(new DefaultHtmlConventions());
            HtmlConventionFactory.Add(new DataAnnotationHtmlConventions());

            var model = new ProfileViewModel() { Name = "Test" };
            var helper = MvcMockHelpers.GetHtmlHelper(model);

            using (helper.Profile(new ProfileTest1()))
            {
                var tag = helper.Input(x => x.Name);
                tag.ToHtmlString().ShouldBe("<profile>Test</profile>");
            }

            var tag1 = helper.Input(x => x.Name);
            tag1.ToHtmlString().ShouldBe("<input type=\"text\" value=\"Test\" id=\"Name\" name=\"Name\" />");
        }

        [Test]
        public void ProfileWithNesteds()
        {
            HtmlConventionFactory.Add(new DefaultHtmlConventions());
            HtmlConventionFactory.Add(new DataAnnotationHtmlConventions());

            var model = new ProfileViewModel() { Name = "Test" };
            var helper = MvcMockHelpers.GetHtmlHelper(model);

            using (helper.Profile(new ProfileTest1()))
            {
                var tag = helper.Input(x => x.Name);
                tag.ToHtmlString().ShouldBe("<profile>Test</profile>");

                using (helper.Profile(new ProfileTest2()))
                {
                    var tag3 = helper.Input(x => x.Name);
                    tag3.ToHtmlString().ShouldBe("<profilenested>Test</profilenested>");
                }

                var tag2 = helper.Input(x => x.Name);
                tag2.ToHtmlString().ShouldBe("<profile>Test</profile>");
            }

            var tag1 = helper.Input(x => x.Name);
            tag1.ToHtmlString().ShouldBe("<input type=\"text\" value=\"Test\" id=\"Name\" name=\"Name\" />");
        }
    }

    public class ProfileTest2 : HtmlProfile
    {
        public ProfileTest2()
        {
            HtmlConventions.Add(new TestConvention2());
        }

        public class TestConvention2 : HtmlConvention
        {
            public TestConvention2()
            {
                Inputs.If<string>().BuildBy(x => new HtmlTag("profilenested").Text(x.GetValue<string>()));
            }
        }
    }

    public class ProfileViewModel
    {
        public string Name { get; set; }
    }

    public class ProfileTest1 : HtmlProfile
    {
        public ProfileTest1()
        {
            HtmlConventions.Add(new TestConvention1());
        }

        public class TestConvention1 : HtmlConvention
        {
            public TestConvention1()
            {
                Inputs.If<string>().BuildBy(x => new HtmlTag("profile").Text(x.GetValue<string>()));
            }
        }
    }
}
