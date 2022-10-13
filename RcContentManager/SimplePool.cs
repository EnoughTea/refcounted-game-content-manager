using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace RcContentManager
{
    internal class SimplePool<T>
    {
        private readonly Func<T> _objectGenerator;
        private readonly Action<T> _objectReset;
        private readonly ConcurrentBag<T> _objects;

        public SimplePool(Func<T> objectGenerator, Action<T> objectReset)
        {
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException(nameof(objectGenerator));
            _objectReset     = objectReset ?? throw new ArgumentNullException(nameof(objectReset));
            _objects         = new ConcurrentBag<T>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get() => _objects.TryTake(out var item) ? item : _objectGenerator();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T item)
        {
            _objectReset(item);
            _objects.Add(item);
        }
    }
}