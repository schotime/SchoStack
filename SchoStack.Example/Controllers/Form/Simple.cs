using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FluentValidation;
using SchoStack.Web;

namespace SchoStack.Example.Controllers.Form
{
    [Route("/form/simple")]
    public class Simple : ActionController
    {
        public ActionResult Get(FormSimpleQueryModel query)
        {
            var vm = new FormSimpleViewModel()
                     {
                         Email = "email@email.com",
                         Multi = new MultiSelectList(new List<SelectListItem>() { 
                             new SelectListItem(){ Text = "One", Value = "1"},
                             new SelectListItem(){ Text = "Two", Value = "2"},
                         }, new[] { "1" }), 
                         Nested =  new FormSimpleName()
                         {
                             Name = "Test"
                         }
                     };
            return View(vm);
        }

        public ActionResult Post(FormSimpleInputModel input)
        {
            if (!ModelState.IsValid)
                return Get(new FormSimpleQueryModel());

            return RedirectToGet();
        }
    }

    public class FormSimpleQueryModel{}
    public class FormSimpleViewModel
    {
        public string Email { get; set; }
        
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public MultiSelectList Multi { get; set; }

        public DateTime? DateTime { get; set; }

        public FormSimpleName Nested { get; set; }
    }

    public class FormSimpleName
    {
        public string Name { get; set; }
    }

    public class FormSimpleInputModel
    {
        public string Email { get; set; }
        public string Password { get; set; }

        public DateTime? DateTime { get; set; }

        public FormSimpleName Nested { get; set; }
    }

    public class FormSimpleValidator : AbstractValidator<FormSimpleInputModel>
    {
        public FormSimpleValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty();

            RuleFor(x => x.Password)
                .NotEmpty();

            RuleFor(x => x.Nested.Name)
                .NotEmpty();
        }
    }
}