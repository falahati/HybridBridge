using System;
using System.Collections.Generic;
using System.Linq;

namespace HybridBridge.InstancePool
{
    /// <summary>
    ///     Represents a list of objects to be registered on
    ///     <see cref="GlobalPool" />
    /// </summary>
    public class InstanceList : List<object>
    {
        /// <summary>
        ///     Creates a new instance of this class
        /// </summary>
        public InstanceList()
        {
        }


        /// <summary>
        ///     Creates a new instance of this class and fills it with passed collection of objects
        /// </summary>
        /// <param name="collection">The collection to add instances from</param>
        public InstanceList(IEnumerable<object> collection)
        {
            AddRange(collection);
        }

        /// <summary>
        ///     Removes all instances from the list
        /// </summary>
        public new void Clear()
        {
            foreach (var key in ToArray())
            {
                Remove(key);
            }
        }

        /// <summary>
        ///     Adds all of the instances in the passed collection of objects
        /// </summary>
        /// <param name="collection">The collection to add instances from</param>
        public new void AddRange(IEnumerable<object> collection)
        {
            foreach (var item in collection)
            {
                Add(item);
            }
        }

        /// <summary>
        ///     Removes all the instances from the list matching the provided critics
        /// </summary>
        /// <param name="match">The method to determinate if an instance should be removed from the list</param>
        public new void RemoveAll(Predicate<object> match)
        {
            foreach (var item in ToArray().Where(o => match(o)))
            {
                Remove(item);
            }
        }

        /// <summary>
        ///     Removes all the instances from the list falling in the specified range
        /// </summary>
        /// <param name="index">The index that marks the start of the range</param>
        /// <param name="count">The number of items to include</param>
        public new void RemoveRange(int index, int count)
        {
            for (var i = index + count; i >= index; i--)
            {
                RemoveAt(i);
            }
        }

        /// <summary>
        ///     Removes an instance at the specified index
        /// </summary>
        /// <param name="index"></param>
        public new void RemoveAt(int index)
        {
            var item = this[index];
            base.RemoveAt(index);
            GlobalPool.RemoveInstance(item);
        }

        /// <summary>
        ///     Adds a new instance to the list
        /// </summary>
        /// <param name="item">The instance to add</param>
        public new void Add(object item)
        {
            base.Add(item);
            GlobalPool.AddInstance(item);
        }

        /// <summary>
        ///     Removes an instance from the list
        /// </summary>
        /// <param name="item">The instance to remove</param>
        /// <returns>
        ///     <see langword="true" /> if the passed instance was in list and removed, otherwise <see langword="false" />
        /// </returns>
        public new bool Remove(object item)
        {
            var index = IndexOf(item);
            if (index < 0)
            {
                return false;
            }
            RemoveAt(index);
            return true;
        }
    }
}