namespace HybridBridge.HttpTools
{
    /// <summary>
    ///     Representing a query string key, value pair
    /// </summary>
    public class HttpValue
    {
        /// <summary>
        ///     Creates a new instance of this class
        /// </summary>
        /// <param name="key">The query string key</param>
        /// <param name="value">The query string value</param>
        public HttpValue(string key, string value)
        {
            Key = key;
            Value = value;
        }

        /// <summary>
        ///     The query string key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        ///     The query string value
        /// </summary>
        public string Value { get; set; }
    }
}