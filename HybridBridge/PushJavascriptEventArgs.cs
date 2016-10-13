using System;

namespace HybridBridge
{
    /// <summary>
    ///     The event arguments used by the PushJavascriptEvent delegate
    /// </summary>
    public class PushJavascriptEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of this class with the specific Javascript code
        /// </summary>
        /// <param name="script">The Javascript code to be pushed</param>
        public PushJavascriptEventArgs(string script) : this(script, null, null)
        {
        }

        /// <summary>
        ///     Creates a new instance of this class with the specific Javascript code and one <see cref="BridgeController" /> to
        ///     target
        /// </summary>
        /// <param name="script">The Javascript code to be pushed</param>
        /// <param name="target">The <see cref="BridgeController" /> class to target</param>
        public PushJavascriptEventArgs(string script, BridgeController target) : this(script, target, null)
        {
        }

        /// <summary>
        ///     Creates a new instance of this class with the specific Javascript code and a callback function for result
        /// </summary>
        /// <param name="script">The Javascript code to be pushed</param>
        /// <param name="callback">The callback function to be used for the result of the operation</param>
        public PushJavascriptEventArgs(string script, Action<object> callback) : this(script, null, callback)
        {
        }

        /// <summary>
        ///     Creates a new instance of this class with the specific Javascript code, one <see cref="BridgeController" /> to
        ///     target and a callback function for result
        /// </summary>
        /// <param name="script">The Javascript code to be pushed</param>
        /// <param name="target">The <see cref="BridgeController" /> class to target</param>
        /// <param name="callback">The callback function to be used for the result of the operation</param>
        public PushJavascriptEventArgs(string script, BridgeController target, Action<object> callback)
        {
            Script = script;
            Target = target;
            Callback = callback;
        }

        /// <summary>
        ///     Indicates if operations needs a result
        /// </summary>
        public bool FireAway { get; set; }

        /// <summary>
        ///     Contains the target <see cref="BridgeController" /> instance or <see langword="null" /> to match all listening
        ///     <see cref="BridgeController" /> instances
        /// </summary>
        public BridgeController Target { get; private set; }

        /// <summary>
        ///     The Javascript code to execute
        /// </summary>
        public string Script { get; private set; }

        /// <summary>
        ///     The callback function to be used for the result of the operation
        /// </summary>
        public Action<object> Callback { get; private set; }
    }
}