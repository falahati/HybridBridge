using System.Collections.Generic;
using System.Linq;

namespace HybridBridge.InstancePool
{
    /// <summary>
    ///     Represents a generic dictionary of instances to be registered on the
    ///     <see cref="GlobalPool" /> and their values
    /// </summary>
    /// <typeparam name="T">The type of the instance values</typeparam>
    public class InstanceDictionary<T> : Dictionary<object, T>
    {
        /// <summary>
        ///     Creates a new instance of this class
        /// </summary>
        public InstanceDictionary()
        {
        }

        /// <summary>
        ///     Creates a new instance of this class and fills it with the collection of provided pairs
        /// </summary>
        /// <param name="dictionary">The pairs to add to the dictionary</param>
        public InstanceDictionary(IDictionary<object, T> dictionary)
        {
            foreach (var item in dictionary)
            {
                Add(item.Key, item.Value);
            }
        }

        /// <summary>
        ///     Clears the dictionary by removing all instance pairs
        /// </summary>
        public new void Clear()
        {
            foreach (var key in Keys.ToArray())
            {
                Remove(key);
            }
        }

        /// <summary>
        ///     Adds a new instance and its value to the dictionary
        /// </summary>
        /// <param name="key">The instance</param>
        /// <param name="value">The instance value</param>
        public new void Add(object key, T value)
        {
            var contains = ContainsKey(key);
            base.Add(key, value); // may raise an exception, or not?
            if (!contains)
            {
                GlobalPool.AddInstance(key);
            }
        }

        /// <summary>
        ///     Removes an instance from the dictionary
        /// </summary>
        /// <param name="key">The instance to remove</param>
        /// <returns>
        ///     <see langword="true" /> if the instance was in dictionary and removed, otherwise <see langword="false" />
        /// </returns>
        public new bool Remove(object key)
        {
            if (!base.Remove(key))
            {
                return false;
            }
            GlobalPool.RemoveInstance(key);
            return true;
        }
    }
}