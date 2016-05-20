using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace MonoCustomContentManagers
{
    /// <summary> Provides simple reference counting for given item type. Calls event handler when
    ///  it is time for an item to be released. </summary>
    /// <remarks>Class is "thread-safe" as in it maintains internal state consistently in individual operations.
    /// You still should lock when you need logical consistency maintained across multiple operations in a sequence.
    /// </remarks>
    /// <typeparam name="TItem">The type of the refcounted item.</typeparam>
    internal class RefCounter<TItem>
    {
        public event Action<TItem> FirstTimeRetained;
        public event Action<TItem> Incremented;
        public event Action<TItem> Decremented;
        public event Action<TItem> Released;

        /// <summary> Refcount value needs to be a reference type. </summary>
        private class ItemRefCount
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="RefCounter{TItem}.ItemRefCount" /> class.
            /// </summary>
            /// <param name="count">Initial refcount.</param>
            public ItemRefCount(int count = 0)
            {
                Count = count;
            }

            /// <summary> Returns current refcount. </summary>
            public int Count { get; private set; }

            /// <summary> Increments refcount by 1. </summary>
            public void Inc()
            {
                Count++;
            }

            /// <summary> Decrements refcount by 1. </summary>
            public void Dec()
            {
                Count--;
            }

            /// <summary> Returns true if refcount is 0; false otherwise. </summary>
            public bool IsZero()
            {
                return Count == 0;
            }

            /// <summary> Sets refcount to 0. </summary>
            public void SetToZero()
            {
                Count = 0;
            }

            /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
            /// <returns> A <see cref="string" /> that represents this instance. </returns>
            public override string ToString()
            {
                return Count.ToString(CultureInfo.InvariantCulture);
            }
        }

        /// <summary> Holds retained items and their refcounts. </summary>
        private readonly ConcurrentDictionary<TItem, ItemRefCount> _refs =
            new ConcurrentDictionary<TItem, ItemRefCount>();

        /// <summary> Returns true if the specified item is refcounted; false otherwise. </summary>
        /// <param name="item">The item to check.</param>
        /// <returns>Returns true if the specified item is refcounted; false otherwise.</returns>
        public bool Tracked(TItem item)
        {
            if (ReferenceEquals(item, null)) {
                return false;
            }

            ItemRefCount itemRef;
            return _refs.TryGetValue(item, out itemRef);
        }

        /// <summary> Increments the refcount for the given item. </summary>
        /// <remarks>This method is thread-safe.</remarks>
        /// <param name="item">The item that should be retained.</param>
        public void Retain(TItem item)
        {
            if (ReferenceEquals(item, null)) {
                return;
            }

            _refs.AddOrUpdate(item, _ => {
                var freshRef = new ItemRefCount(1);
                FirstTimeRetained.Raise(item);
                return freshRef;
            }, (_, existingRef) => {
                existingRef.Inc();
                return existingRef;
            });

            Incremented.Raise(item);
        }

        /// <summary> Decrements the refcount for the given item. </summary>
        /// <remarks>This method is thread-safe.</remarks>
        /// <param name="item">The item to release.</param>
        public void Release(TItem item)
        {
            if (ReferenceEquals(item, null)) {
                return;
            }

            ItemRefCount itemRef;
            if (_refs.TryGetValue(item, out itemRef)) {
                lock (itemRef) {
                    if (itemRef.Count > 0) {
                        itemRef.Dec();
                        Decremented.Raise(item);
                        ItemRefCount removedRef;
                        if (itemRef.IsZero() && _refs.TryRemove(item, out removedRef)) {
                            Released.Raise(item);
                        }
                    }
                }
            }
            else {
                // Retain() was never called, so assume there is only one reference, which is now calling Release().
                Released.Raise(item);
            }
        }

        /// <summary> Sets refcount to zero for all retained items and releases them. </summary>
        /// <remarks>This method is thread-safe.</remarks>
        public void Clear()
        {
            foreach (var refKvp in _refs) {
                ItemRefCount itemRef;
                if (_refs.TryRemove(refKvp.Key, out itemRef)) {
                    itemRef.SetToZero();
                    Decremented.Raise(refKvp.Key);
                    Released.Raise(refKvp.Key);
                }
            }
        }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString()
        {
            return $"{_refs.Count} refcounted items";
        }
    }
}
