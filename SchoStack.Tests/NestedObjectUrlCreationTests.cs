using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using FubuCore;
using Moq;
using NUnit.Framework;
using Promaster.Tests;
using SchoStack.Web;
using SchoStack.Web.Url;

namespace SchoStack.Tests
{
    public class NestedObjectUrlCreationTests
    {
        [Test, RequiresSTA]
        public void SimpleUrlGeneration()
        {
            RouteTable.Routes.Clear();
            ActionFactory.Actions[typeof(NestedQueryModel)] = new ActionInfo();
            RouteTable.Routes.Add(typeof(NestedQueryModel).FullName, new Route("fakeUrl", null));

            var urlHelper = MvcMockHelpers.GetUrlHelper("~/fakeUrl");

            var httpContext = Mock.Get(urlHelper.RequestContext.HttpContext.Request);
            httpContext.Setup(x => x.QueryString).Returns(new NameValueCollection()
            {
                { "Age", "1" }
            });

            var url = urlHelper.For<NestedQueryModel>(x => x.Age = 2);
            Assert.AreEqual(url, "/fakeUrl?Age=2");
        }

        [Test, RequiresSTA]
        public void ComplexUrlGeneration()
        {
            RouteTable.Routes.Clear();
            ActionFactory.Actions[typeof(NestedQueryModel)] = new ActionInfo();
            RouteTable.Routes.Add(typeof(NestedQueryModel).FullName, new Route("fakeUrl", null));
            ModelBinders.Binders.Remove(typeof (DateTime?));
            ModelBinders.Binders.Add(typeof(DateTime?), new DateTimeBinder());

            var urlHelper = MvcMockHelpers.GetUrlHelper("~/fakeUrl");

            var httpContext = Mock.Get(urlHelper.RequestContext.HttpContext.Request);
            httpContext.Setup(x => x.QueryString).Returns(new NameValueCollection()
            {
                { "Age", "1" },
                { "NestedList[0].Name", "MyName" },
                { "NestedObj.Name", "MyName2" },
                { "Today", "7/12/2010 12:00:00" }
            });

            var url = urlHelper.For<NestedQueryModel>(x => x.Age = 2, x => x.NestedObj.Name = "Wow");
            Assert.AreEqual("/fakeUrl?Age=2&Today=7%2F12%2F2010%2012%3A00%3A00%20PM&NestedList%5B0%5D.Name=MyName&NestedObj.Name=Wow", url);
        }

        [Test, RequiresSTA]
        public void ComplexUrlGenerationWithEnum()
        {
            RouteTable.Routes.Clear();
            ActionFactory.Actions[typeof(NestedQueryModel)] = new ActionInfo();
            RouteTable.Routes.Add(typeof(NestedQueryModel).FullName, new Route("fakeUrl", null));
            
            var urlHelper = MvcMockHelpers.GetUrlHelper("~/fakeUrl");

            var httpContext = Mock.Get(urlHelper.RequestContext.HttpContext.Request);
            httpContext.Setup(x => x.QueryString).Returns(new NameValueCollection());

            var url = urlHelper.For<NestedQueryModel>(x => x.MyEnum = MyEnum.Two, x => x.NestedObj.MyEnum = MyEnum.Two);
            Assert.AreEqual("/fakeUrl?MyEnum=Two&NestedObj.MyEnum=Two", url);
        }

        [Test, RequiresSTA]
        public void AliasBinding()
        {
            RouteTable.Routes.Clear();
            ModelBinders.Binders.DefaultBinder = new AliasModelBinder();
            ActionFactory.Actions[typeof(AliasQueryModel)] = new ActionInfo();
            RouteTable.Routes.Add(typeof(AliasQueryModel).FullName, new Route("fakeUrl", null));

            var urlHelper = MvcMockHelpers.GetUrlHelper("~/fakeUrl");

            var httpContext = Mock.Get(urlHelper.RequestContext.HttpContext.Request);
            httpContext.Setup(x => x.QueryString).Returns(new NameValueCollection());

            var url = urlHelper.For(new AliasQueryModel { MyBigLongNameHere = "MyName"});
            Assert.AreEqual("/fakeUrl?N=MyName", url);
            ModelBinders.Binders.DefaultBinder = new DefaultModelBinder();
        }

