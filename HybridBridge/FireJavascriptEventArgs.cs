namespace HybridBridge
{
    /// <summary>
    ///     The event arguments used by the PushJavascriptEvent delegate for
    ///     codes expecting no result at all
    /// </summary>
    public class FireJavascriptEventArgs : PushJavascriptEventArgs
    {
        /// <summary>
        ///     Creates a new instance of this class with the specific Javascript code
        /// </summary>
        /// <param name="script">The Javascript code to be pushed</param>
        public FireJavascriptEventArgs(string script) : base(script)
        {
            FireAway = true;
        }

        /// <summary>
        ///     Creates a new instance of this class with the specific Javascript code and one <see cref="BridgeController" /> to
        ///     target
        /// </summary>
        /// <param name="script">The Javascript code to be pushed</param>
        /// <param name="target">The <see cref="BridgeController" /> class to target</param>
        public FireJavascriptEventArgs(string script, BridgeController target) : base(script, target)
        {
            FireAway = true;
        }
    }
}