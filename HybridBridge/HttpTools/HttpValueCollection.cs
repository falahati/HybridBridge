using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HybridBridge.HttpTools
{
    /// <summary>
    ///     A readonly collection of the query string key, value pairs
    /// </summary>
    public class HttpValueCollection : ReadOnlyCollection<HttpValue>
    {
        /// <summary>
        ///     Creates a new instances of this class and fills it with a collection of HttpValue instances
        /// </summary>
        /// <param name="list">The list of HttpValue instances</param>
        public HttpValueCollection(IList<HttpValue> list) : base(list)
        {
        }

        /// <summary>
        ///     Gets the value of the first matching query string pair
        /// </summary>
        /// <param name="key"></param>
        public string this[string key]
        {
            get { return this.First(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase)).Value; }
        }


        /// <summary>
        ///     Goes through all query string pairs for the provided key
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <returns><see langword="true" /> if the provided key was in the list, otherwise <see langword="false" /></returns>
        public bool ContainsKey(string key)
        {
            return this.Any(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        ///     Gets values of all query string pairs matching the provided key
        /// </summary>
        /// <param name="key">The key to search for</param>
        /// <returns>An array of all values</returns>
        public string[] GetValues(string key)
        {
            return
                this.Where(x => string.Equals(x.Key, key, StringComparison.OrdinalIgnoreCase))
                    .Select(x => x.Value)
                    .ToArray();
        }
    }
}