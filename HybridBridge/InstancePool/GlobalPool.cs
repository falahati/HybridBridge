using System;
using System.Collections.Generic;
using System.Linq;

namespace HybridBridge.InstancePool
{
    /// <summary>
    ///     Contains a list of all registered instances and their identical
    ///     identification string
    /// </summary>
    public class GlobalPool
    {
        private static readonly object Lock = new object();
        private static readonly Dictionary<InstanceInfo, int> Usages = new Dictionary<InstanceInfo, int>();

        /// <summary>
        ///     Returns the instance identification string for the passed instance
        /// </summary>
        /// <param name="instance">The instance to search for</param>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <returns>Returns the identification string of the passed instance</returns>
        public static string GetInstanceId<T>(T instance) where T : class
        {
            lock (Lock)
            {
                return Usages.Keys.FirstOrDefault(info => info.Object == instance)?.Id;
            }
        }

        /// <summary>
        ///     Returns the registered instance for the provide instance identification string
        /// </summary>
        /// <param name="instanceId">The instance identification string</param>
        /// <returns>Returns the instance that matched the instance identification string</returns>
        public static object GetInstance(string instanceId)
        {
            lock (Lock)
            {
                return Usages.Keys.FirstOrDefault(info => info.Id == instanceId)?.Object;
            }
        }

        /// <summary>
        ///     Returns the registered instance for the provide instance identification string
        /// </summary>
        /// <param name="instanceId">The instance identification string</param>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <returns>Returns the instance that matched the instance identification string</returns>
        public static T GetInstance<T>(string instanceId) where T : class
        {
            return GetInstance(instanceId) as T;
        }

        /// <summary>
        ///     Goes through all registered instances to find the passed instance
        /// </summary>
        /// <param name="instance">The instance to search for</param>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <returns><see langword="true" /> if the passed instance was in list, otherwise <see langword="false" /></returns>
        public static bool IsInstanceInPool<T>(T instance) where T : class
        {
            lock (Lock)
            {
                return Usages.Keys.Any(info => info.Object == instance);
            }
        }

        /// <summary>
        ///     Goes through all registered instances to find the instance marching the passed instance identification string
        /// </summary>
        /// <param name="instanceId">The instance identification string to search for</param>
        /// <returns>
        ///     <see langword="true" /> if the passed instance identification string was in list, otherwise
        ///     <see langword="false" />
        /// </returns>
        public static bool IsInPool(string instanceId)
        {
            lock (Lock)
            {
                return Usages.Keys.Any(info => info.Id == instanceId);
            }
        }

        /// <summary>
        ///     Adds a new instance to the pool
        /// </summary>
        /// <param name="instance">The instance to add</param>
        /// <typeparam name="T">The type of the instance</typeparam>
        public static void AddInstance<T>(T instance) where T : class
        {
            lock (Lock)
            {
                var instanceInfo = Usages.Keys.FirstOrDefault(info => info.Object == instance);
                if (instanceInfo != null)
                {
                    Usages[instanceInfo]++;
                }
                else
                {
                    instanceInfo = new InstanceInfo(instance);
                    Usages.Add(instanceInfo, 1);
                }
            }
        }

        /// <summary>
        ///     Removes an instance from the pool
        /// </summary>
        /// <param name="instance">The instance to remove</param>
        /// <typeparam name="T">The type of the instance</typeparam>
        /// <returns>
        ///     <see langword="true" /> if the instance was in the pool and removed, otherwise <see langword="false" />
        /// </returns>
        public static bool RemoveInstance<T>(T instance) where T : class
        {
            lock (Lock)
            {
                var instanceInfo = Usages.Keys.FirstOrDefault(info => info.Object == instance);
                if (instanceInfo == null)
                {
                    return false;
                }
                Usages[instanceInfo]--;
                if (Usages[instanceInfo] <= 0)
                {
                    Usages.Remove(instanceInfo);
                }
            }
            return true;
        }


        private class InstanceInfo
        {
            public InstanceInfo(object o)
            {
                Id = Guid.NewGuid().ToString();
                Object = o;
            }

            public string Id { get; }
            public object Object { get; }
        }
    }
}