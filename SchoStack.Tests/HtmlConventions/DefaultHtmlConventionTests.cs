using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Web.Mvc;
using FubuCore.Reflection;
using NUnit.Framework;
using Promaster.Tests;
using SchoStack.Web.Conventions;
using SchoStack.Web.Conventions.Core;
using SchoStack.Web.Html;
using Shouldly;

namespace SchoStack.Tests.HtmlConventions
{
    public class DefaultHtmlConventionTests
    {
        public DefaultHtmlConventionTests()
        {
            HtmlConventionFactory.Add(new DefaultHtmlConventions());
            HtmlConventionFactory.Add(new DataAnnotationHtmlConventions());
        }

        [Test]
        public void DisplayOfPropertyShouldByDefaultUseSpanTag()
        {
            var model = new TestViewModel()
            {
                Name = "Test"
            };
            var tag = MvcMockHelpers.GetHtmlHelper(model).Display(x => x.Name);
            tag.TagName().ShouldBe("span");
            tag.Text().ShouldBe(model.Name);
        }

        [Test]
        public void LabelOfPropertyShouldByDefaultUseSpanTag()
        {
            var model = new TestViewModel();
            Expression<Func<TestViewModel, string>> expression = x => x.Name;
            var tag = MvcMockHelpers.GetHtmlHelper(model).Label(expression);
            var name = expression.GetMemberExpression(false).Member.Name;
            tag.TagName().ShouldBe("label");
            tag.Text().ShouldBe(name);
        }

        [Test]
        public void InputOfPropertyShouldBeTheAttemptedValueIfThereIsAModelError()
        {
            var model = new TestViewModel()
            {
                Name = "Test"
            };
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.SetError("Name", "testinvalid");

            var tag = helper.Input(x => x.Name);

            tag.Attr("value").ShouldBe("testinvalid");
        }

        [Test]
        public void InputOfPropertyShouldBeTheModelValueIfThereIsNoError()
        {
            var model = new TestViewModel()
            {
                Name = "Test"
            };
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.Name);
            tag.Attr("value").ShouldBe(model.Name);
        }

