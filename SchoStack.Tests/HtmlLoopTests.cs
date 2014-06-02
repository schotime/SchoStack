using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using NUnit.Framework;
using Promaster.Tests;
using SchoStack.Tests.HtmlConventions;
using SchoStack.Web.Conventions;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.Html;
using Shouldly;

namespace SchoStack.Tests
{
    public class HtmlLoopTests
    {
        public HtmlLoopTests()
        {
            HtmlConventionFactory.Add(new DefaultHtmlConventions());
        }

        [Test]
        public void Test1()
        {
            var model = new LoopViewModel()
            {
                Address = "My Address",
                LoopItems = new List<LoopViewModel.LoopItem>()
                {
                    new LoopViewModel.LoopItem()
                    {
                        Name = "Name1",
                        NestedLoopItems = new List<LoopViewModel.LoopItem.NestedLoopItem>()
                        {
                            new LoopViewModel.LoopItem.NestedLoopItem() {Age = 1},
                            new LoopViewModel.LoopItem.NestedLoopItem() {Age = 2}
                        }
                    },
                    new LoopViewModel.LoopItem()
                    {
                        Name = "Name2",
                        NestedLoopItems = new List<LoopViewModel.LoopItem.NestedLoopItem>()
                        {
                            new LoopViewModel.LoopItem.NestedLoopItem() {Age = 3},
                            new LoopViewModel.LoopItem.NestedLoopItem() {Age = 4}
                        }
                    }
                }
            };

            var helper = MvcMockHelpers.GetHtmlHelper(model);

            var i = 0;
            foreach (var loop in helper.Loop(x=>x.LoopItems))
            {
                loop.Input(x=>x.Name).Attr("name").ShouldBe("LoopItems[" + loop.Info.Index + "].Name");
                
                loop.Info.Index.ShouldBe(i);
                loop.Info.IsFirst.ShouldBe(i == 0);
                loop.Info.IsLast.ShouldBe(i == 1);
                loop.Info.IsEven.ShouldBe(i % 2 == 0);
                loop.Info.IsOdd.ShouldBe(i % 2 != 0);

                var j = 0;
                foreach (var nestedLoop in loop.Loop(x=>x.NestedLoopItems))
                {
                    nestedLoop.Input(x => x.Age).Attr("name").ShouldBe("LoopItems[" + loop.Info.Index + "].NestedLoopItems[" + nestedLoop.Info.Index + "].Age");

                    nestedLoop.Info.Index.ShouldBe(j);
                    nestedLoop.Info.IsFirst.ShouldBe(j == 0);
                    nestedLoop.Info.IsLast.ShouldBe(j == 1);
                    nestedLoop.Info.IsEven.ShouldBe(j % 2 == 0);
                    nestedLoop.Info.IsOdd.ShouldBe(j % 2 != 0);

                    j++;
                }

                i++;
            }
        }

        [Test]
        public void Test2()
        {
            var model = new LoopViewModel()
            {
                Address = "My Address"
            };
            
            for (int j = 0; j < 1000; j++)
            {
                model.LoopItems.Add(new LoopViewModel.LoopItem()
                {
                    Name = "MyName:" + j
                });
            }
          
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var sw = Stopwatch.StartNew();
            foreach (var loop in helper.Loop(x => x.LoopItems))
            {
                loop.Input(x => x.Name);
                loop.Display(x => x.Name);
          
                foreach (var loopItem in loop.Loop(x => x.NestedLoopItems))
                {
                    loopItem.Input(x => x.Age);
                }
            }
            sw.Stop();
            Console.WriteLine(sw.ElapsedMilliseconds);
        }

        [Test]
        public void Test3()
        {
            var model = new LoopViewModel()
            {
                Address = "My Address",
                LoopItems = new List<LoopViewModel.LoopItem>()
                {
                    new LoopViewModel.LoopItem()
                    {
                        Name = "MyName"
                    }
                }
            };
            var helper = MvcMockHelpers.GetHtmlHelper(model);

            Assert.AreEqual("Address", helper.IdFor(x => x.Address));
            Assert.AreEqual("Address", helper.NameFor(x => x.Address));

            foreach (var loop in helper.Loop(x => x.LoopItems))
            {
                Assert.AreEqual("LoopItems_0__Name", loop.IdFor(x=>x.Name));
                Assert.AreEqual("LoopItems[0].Name", loop.NameFor(x=>x.Name));
            }
        }
    }

    public class LoopViewModel
    {
        public LoopViewModel()
        {
            LoopItems = new List<LoopItem>();
        }

        public string Address { get; set; }
        public List<LoopItem> LoopItems { get; set; }

        public class LoopItem
        {
            public LoopItem()
            {
                NestedLoopItems = new List<NestedLoopItem>()
                {
                    new NestedLoopItem()
                    {
                        Age = 3,
                    }
                };
            }

            public List<NestedLoopItem> NestedLoopItems { get; set; }
            public string Name { get; set; }

            public class NestedLoopItem
            {
                public NestedLoopItem()
                {
                    NestedLoopItems = new List<NestedNestedItem>()
                    {
                        new NestedNestedItem()
                        {
                            Desc = "George"
                        }
                    };
                }

                public int Age { get; set; }

                public List<NestedNestedItem> NestedLoopItems { get; set; }
            }

            public class NestedNestedItem
            {
                public string Desc { get; set; }
            }
        }
    }
}
