using System;
using Android.Webkit;

namespace HybridBridge.Android
{
    /// <summary>
    ///     The event arguments used by the JavascriptMessageEvent delegate
    /// </summary>
    public class JavascriptMessageEventArgs : EventArgs
    {
        /// <summary>
        ///     Creates a new instance of this class with the provided message string
        /// </summary>
        /// <param name="message">The message string</param>
        public JavascriptMessageEventArgs(string message)
        {
            MessageLevel = ConsoleMessage.MessageLevel.Error;
            Message = message;
        }

        /// <summary>
        ///     Creates a new instance of this class with the provided message string and message level
        /// </summary>
        /// <param name="message">The message string</param>
        /// <param name="messageLevel">The message level</param>
        public JavascriptMessageEventArgs(string message, ConsoleMessage.MessageLevel messageLevel) : this(message)
        {
            MessageLevel = messageLevel;
        }

        /// <summary>
        ///     The message string
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        ///     The message level
        /// </summary>
        public ConsoleMessage.MessageLevel MessageLevel { get; private set; }
    }
}