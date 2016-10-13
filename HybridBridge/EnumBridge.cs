using System;

namespace HybridBridge
{
    /// <summary>
    ///     Contains static, reusable and non generic methods for the EnumBridge
    ///     class
    /// </summary>
    public static class EnumBridge
    {
        /// <summary>
        ///     Creates a new instance of <see cref="EnumBridge" /> class with the specified <see cref="Type" /> as the class's
        ///     generic type
        /// </summary>
        /// <param name="genericType">The type to create a <see cref="EnumBridge" /> instance for</param>
        /// <returns>Newly created <see cref="EnumBridge" /> instance</returns>
        public static IBridgeHandler FromType(Type genericType)
        {
            return Activator.CreateInstance(typeof (EnumBridge<>).MakeGenericType(genericType)) as IBridgeHandler;
        }
    }
}