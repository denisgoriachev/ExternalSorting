using System;
using System.Collections;
using System.Collections.Generic;

namespace ExternalSorting
{
    public class BinaryHeap<T> : IEnumerable<T>
    {
        private readonly IComparer<T> Comparer;
        private readonly List<T> Items;

        public BinaryHeap()
            : this(0, Comparer<T>.Default)
        {
        }

        public BinaryHeap(int capacity)
            : this(capacity, Comparer<T>.Default)
        {
        }

        public BinaryHeap(int capacity, IComparer<T> comp)
        {
            Comparer = comp;
            Items = new List<T>(capacity);
        }

        /// <summary>
        ///     Get a count of the number of items in the collection.
        /// </summary>
        public int Count
        {
            get { return Items.Count; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)Items).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        ///     Removes all items from the collection.
        /// </summary>
        public void Clear()
        {
            Items.Clear();
        }

        /// <summary>
        ///     Sets the capacity to the actual number of elements in the BinaryHeap,
        ///     if that number is less than a threshold value.
        /// </summary>
        /// <remarks>
        ///     The current threshold value is 90% (.NET 3.5), but might change in a future release.
        /// </remarks>
        public void TrimExcess()
        {
            Items.TrimExcess();
        }

        /// <summary>
        ///     Inserts an item onto the heap.
        /// </summary>
        /// <param name="newItem">The item to be inserted.</param>
        public void Insert(T newItem)
        {
            var i = Count;
            Items.Add(newItem);
            while (i > 0 && Comparer.Compare(Items[(i - 1) / 2], newItem) > 0)
            {
                Items[i] = Items[(i - 1) / 2];
                i = (i - 1) / 2;
            }
            Items[i] = newItem;
        }

        /// <summary>
        ///     Return the root item from the collection, without removing it.
        /// </summary>
        /// <returns>Returns the item at the root of the heap.</returns>
        public T Peek()
        {
            if (Items.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }
            return Items[0];
        }

        /// <summary>
        ///     Removes and returns the root item from the collection.
        /// </summary>
        /// <returns>Returns the item at the root of the heap.</returns>
        public T RemoveRoot()
        {
            if (Items.Count == 0)
            {
                throw new InvalidOperationException("The heap is empty.");
            }
            // Get the first item
            var rslt = Items[0];
            // Get the last item and bubble it down.
            var tmp = Items[Items.Count - 1];
            Items.RemoveAt(Items.Count - 1);
            if (Items.Count > 0)
            {
                var i = 0;
                while (i < Items.Count / 2)
                {
                    var j = 2 * i + 1;
                    if ((j < Items.Count - 1) && (Comparer.Compare(Items[j], Items[j + 1]) > 0))
                    {
                        ++j;
                    }
                    if (Comparer.Compare(Items[j], tmp) >= 0)
                    {
                        break;
                    }
                    Items[i] = Items[j];
                    i = j;
                }
                Items[i] = tmp;
            }
            return rslt;
        }
    }
}