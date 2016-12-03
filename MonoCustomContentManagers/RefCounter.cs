using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace MonoCustomContentManagers
{
    /// <summary>
    ///     Provides reference counting for given item type. Calls event handler when
    ///     it is time for an item to be released. Thread-safe. Contains maximum of <see cref="Int32.MaxValue" /> items.
    /// </summary>
    /// <typeparam name="TItem">The type of the refcounted item.</typeparam>
    public class RefCounter<TItem>
    {
        /// <summary> Holds retained items and their refcounts. </summary>
        private readonly ConcurrentDictionary<TItem, ItemRefCount> _refs =
            new ConcurrentDictionary<TItem, ItemRefCount>();

        /// <summary>Occurs after counter for a reference was decremented.</summary>
        public event Action<TItem> Decremented;

        /// <summary>Occurs after a reference was retained for the first time.</summary>
        public event Action<TItem> FirstTimeRetained;

        /// <summary>Occurs after counter for a reference was icremented.</summary>
        public event Action<TItem> Incremented;

        /// <summary>Occurs after reference counter for a reference was set to zero and it was released.</summary>
        public event Action<TItem> Released;

        /// <summary> Gets the tracked items count. </summary>
        public int Count => _refs.Count;

        /// <summary> Sets refcount to zero for all retained items and releases them. </summary>
        /// <remarks>This method is thread-safe.</remarks>
        public void Clear()
        {
            foreach (var refKvp in _refs) {
                ItemRefCount itemRef;
                if (_refs.TryRemove(refKvp.Key, out itemRef)) {
                    itemRef.SetToZero();
                    Decremented.Call(refKvp.Key);
                    Released.Call(refKvp.Key);
                }
            }
        }

        /// <summary> Decrements the refcount for the given item. Releases the item if its refcount reaches zero.
        /// </summary>
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
                        Decremented.Call(item);
                        ItemRefCount removedRef;
                        if (itemRef.IsZero() && _refs.TryRemove(item, out removedRef)) {
                            Released.Call(item);
                        }
                    }
                }
            }
        }

        /// <summary> Increments the refcount for the given item. </summary>
        /// <remarks>This method is thread-safe.</remarks>
        /// <param name="item">The item that should be retained.</param>
        public void Retain(TItem item)
        {
            if (ReferenceEquals(item, null)) {
                return;
            }

            _refs.AddOrUpdate(
                item,
                _ => {
                    var freshRef = new ItemRefCount(1);
                    FirstTimeRetained.Call(item);
                    Incremented.Call(item);
                    return freshRef;
                },
                (_, existingRef) => {
                    existingRef.Inc();
                    Incremented.Call(item);
                    return existingRef;
                });
        }

        /// <summary> Returns a <see cref="string" /> that represents this instance. </summary>
        /// <returns> A <see cref="string" /> that represents this instance. </returns>
        public override string ToString()
        {
            return $"{_refs.Count} refcounted items";
        }

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

        /// <summary> Refcount value needs to be a reference type. </summary>
        private class ItemRefCount
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="RefCounter{TItem}.ItemRefCount" /> class.
            /// </summary>
            /// <param name="count">Initial refcount.</param>
            public ItemRefCount(int count = 0)
            {
                Count = count;
            }

            /// <summary> Returns current refcount. </summary>
            public int Count { get; private set; }

            /// <summary> Decrements refcount by 1. </summary>
            public void Dec()
            {
                Count--;
            }

            /// <summary> Increments refcount by 1. </summary>
            public void Inc()
            {
                Count++;
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
    }
}