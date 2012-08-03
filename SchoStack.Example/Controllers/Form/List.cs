using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using FluentValidation;
using SchoStack.Web;

namespace SchoStack.Example.Controllers.Form
{
    [Route("/form/list")]
    public class List : ActionController
    {
        public ActionResult Get(FormListQueryModel query)
        {
            var vm = new FormListViewModel()
                     {
                         Todos = new List<FormListViewModel.ToDo>()
                                 {
                                     new FormListViewModel.ToDo() {
                                         Desc = "Description 1", 
                                         Done = false,
                                         Todo2s = new List<FormListViewModel.ToDo.Todo2>()
                                                  {
                                                      new FormListViewModel.ToDo.Todo2() { Nest = "Nest 1"},
                                                      new FormListViewModel.ToDo.Todo2() { Nest = "Nest 2"},
                                                  }
                                     },
                                     new FormListViewModel.ToDo() {Desc = "Description 2", Done = true},
                                 }
                     };
            return View(vm);
        }

        public ActionResult Post(FormListInputModel input)
        {
            var val = new FormListValidator();
            var result = val.Validate(input);


            if (!ModelState.IsValid)
                return Get(new FormListQueryModel());

            return RedirectToGet();
        }
    }

    public class FormListQueryModel{}
    public class FormListViewModel
    {
        public List<ToDo> Todos { get; set; }

        public class ToDo
        {
            public string Desc { get; set; } 
            public bool Done { get; set; }
            public List<Todo2> Todo2s { get; set; }

            public class Todo2
            {
                public string Nest { get; set; }
            }
        }
    }
    public class FormListInputModel
    {
        public List<ToDo> Todos { get; set; }

        public class ToDo
        {
            public string Desc { get; set; }
            public bool Done { get; set; }
        }
    }

    public class FormListValidator : AbstractValidator<FormListInputModel>
    {
        public FormListValidator()
        {
            RuleFor(x => x.Todos)
                .SetCollectionValidator(new ToDoValidator());
        }
    }

    public class ToDoValidator : AbstractValidator<FormListInputModel.ToDo>
    {
        public ToDoValidator()
        {
             RuleFor(x => x.Desc)
                .NotEmpty();
        }
    }
}