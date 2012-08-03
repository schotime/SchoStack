using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace SchoStack.Web
{
    public class Invoker : IInvoker
    {
        private readonly Func<Type, object> _resolver;

        public Invoker(Func<Type, object> resolver)
        {
            _resolver = resolver;
        }

        public TOutput Execute<TOutput>(object inputModel)
        {
            var type = typeof(IHandler<,>).MakeGenericType(inputModel.GetType(), typeof(TOutput));
            var thandler = _resolver(type);
            EnsureHandlerFound(thandler, type);
            var methodName = GetMethodName(x => x.Handle(null));
            var method = thandler.GetType().GetMethod(methodName);
            return (TOutput) method.Invoke(thandler, new[] {inputModel});
        }

        public TOutput Execute <TOutput>()
        {
            var type = typeof(IHandler<>).MakeGenericType(typeof(TOutput));
            var thandler = (IHandler<TOutput>)_resolver(type);
            EnsureHandlerFound(thandler, type);
            var result = thandler.Handle();
            return result;
        }

        public void Execute(object inputModel)
        {
            var type = typeof(ICommandHandler<>).MakeGenericType(inputModel.GetType());
            var thandler = _resolver(type);
            EnsureHandlerFound(thandler, type);
            var methodName = GetMethodName(x => x.Handle(null));
            var method = thandler.GetType().GetMethod(methodName);
            method.Invoke(thandler, new[] { inputModel });
        }

        private void EnsureHandlerFound(object handler, Type type)
        {
            if (handler == null)
                throw new Exception("Could not find handler with type: " + FixGenerics(type.ToString()));
        }

        private static readonly Regex GenericRegex = new Regex(@"\`[0-9]+\[([^\s^:^;^""]+)\]", RegexOptions.Compiled);
        private static string FixGenerics(string json)
        {
            var i = 0;
            while (GenericRegex.IsMatch(json) && i < 10)
            {
                json = GenericRegex.Replace(json, @"<$1>");
                i++;
            }
            return json;
        }

        private string GetMethodName(Expression<Action<InvokerHelper>> method)
        {
            return ((MethodCallExpression) method.Body).Method.Name;
        }

        private class InvokerHelper : IHandler<object, object>
        {
            public object Handle(object model)
            {
                return new object();
            }
        }
    }
}