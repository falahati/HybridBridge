namespace HybridBridge
{
    /// <summary>
    ///     Delegate used by IBridgeHandler's PushJavascript event
    /// </summary>
    /// <param name="sender">The IBridgeHandler that requested this push</param>
    /// <param name="eventArgs">
    ///     The PushJavascriptEventArgs containing information about this event
    /// </param>
    public delegate object PushJavascriptEvent(IBridgeHandler sender, PushJavascriptEventArgs eventArgs);
}