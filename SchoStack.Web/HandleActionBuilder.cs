using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace SchoStack.Web
{
    public class HandleActionBuilder
    {
        private readonly IInvoker _invoker;
        private Func<ControllerContext, ActionResult> _successResult;
        private Func<ControllerContext, ActionResult> _errorResult;

        public HandleActionBuilder(IInvoker invoker)
        {
            _invoker = invoker;
        }

        public HandleActionBuilder OnSuccess(Func<ActionResult> result)
        {
            _successResult = _ => result();
            return this;
        }

        public HandleActionBuilder OnSuccess(Func<ControllerContext, ActionResult> result)
        {
            _successResult = result;
            return this;
        }

        public HandleActionBuilder OnModelError(Func<ActionResult> result)
        {
            _errorResult = _ => result();
            return this;
        }

        public HandleActionBuilder OnModelError(Func<ControllerContext, ActionResult> result)
        {
            _errorResult = result;
            return this;
        }

        public HandleActionBuilder<HandleActionBuilder, TRet> Returning<TRet>()
        {
            return new HandleActionBuilder<HandleActionBuilder, TRet>(null, _invoker, _successResult, _errorResult);
        }
    }

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

        public HandleActionBuilder<T> OnModelError(Func<ActionResult> result)
        {
            _errorResult = _ => result();
            return this;
        }

        public HandleActionBuilder<T> OnModelError(Func<ControllerContext, ActionResult> result)
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
                var result = ExecuteResult(new SchoStackControllerContext(context));
                if (result != null)
                {
                    result.ExecuteResult(context);
                }
            }

            public ActionResult ExecuteResult(IControllerContext context)
            {
                if (!context.IsValidModel && _builder._errorResult != null)
                {
                    return _builder._errorResult(context.Context);
                }

                _builder._invoker.Execute(_builder._inputModel);

                if (_builder._successResult != null)
                {
                    return _builder._successResult(context.Context);
                }

                return null;
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

        public HandleActionBuilder<T, TRet> OnModelError(Func<ActionResult> result)
        {
            _errorResult = _ => result();
            return this;
        }

        public HandleActionBuilder<T, TRet> OnModelError(Func<ControllerContext, ActionResult> result)
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
                var result = ExecuteResult(new SchoStackControllerContext(context));
                if (result != null)
                {
                    result.ExecuteResult(context);
                }
            }

            public ActionResult ExecuteResult(IControllerContext context)
            {
                if (!context.IsValidModel && _builder._errorResult != null)
                {
                    return _builder._errorResult(context.Context);
                }

                var result = _builder._inputModel == null ? _builder._invoker.Execute<TRet>() : _builder._invoker.Execute<TRet>(_builder._inputModel);
                var conditionResult = _builder._conditionResults.FirstOrDefault(x => x.Condition(result, context.Context));
                if (conditionResult != null)
                {
                    return conditionResult.Result(result, context.Context);
                }
                if (_builder._successResult != null)
                {
                    return _builder._successResult(result, context.Context);
                }

                return null;
            }
        }
    }

    public class ConditionResult<TRet>
    {
        public Func<TRet, ControllerContext, bool> Condition { get; set; }
        public Func<TRet, ControllerContext, ActionResult> Result { get; set; }
    }
}
