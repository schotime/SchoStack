using System;
using System.Collections.Generic;
using System.Web;

namespace SchoStack.Web
{
    public interface IChildMessaging
    {
        void Publish<T>(T obj);
        T Subscribe<T>(Func<T> func);
        T Subscribe<T>();
    }

    public class ChildMessaging : IChildMessaging
    {
        private IChildMessagingStorage _storage;

        public ChildMessaging() : this(new ChildMessagingStorage()) { }

        public ChildMessaging(IChildMessagingStorage storage)
        {
            _storage = storage;
        }

        private Dictionary<Type, object> Dict
        {
            get { return _storage.GetStorage(); }
        }

        public void Publish<T>(T obj)
        {
            Dict.Add(typeof(T), obj);
        }

        public T Subscribe<T>(Func<T> func)
        {
            T obj = default(T);
            if (Dict.ContainsKey(typeof(T)))
            {
                obj = (T)Dict[typeof(T)];
            }

            if (obj == null)
                return func();

            return obj;
        }

        public T Subscribe<T>()
        {
            return Subscribe(() => default(T));
        }
    }

    public interface IChildMessagingStorage
    {
        Dictionary<Type, object> GetStorage();
    }

    public class ChildMessagingStorage : IChildMessagingStorage
    {
        const string MESSAGES = "__messages";

        public Dictionary<Type, object> GetStorage()
        {
            if (!HttpContext.Current.Items.Contains(MESSAGES))
                HttpContext.Current.Items[MESSAGES] = new Dictionary<Type, object>();

            return (Dictionary<Type, object>)HttpContext.Current.Items[MESSAGES];
        }
    }


}
