using System;
using System.Collections.Concurrent;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;

namespace MonoCustomContentManagers {
    /// <summary> Provides simple reference counting for given item type. Calls event handler when
    ///  it is time for an item to be released. </summary>
    /// <remarks>Class is "thread-safe" as in it maintains internal state consistently in individual operations.
    /// You still should lock when you need logical consistency maintained across multiple operations in a sequence.
    /// </remarks>
    /// <typeparam name="TItem">The type of the refcounted item.</typeparam>
    internal class RefCounter<TItem> {
        /// <summary> Occurs when an item was retained for the first time. </summary>
        public event Action<TItem> FirstTimeRetained;

        /// <summary> Occurs when item refcount was incremented. </summary>
        public event Action<TItem> Incremented;

        /// <summary> Occurs when item refcount was decremented. </summary>
        public event Action<TItem> Decremented;

        /// <summary> Occurs when an item was released. </summary>
        public event Action<TItem> Released;

        /// <summary> Holds retained items and their refcounts. </summary>
        private readonly ConcurrentDictionary<TItem, ItemRefCount> _refs =
            new ConcurrentDictionary<TItem, ItemRefCount>();

        private readonly object _locker = new object();

        /// <summary> Returns true if the specified item is refcounted; false otherwise. </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>Returns true if the specified item is refcounted; false otherwise.</returns>
        public bool Tracked(TItem item) {
            if (item == null) return false;

            ItemRefCount itemRef;
            return _refs.TryGetValue(item, out itemRef);
        }

        /// <summary> Increments the refcount for the given item. </summary>
        /// <param name="item">The item that should be retained.</param>
        public void Retain(TItem item) {
            Contract.Requires(item != null);

            _refs.AddOrUpdate(item,
                _ => {
                    var freshRef = new ItemRefCount(1);
                    Volatile.Read(ref FirstTimeRetained)?.Invoke(item);
                    return freshRef;
                },

                (_, existingRef) => {
                    existingRef.Inc();
                    return existingRef;
                });
            Volatile.Read(ref Incremented)?.Invoke(item);
        }

        /// <summary> Decrements the refcount for the given item. </summary>
        /// <param name="item">The item to release.</param>
        public void Release(TItem item) {
            Contract.Requires(item != null);

            lock (_locker) {
                ItemRefCount itemRef;
                if (_refs.TryGetValue(item, out itemRef) && (itemRef.GetCount() > 0)) {
                    itemRef.Dec();
                    Volatile.Read(ref Decremented)?.Invoke(item);
                    if (itemRef.IsZero() && _refs.TryRemove(item, out itemRef)) {
                        Volatile.Read(ref Released)?.Invoke(item);
                    }
                } else {
                    // Retain() was never called, so assume there is only one reference, which is now calling Release().
                    Volatile.Read(ref Released)?.Invoke(item);
                }
            }
        }

        /// <summary> Sets refcount to zero for all retained items and releases them. </summary>
        public void Clear() {
            foreach (var refKvp in _refs) {
                ItemRefCount itemRef;
                if (_refs.TryRemove(refKvp.Key, out itemRef)) {
                    itemRef.SetToZero();
                    Volatile.Read(ref Decremented)?.Invoke(refKvp.Key);
                    Volatile.Read(ref Released)?.Invoke(refKvp.Key);
                }
            }
        }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString() {
            return _refs.Count.ToString(CultureInfo.InvariantCulture) + " refcounted items";
        }


        /// <summary> Refcount value needs to be a reference type. </summary>
        /// <remarks> All methods are atomic operations. </remarks>
        private class ItemRefCount {
            /// <summary>
            /// Initializes a new instance of the <see cref="RefCounter{TItem}.ItemRefCount" /> class.
            /// </summary>
            /// <param name="count">Initial refcount.</param>
            public ItemRefCount(int count = 0) {
                Interlocked.Exchange(ref _count, count);
            }

            /// <summary> Current refcount. </summary>
            private int _count;

            /// <summary> Returns current refcount, IO fenced in 'down' direction. </summary>
            public int GetCount() {
                return Volatile.Read(ref _count);
            }

            /// <summary> Increments refcount by 1. </summary>
            public void Inc() {
                Interlocked.Increment(ref _count);
            }

            /// <summary> Decrements refcount by 1. </summary>
            public void Dec() {
                Interlocked.Decrement(ref _count);
            }

            /// <summary> Returns true if refcount is 0; false otherwise. </summary>
            public bool IsZero() {
                return Volatile.Read(ref _count) == 0;
            }

            /// <summary> Sets refcount to 0. </summary>
            public void SetToZero() {
                Interlocked.Exchange(ref _count, 0);
            }

            /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
            /// <returns> A <see cref="string" /> that represents this instance. </returns>
            public override string ToString() {
                return GetCount().ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}