        [Test]
        public void InputOfPropertyShouldBeCheckboxIfPropertyIsABool()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.IsCorrect);
            tag.Attr("type").ShouldBe("checkbox");
        }

        [Test]
        public void InputOfBoolPropertyWhosValueIsTrueShouldGiveAttrValueOfTrue()
        {
            var model = new TestViewModel() {IsCorrect = true};
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.IsCorrect);
            tag.Attr("value").ShouldBe(true.ToString());
        }

        [Test]
        public void InputOfBoolPropertyWhosValueIsFalseShouldGiveAttrValueOfTrue()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.IsCorrect);
            tag.Attr("value").ShouldBe(true.ToString());
        }

        [Test]
        public void InputOfBoolPropertyWhosValueIsTrueShouldHaveCheckedTrue()
        {
            var model = new TestViewModel() { IsCorrect = true };
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.IsCorrect);
            tag.Attr("checked").ShouldBe(model.IsCorrect.ToString().ToLowerInvariant());
        }

        [Test]
        public void InputOfBoolPropertyWhosValueIsTrueShouldNotBeCheckedIfErrorOnModelAndNotChecked()
        {
            var model = new TestViewModel() { IsCorrect = true };
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.SetError("IsCorrect", "False");

            var tag = helper.Input(x => x.IsCorrect);
            tag.HasAttr("checked").ShouldBe(false);
        }

        [Test]
        public void InputOfBoolPropertyWhosValueIsFalseShouldBeCheckedIfErrorOnModelAndChecked()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.SetError("IsCorrect", "True,False");

            var tag = helper.Input(x => x.IsCorrect);
            tag.Attr("checked").ShouldBe("true");
        }

        [Test]
        public void InputOfBoolPropertyShouldGenerateHiddenInputWithValueOfFalse()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.IsCorrect);
            tag.Next.Attr("type").ShouldBe("hidden");
            tag.Next.Attr("value").ShouldBe(false.ToString());
        }

        [Test]
        public void InputOfBoolPropertyShouldGenerateHiddenInputWithTheSameName()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.IsCorrect);
            tag.Next.TagName().ShouldBe(tag.TagName());
        }

        [Test]
        public void InputOfListSelectItemListPropertyShouldGenerateSelectTag()
        {
            var model = new TestViewModel()
                        {
                            Dropdown = new List<SelectListItem>()
                        };
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.Dropdown);
            tag.TagName().ShouldBe("select");

        }

        [Test]
        public void InputOfListSelectItemListPropertyShouldGenerateSelectTagWithNestedOptionTags()
        {
            var model = new TestViewModel()
                        {
                            Dropdown = new List<SelectListItem>()
                                       {
                                           new SelectListItem() {Text = "Text1", Value = "Value1"},
                                           new SelectListItem() {Text = "Text2", Value = "Value2", Selected = true}
                                       }
                        };

            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.Dropdown);
            tag.Children.Count.ShouldBe(2);
            tag.Children.Each(x => x.TagName().ShouldBe("option"));
        }

        [Test]
        public void InputOfListSelectItemListPropertyShouldGenerateSelectTagWithSelectedOptionTag()
        {
            var model = new TestViewModel()
            {
                Dropdown = new List<SelectListItem>()
                                       {
                                           new SelectListItem() {Text = "Text1", Value = "Value1"},
                                           new SelectListItem() {Text = "Text2", Value = "Value2", Selected = true}
                                       }
            };

            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.Dropdown);
            tag.Children[0].HasAttr("selected").ShouldBe(false);
            tag.Children[1].HasAttr("selected").ShouldBe(true);
        }

        [Test]
        public void InputOfListSelectItemListPropertyShouldGenerateSelectTagWithTextInOption()
        {
            var model = new TestViewModel()
            {
                Dropdown = new List<SelectListItem>()
                                       {
                                           new SelectListItem() {Text = "Text1", Value = "Value1"},
                                           new SelectListItem() {Text = "Text2", Value = "Value2", Selected = true}
                                       }
            };

            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.Dropdown);
            tag.Children[0].Text().ShouldBe(model.Dropdown[0].Text);
            tag.Children[1].Text().ShouldBe(model.Dropdown[1].Text);
        }

        [Test]
        public void InputOfListSelectItemListPropertyShouldGenerateSelectTagWithValueInOption()
        {
            var model = new TestViewModel()
            {
                Dropdown = new List<SelectListItem>()
                                       {
                                           new SelectListItem() {Text = "Text1", Value = "Value1"},
                                           new SelectListItem() {Text = "Text2", Value = "Value2", Selected = true}
                                       }
            };

            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.Dropdown);
            tag.Children[0].Attr("value").ShouldBe(model.Dropdown[0].Value);
            tag.Children[1].Attr("value").ShouldBe(model.Dropdown[1].Value);
        }

        [Test]
        public void InputOfListSelectItemListPropertyShouldGenerateSelectTagWithClientSelectedOptionStillSelectedIfError()
        {
            var model = new TestViewModel()
            {
                Dropdown = new List<SelectListItem>()
                                       {
                                           new SelectListItem() {Text = "Text1", Value = "Value1"},
                                           new SelectListItem() {Text = "Text2", Value = "Value2", Selected = true}
                                       }
            };

            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.SetError("Dropdown", "Value1");

            var tag = helper.Input(x => x.Dropdown);
            tag.Children[0].HasAttr("selected").ShouldBe(true);
            tag.Children[1].HasAttr("selected").ShouldBe(false);
        }

        [Test]
        public void InputOfMultiSelectListPropertyShouldGenerateSelectTagWithMultipleAttribute()
        {
            var model = new TestViewModel()
            {
                MultiSelect = new MultiSelectList(new List<SelectListItem>()
                                       {
                                           new SelectListItem() {Text = "Text1", Value = "Value1", Selected = true},
                                           new SelectListItem() {Text = "Text2", Value = "Value2", Selected = true}
                                       })
            };

            var helper = MvcMockHelpers.GetHtmlHelper(model);
            var tag = helper.Input(x => x.MultiSelect);
            tag.HasAttr("multiple").ShouldBe(true);
            tag.Children.Each(x => x.HasAttr("selected").ShouldBe(true));
        }

        [Test]
        public void AllInputsWithErrorShouldHaveHtmlHelperValidationInputCssClassName()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);
            helper.ViewContext.SetError("Name", "doesntMatter");

            var tag = helper.Input(x => x.Name);
            var tag1 = helper.Input(x => x.Password);

            tag.HasClass(HtmlHelper.ValidationInputCssClassName).ShouldBe(true);
            tag1.HasClass(HtmlHelper.ValidationInputCssClassName).ShouldBe(false);
        }

        [Test]
        public void AllInputsShouldHaveIdAndName()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);

            var tag = helper.Input(x => x.Name);

            tag.Attr("name").ShouldBe("Name");
            tag.Attr("id").ShouldBe("Name");
        }

        [Test]
        public void AllLabelsShouldHaveIdWithLabelSuffix()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);

            var tag = helper.Label(x => x.Name);

            tag.Attr("id").ShouldBe("Name_Label");
        }

        [Test]
        public void AllDisplaysShouldHaveIdWithDisplaySuffix()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);

            var tag = helper.Display(x => x.Name);

            tag.Attr("id").ShouldBe("Name_Display");
        }

        [Test]
        public void AllLabelsWithDisplayNameAttributeShouldOutputTheNameSupplied()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);

            Expression<Func<TestViewModel, string>> expression = x => x.DisplayName;
            var tag = helper.Label(expression);

            var attribute = expression.GetMemberExpression(false).Member.GetAttribute<DisplayNameAttribute>();
            tag.Text().ShouldBe(attribute.DisplayName);
        }

        [Test]
        public void SubmitButtonShouldHaveNoIdTag()
        {
            var model = new TestViewModel();
            var helper = MvcMockHelpers.GetHtmlHelper(model);

            var tag = helper.Submit("test").AddClass("testclass");

            tag.ToString().ShouldBe("<input type=\"submit\" value=\"test\" class=\"testclass\" />");
        }

        [Test]
        public void DateTimeOffsetToString()
        {
            var model = new TestViewModel() { CreatedAtOffset = new DateTimeOffset(2000, 1, 1, 0, 0, 0, new TimeSpan(0, 1, 0, 0)) };
            var helper = MvcMockHelpers.GetHtmlHelper(model);

            var tag = helper.Display(x => x.CreatedAtOffset);

            tag.Text().ShouldBe("1/01/2000 12:00:00 AM +01:00");
        }
    }

}