        [Test, RequiresSTA]
        public void AliasBindingWithModifers()
        {
            RouteTable.Routes.Clear();
            ModelBinders.Binders.DefaultBinder = new AliasModelBinder();
            ActionFactory.Actions[typeof(AliasQueryModel)] = new ActionInfo();
            RouteTable.Routes.Add(typeof(AliasQueryModel).FullName, new Route("fakeUrl", null));

            var urlHelper = MvcMockHelpers.GetUrlHelper("~/fakeUrl");

            var httpContext = Mock.Get(urlHelper.RequestContext.HttpContext.Request);
            httpContext.Setup(x => x.QueryString).Returns(new NameValueCollection()
            {
                { "N", "MyNameOrig" },
                { "N2", "MyNameOrig2" },
            });

            var url = urlHelper.For<AliasQueryModel>(x => x.MyBigLongNameHere2 = "Mod");
            Assert.AreEqual("/fakeUrl?N=MyNameOrig&N2=Mod", url);
            ModelBinders.Binders.DefaultBinder = new DefaultModelBinder();
        }

        [Test, RequiresSTA]
        public void NestedAliases()
        {
            RouteTable.Routes.Clear();
            ModelBinders.Binders.DefaultBinder = new AliasModelBinder();
            ActionFactory.Actions[typeof(AliasQueryModel)] = new ActionInfo();
            RouteTable.Routes.Add(typeof(AliasQueryModel).FullName, new Route("fakeUrl", null));

            var urlHelper = MvcMockHelpers.GetUrlHelper("~/fakeUrl");

            var httpContext = Mock.Get(urlHelper.RequestContext.HttpContext.Request);
            httpContext.Setup(x => x.QueryString).Returns(new NameValueCollection()
            {
                { "NAQ.LNP2", "Binding" }
            });

            var url = urlHelper.For<AliasQueryModel>(x => x.NestedAliasQuery.LongerNestedProp = "WowWee");
            Assert.AreEqual("/fakeUrl?NAQ.LNP=WowWee&NAQ.LNP2=Binding", url);
            ModelBinders.Binders.DefaultBinder = new DefaultModelBinder();
        }
    }

    public class DateTimeBinder : IModelBinder
    {
        public object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            var value = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
            var date = DateTime.Parse(value.AttemptedValue, new CultureInfo("en-AU"));
            return date;
        }
    }

    public class AliasQueryModel
    {
        public AliasQueryModel()
        {
            NestedAliasQuery = new NestedAliasQueryModel();
        }

        [BindAlias("N")]
        public string MyBigLongNameHere { get; set; }
        [BindAlias("N2")]
        public string MyBigLongNameHere2 { get; set; }

        [BindAlias("NAQ")]
        public NestedAliasQueryModel NestedAliasQuery { get; set; }

        public class NestedAliasQueryModel
        {
            [BindAlias("LNP")]
            public string LongerNestedProp { get; set; }

            [BindAlias("LNP2")]
            public string LongerNestedProp2 { get; set; }
        }
    }

    public class NestedQueryModel
    {
        public NestedQueryModel()
        {
            NestedList = new List<NestedObj>();
            NestedObj = new NestedObj();
        }

        public int Age { get; set; }
        public DateTime? Today { get; set; }
        public List<NestedObj> NestedList { get; set; }
        public MyEnum MyEnum { get; set; }
        public NestedObj NestedObj { get; set; }
    }

    public enum MyEnum
    {
        One,
        Two
    }

    public class NestedObj
    {
        public string Name { get; set; }
        public MyEnum MyEnum { get; set; }
    }
}
