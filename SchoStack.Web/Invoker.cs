using System;
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
            return ExecuteImpl((dynamic)inputModel, default(TOutput));
        }

        private TOutput ExecuteImpl<T, TOutput>(T input, TOutput _)
        {
            var thandler = (IHandler<T, TOutput>)_resolver(typeof(IHandler<T, TOutput>));
            EnsureHandlerFound(thandler, typeof(T));
            return thandler.Handle((input));
        }

        public TOutput Execute<TOutput>()
        {
            var type = typeof(IHandler<>).MakeGenericType(typeof(TOutput));
            var thandler = (IHandler<TOutput>)_resolver(type);
            EnsureHandlerFound(thandler, type);
            var result = thandler.Handle();
            return result;
        }

        public void Execute(object inputModel)
        {
            ExecuteImpl((dynamic)inputModel);
        }

        private void ExecuteImpl<T>(T input)
        {
            var thandler = (ICommandHandler<T>)_resolver(typeof(ICommandHandler<T>));
            EnsureHandlerFound(thandler, typeof(T));
            thandler.Handle((input));
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
    }
}