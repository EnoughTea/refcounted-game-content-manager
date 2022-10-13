using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

namespace RcContentManager
{
    /// <summary>
    ///     Provides a base for reference counting of the given item type.
    ///     Use provided events to implement actual logic.
    /// </summary>
    /// <remarks>Contains maximum of <see cref="int.MaxValue" /> real item instances.</remarks>
    /// <typeparam name="T">The type of the refcounted item.</typeparam>
    public interface IRefCounter<T>
    {
        /// <summary>Gets the different item instances count.</summary>
        int ItemCount { get; }

        /// <summary>Occurs after counter for a reference was decremented.</summary>
        /// <remarks>
        ///     Note that <see cref="Clear()" /> & <see cref="Clear(T)" /> calls do not raise this event,
        ///     they raise <see cref="Released" /> only.
        /// </remarks>
        public event Action<IRefCounter<T>, T>? Decremented;

        /// <summary>Occurs after a reference was retained for the first time.</summary>
        public event Action<IRefCounter<T>, T>? FirstTimeRetained;

        /// <summary>Occurs after counter for a reference was incremented.</summary>
        public event Action<IRefCounter<T>, T>? Incremented;

        /// <summary>Occurs after reference counter for a reference was set to zero and it was released.</summary>
        public event Action<IRefCounter<T>, T>? Released;

        /// <summary> Sets refcount to zero for all retained items and releases them. </summary>
        void Clear();

        /// <summary> Sets refcount to zero for the specified item and releases it. </summary>
        void Clear(T item);

        /// <summary> Gets the number of references for the tracked item. </summary>
        long Count(T item);

        /// <summary>
        ///     Decrements the refcount for the given item. Releases the item if its refcount reaches zero.
        /// </summary>
        /// <param name="item">The item to release.</param>
        void Release(T item);

        /// <summary> Increments the refcount for the given item. </summary>
        /// <param name="item">The item that should be retained.</param>
        void Retain(T item);

        /// <summary> Returns true if the specified item is refcounted; false otherwise. </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>Returns true if the specified item is refcounted; false otherwise.</returns>
        bool Tracked(T item);
    }

    /// <summary>
    ///     Provides a base for reference counting of a given item type.
    ///     Calls appropriate events when it is time for an item to be released.
    ///     Thread-safe. Contains maximum of <see cref="Int32.MaxValue" /> items.
    /// </summary>
    /// <typeparam name="T">The type of the refcounted item.</typeparam>
    public class RefCounter<T> : IRefCounter<T>
    {
        private static readonly SimplePool<ItemRefCount> ItemRefCountPool =
            new SimplePool<ItemRefCount>(() => new ItemRefCount(1), itemRefs => itemRefs.SetTo(1));

        /// <summary> Holds retained items and their refcounts. </summary>
        private readonly ConcurrentDictionary<T, ItemRefCount> _refs =
            new ConcurrentDictionary<T, ItemRefCount>();

        /// <inheritdoc />
        public event Action<IRefCounter<T>, T>? Decremented;

        /// <inheritdoc />
        public event Action<IRefCounter<T>, T>? FirstTimeRetained;

        /// <inheritdoc />
        public event Action<IRefCounter<T>, T>? Incremented;

        /// <inheritdoc />
        public event Action<IRefCounter<T>, T>? Released;

        /// <inheritdoc />
        public int ItemCount => _refs.Count;

        /// <summary> Gets the ref count for the tracked item. </summary>
        public long Count(T item) => GetItemRefs(item)?.Count ?? 0;

        /// <inheritdoc />
        public void Clear()
        {
            var itemsToClear = _refs.Keys.ToArray();
            foreach (var item in itemsToClear)
                if (_refs.TryRemove(item, out var refCount)) {
                    ItemRefCountPool.Return(refCount);
                    Released?.Invoke(this, item);
                }
        }

        /// <inheritdoc />
        public void Clear(T item)
        {
            if (_refs.TryRemove(item, out var refCount)) {
                ItemRefCountPool.Return(refCount);
                Released?.Invoke(this, item);
            }
        }

        /// <inheritdoc />
        public void Release(T item)
        {
            if (item is null || !_refs.TryGetValue(item, out var refCount)) return;

            long newRefCount = refCount.Dec();
            if (newRefCount >= 0) Decremented?.Invoke(this, item);
            if (newRefCount == 0 && _refs.TryRemove(item, out var lastRefCount)) {
                ItemRefCountPool.Return(lastRefCount);
                Released?.Invoke(this, item);
            }
        }

        /// <inheritdoc />
        public void Retain(T item)
        {
            if (item is null) return;

            _refs.AddOrUpdate(
                item,
                _ => {
                    var newRefCount = ItemRefCountPool.Get();
                    return newRefCount;
                },
                (_, existingRefCount) => {
                    existingRefCount.Inc();
                    return existingRefCount;
                });
            if (Count(item) == 1) FirstTimeRetained?.Invoke(this, item);

            Incremented?.Invoke(this, item);
        }

        /// <inheritdoc />
        public bool Tracked(T item) => GetItemRefs(item) != null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ItemRefCount? GetItemRefs(T item) =>
            !(item is null) && _refs.TryGetValue(item, out var refCount) ? refCount : null;

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() => $"{_refs.Count} refs";

        /// <summary> Refcount value needs to be a reference type. </summary>
        private class ItemRefCount
        {
            private long _count;

            /// <summary>
            ///     Initializes a new instance of the <see cref="RefCounter{TItem}.ItemRefCount" /> class.
            /// </summary>
            /// <param name="count">Initial refcount.</param>
            public ItemRefCount(long count = 0) => _count = count;

            /// <summary> Returns current refcount. </summary>
            public long Count => Interlocked.Read(ref _count);

            /// <summary> Decrements refcount by 1. </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public long Dec() => Interlocked.Decrement(ref _count);

            /// <summary> Increments refcount by 1. </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public long Inc() => Interlocked.Increment(ref _count);

            /// <summary> Sets refcount to the given value. </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public long SetTo(long count) => Interlocked.Exchange(ref _count, count);

            /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
            /// <returns> A <see cref="string" /> that represents this instance. </returns>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override string ToString() => Count.ToString(CultureInfo.InvariantCulture);
        }
    }
}