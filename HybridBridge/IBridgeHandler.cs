using System.Collections.Generic;

namespace HybridBridge
{
    /// <summary>
    ///     Interface that needs to be implemented by handlers to be
    ///     understandable by the holding <see cref="BridgeController" />
    /// </summary>
    public interface IBridgeHandler
    {
        /// <summary>
        ///     Handles the passed request and returns the result
        /// </summary>
        /// <param name="method">The method name to handle</param>
        /// <param name="parameters">The method parameters</param>
        /// <param name="hasResult">A boolean value indicting if the handling process resulted in a value</param>
        /// <returns>Returns the value that created from the handling of the request</returns>
        object InterceptRequest(string method, Dictionary<string, object> parameters, out bool hasResult);

        /// <summary>
        ///     Checks the request and returns true if this request can be handled by this handler
        /// </summary>
        /// <param name="handler">The name of requested handler</param>
        /// <param name="method">The method name to handle</param>
        /// <returns>true, if this handler can handle this request, false otherwise</returns>
        bool ShouldInterceptRequest(string handler, string method);

        /// <summary>
        ///     Initialize the handler and generates the needed Javascript code
        /// </summary>
        /// <param name="bridge">The <see cref="BridgeController" /> object requesting initialization</param>
        void Initialize(BridgeController bridge);

        /// <summary>
        ///     Event that gets raised when handler needs to push some Javascript code
        /// </summary>
        event PushJavascriptEvent PushJavascript;
    }
}