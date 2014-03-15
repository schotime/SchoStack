using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Mvc;
using NUnit.Framework;
using Promaster.Tests;
using SchoStack.Web.Conventions;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.Html;
using Shouldly;

namespace SchoStack.Tests.HtmlConventions
{
    public class ReusingConventionTests
    {
        public ReusingConventionTests()
        {
            HtmlConventionFactory.Add(new DefaultHtmlConventions());
            HtmlConventionFactory.Add(new ReusingConventions());
        }

        [Test]
        public void CallingBuildShouldRunTheConventionsAsIfItWasAnotherType()
        {
            var model = new ReusingViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.AnimalId);    
            tag.TagName().ShouldBe("select");
            Console.WriteLine(tag);
        }
    }

    public class ReusingConventions : HtmlConvention
    {
        public ReusingConventions()
        {
            Inputs.If(x=>x.Accessor.InnerProperty.Name == "AnimalId").BuildBy((x, p) =>
            {
                var s = new[] { new SelectListItem() { Text = "Cow", Value = "1" }, new SelectListItem() { Text = "Dog", Value = "2" } };
                return p.Build(s);
            });
        }
    }

    public class ReusingViewModel
    {
        public int AnimalId { get; set; }
    }
}