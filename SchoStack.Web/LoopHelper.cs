using System;
using System.Collections.Generic;
using System.Web;

namespace SchoStack.Web.Html
{
    public static class Loop
    {
        private static readonly object _itemsKey = new object();

        public static IEnumerable<T> Track<T>(this IEnumerator<T> enumerator)
        {
            var tracker = new LoopState<T>(CurrentLoop, enumerator);
            CurrentLoop = tracker;
            return tracker;
        }

        public static IEnumerable<T> Track<T>(this IEnumerable<T> enumerable)
        {
            return Track(enumerable.GetEnumerator());
        }

        public static bool Even
        {
            get
            {
                var loop = EnsureCurrentLoop(CurrentLoop);
                return loop.Even;
            }
        }

        public static bool Odd
        {
            get
            {
                var loop = EnsureCurrentLoop(CurrentLoop);
                return loop.Odd;
            }
        }

        public static int Index
        {
            get
            {
                var loop = EnsureCurrentLoop(CurrentLoop);
                return loop.Index;
            }
        }

        public static bool First
        {
            get
            {
                var loop = EnsureCurrentLoop(CurrentLoop);
                return loop.First;
            }
        }

        public static bool Last
        {
            get
            {
                var loop = EnsureCurrentLoop(CurrentLoop);
                return loop.Last;
            }
        }

        public static ILoopState OuterLoop
        {
            get
            {
                var loop = CurrentLoop;
                return EnsureCurrentLoop(loop).OuterLoop;
            }
        }

        private static ILoopState CurrentLoop
        {
            get
            {
                return HttpContext.Current.Items[_itemsKey] as ILoopState;
            }
            set
            {
                HttpContext.Current.Items[_itemsKey] = value;
            }
        }

        private static void End()
        {
            var loop = CurrentLoop;
            if (loop != null)
            {
                CurrentLoop = CurrentLoop.OuterLoop;
            }
        }

        private static ILoopState EnsureCurrentLoop(ILoopState loop)
        {
            if (loop == null)
            {
                throw new InvalidOperationException("No loop being tracked.");
            }
            return loop;
        }

        public interface ILoopState
        {
            ILoopState OuterLoop
            {
                get;
            }

            bool First
            {
                get;
            }

            bool Last
            {
                get;
            }

            bool Even
            {
                get;
            }

            bool Odd
            {
                get;
            }

            int Index
            {
                get;
            }
        }

        private class LoopState<T> : ILoopState, IEnumerable<T>, IEnumerator<T>
        {
            private readonly IEnumerator<T> _innerLoop;
            private T _current;
            private int _index = -1;

            public LoopState(ILoopState outerLoop, IEnumerator<T> innerLoop)
            {
                if (innerLoop == null)
                {
                    throw new ArgumentNullException("innerLoop");
                }
                OuterLoop = outerLoop;
                _innerLoop = innerLoop;
                Init();
            }

            public ILoopState OuterLoop
            {
                get;
                private set;
            }

            public int Index
            {
                get
                {
                    return _index;
                }
            }


            public bool First
            {
                get
                {
                    return Index == 0;
                }
            }

            public bool Last
            {
                get;
                private set;
            }

            public bool Even
            {
                get
                {
                    return (Index & 1) == 0;
                }
            }

            public bool Odd
            {
                get
                {
                    return (Index & 1) == 1;
                }
            }

            public T Current
            {
                get
                {

                    return _current;
                }
            }

            public void Dispose()
            {
                Loop.End();
            }

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return Current;
                }
            }

            public bool MoveNext()
            {
                _index++;
                if (Last)
                {
                    return false;
                }
                else
                {
                    FetchNext();
                }
                return true;
            }

            private void FetchNext()
            {
                _current = (_index == -1) ? default(T) : _innerLoop.Current;
                Last = !_innerLoop.MoveNext();
            }

            public void Reset()
            {
                _innerLoop.Reset();
                Init();
            }

            public IEnumerator<T> GetEnumerator()
            {
                return this;
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return this;
            }

            private void Init()
            {
                _index = -1;
                Last = false;
                // Look ahead
                FetchNext();
            }
        }
    }
}