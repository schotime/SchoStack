using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SchoStack.Web
{
    public class HandleActionBuilder<T>
    {
        private readonly T _inputModel;
        private readonly IInvoker _invoker;
        private Func<ControllerContext, ActionResult> _successResult;
        private Func<ControllerContext, ActionResult> _errorResult;

        public HandleActionBuilder(T inputModel, IInvoker invoker)
        {
            _inputModel = inputModel;
            _invoker = invoker;
        }

        public HandleActionBuilder<T, TRet> Returning<TRet>()
        {
            return new HandleActionBuilder<T, TRet>(_inputModel, _invoker, _successResult, _errorResult);
        }

        public HandleActionBuilder<T> OnSuccess(Func<ActionResult> result)
        {
            _successResult = _ => result();
            return this;
        }

        public HandleActionBuilder<T> OnSuccess(Func<ControllerContext, ActionResult> result)
        {
            _successResult = result;
            return this;
        }

        public HandleActionBuilder<T> OnError(Func<ActionResult> result)
        {
            _errorResult = _ => result();
            return this;
        }

        public HandleActionBuilder<T> OnError(Func<ControllerContext, ActionResult> result)
        {
            _errorResult = result;
            return this;
        }

        public static implicit operator HandleActionResult(HandleActionBuilder<T> obj)
        {
            return new HandleActionResult(obj);
        }

        public class HandleActionResult : ActionResult
        {
            private readonly HandleActionBuilder<T> _builder;

            public HandleActionResult(HandleActionBuilder<T> builder)
            {
                _builder = builder;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                if (!context.Controller.ViewData.ModelState.IsValid && _builder._errorResult != null)
                {
                    _builder._errorResult(context).ExecuteResult(context);
                    return;
                }

                _builder._invoker.Execute(_builder._inputModel);

                if (_builder._successResult != null)
                    _builder._successResult(context).ExecuteResult(context);
            }
        }
    }
    
    public class HandleActionBuilder<T, TRet>
    {
        private readonly T _inputModel;
        private readonly IInvoker _invoker;
        private readonly List<ConditionResult<TRet>> _conditionResults = new List<ConditionResult<TRet>>();
        private Func<TRet, ControllerContext, ActionResult> _successResult;
        private Func<ControllerContext, ActionResult> _errorResult;

        public HandleActionBuilder(T inputModel, IInvoker invoker, Func<ControllerContext, ActionResult> successResult, Func<ControllerContext, ActionResult> errorResult)
        {
            _inputModel = inputModel;
            _invoker = invoker;
            _successResult = (_, x) => successResult(x);
            _errorResult = errorResult;
        }

        public HandleActionBuilder<T, TRet> On(Func<TRet, bool> condition, Func<TRet, ActionResult> result)
        {
            _conditionResults.Add(new ConditionResult<TRet>
            {
                Condition = (x, _) => condition(x),
                Result = (x, _) => result(x)
            });
            return this;
        }

        public HandleActionBuilder<T, TRet> On(Func<TRet, ControllerContext, bool> condition, Func<TRet, ControllerContext, ActionResult> result)
        {
            _conditionResults.Add(new ConditionResult<TRet>
            {
                Condition = condition,
                Result = result
            });
            return this;
        }

        public HandleActionBuilder<T, TRet> OnSuccess(Func<TRet, ActionResult> result)
        {
            _successResult = (x, cc) => result(x);
            return this;
        }

        public HandleActionBuilder<T, TRet> OnSuccess(Func<TRet, ControllerContext, ActionResult> result)
        {
            _successResult = result;
            return this;
        }

        public HandleActionBuilder<T, TRet> OnError(Func<ActionResult> result)
        {
            _errorResult = _ => result();
            return this;
        }

        public HandleActionBuilder<T, TRet> OnError(Func<ControllerContext, ActionResult> result)
        {
            _errorResult = result;
            return this;
        }

        public static implicit operator HandleActionResult(HandleActionBuilder<T, TRet> obj)
        {
            return new HandleActionResult(obj);
        }

        public class HandleActionResult : ActionResult
        {
            private readonly HandleActionBuilder<T, TRet> _builder;

            public HandleActionResult(HandleActionBuilder<T, TRet> builder)
            {
                _builder = builder;
            }

            public override void ExecuteResult(ControllerContext context)
            {
                if (!context.Controller.ViewData.ModelState.IsValid && _builder._errorResult != null)
                {
                    _builder._errorResult(context).ExecuteResult(context);
                    return;
                }

                var result = _builder._invoker.Execute<TRet>(_builder._inputModel);
                var conditionResult = _builder._conditionResults.FirstOrDefault(x => x.Condition(result, context));
                if (conditionResult != null)
                {
                    conditionResult.Result(result, context).ExecuteResult(context);
                }
                else if (_builder._successResult != null)
                {
                    _builder._successResult(result, context).ExecuteResult(context);
                }
            }
        }
    }

    public class ConditionResult<TRet>
    {
        public Func<TRet, ControllerContext, bool> Condition { get; set; }
        public Func<TRet, ControllerContext, ActionResult> Result { get; set; }
    }
}