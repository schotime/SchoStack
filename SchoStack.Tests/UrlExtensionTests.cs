using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Web.Routing;
using NUnit.Framework;
using Promaster.Tests;
using SchoStack.Web;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.Html.UrlForm;
using SchoStack.Web.Url;
using Shouldly;

namespace SchoStack.Tests
{
    public class UrlExtensionTests
    {
        [Test]
        public void UrlForInterfaceShouldReturnCorrectUrl()
        {
            var vm = new TestUrlViewModel()
            {
                TestUrl = new TestUrl()
            };

            ActionFactory.Actions.Add(typeof(TestUrl), new ActionInfo());
            RouteTable.Routes.Add(typeof(TestUrl).FullName, new Route("fakeUrl", null));

            var html = MvcMockHelpers.GetHtmlHelper(vm);
            html.Form(vm.TestUrl);

            Assert.AreEqual(typeof(TestUrl), html.ViewContext.HttpContext.Items[TagGenerator.FORMINPUTTYPE]);
        }
        
        public class TestUrlViewModel
        {
            public ITestUrl TestUrl { get; set; }
        }

        public class TestUrl : ITestUrl
        {
        }

        public interface ITestUrl
        {
        }
    }

    public class UrlExtensionTestsForTypes
    {
        [Test]
        public void UrlForInterfaceShouldReturnCorrectUrl()
        {
            ActionFactory.Actions.Add(typeof(TestUrl), new ActionInfo() { });
            RouteTable.Routes.Add(typeof(TestUrl).FullName, new Route("fakeUrl/{Id}", null));

            var url = MvcMockHelpers.GetUrlHelper("~/fakeUrl");

            var newGuid = Guid.NewGuid();
            url.For(new TestUrl() { Id = newGuid }).ShouldBe("/fakeUrl/" + newGuid);
        }

        [Test]
        public void UrlForInterfaceShouldReturnCorrectUrl1()
        {
            ActionFactory.Actions.Add(typeof(TestUrl), new ActionInfo() { });
            RouteTable.Routes.Add(typeof(TestUrl).FullName, new Route("fakeUrl/{Age}/edit", null));

            var url = MvcMockHelpers.GetUrlHelper("~/fakeUrl");
            url.For(new TestUrl()).ShouldBe("/fakeUrl/0/edit");
        }

        public class TestUrl 
        {
            public Guid Id { get; set; }
            
            [RouteParam]
            public int Age { get; set; }
        }
    }
}
