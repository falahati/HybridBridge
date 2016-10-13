using System.Collections.Generic;
using System.Text;

namespace HybridBridge.HttpTools
{
    /// <summary>
    ///     A static class containing useful methods for Http manipulation
    /// </summary>
    public static class HttpUtility
    {
        /// <summary>
        ///     Decodes an Http escaped string
        /// </summary>
        /// <param name="s">The string to decode</param>
        /// <returns>The decoded string</returns>
        public static string UrlDecode(string s)
        {
            return UrlDecode(s, Encoding.UTF8);
        }

        /// <summary>
        ///     Decodes an Http escaped string
        /// </summary>
        /// <param name="s">The string to decode</param>
        /// <param name="e">The string encoding</param>
        /// <returns>The decoded string</returns>
        public static string UrlDecode(string s, Encoding e)
        {
            if (s == null)
                return null;
            if (s.IndexOf('%') == -1 && s.IndexOf('+') == -1)
                return s;
            if (e == null)
                e = Encoding.UTF8;
            long num = s.Length;
            var list = new List<byte>();
            for (var index = 0; index < num; ++index)
            {
                var ch = s[index];
                if (ch == 37 && index + 2 < num && s[index + 1] != 37)
                {
                    if (s[index + 1] == 117 && index + 5 < num)
                    {
                        var @char = HttpHelper.PollChar(s, index + 2, 4);
                        if (@char != -1)
                        {
                            HttpHelper.PushChar(list, (char) @char, e);
                            index += 5;
                        }
                        else
                            HttpHelper.PushChar(list, '%', e);
                    }
                    else
                    {
                        int @char;
                        if ((@char = HttpHelper.PollChar(s, index + 1, 2)) != -1)
                        {
                            HttpHelper.PushChar(list, (char) @char, e);
                            index += 2;
                        }
                        else
                            HttpHelper.PushChar(list, '%', e);
                    }
                }
                else if (ch == 43)
                    HttpHelper.PushChar(list, ' ', e);
                else
                    HttpHelper.PushChar(list, ch, e);
            }
            var bytes = list.ToArray();
            return e.GetString(bytes, 0, bytes.Length);
        }

        /// <summary>
        ///     Parses a query string and returns the corresponding HttpValueCollection instance
        /// </summary>
        /// <param name="query">The query string to parse</param>
        /// <returns>The HttpValueCollection instance representing the passed query string</returns>
        public static HttpValueCollection ParseQueryString(string query)
        {
            return ParseQueryString(query, Encoding.UTF8);
        }

        /// <summary>
        ///     Parses a query string and returns the corresponding HttpValueCollection instance
        /// </summary>
        /// <param name="query">The query string to parse</param>
        /// <param name="encoding">The query string encoding</param>
        /// <returns>The HttpValueCollection instance representing the passed query string</returns>
        public static HttpValueCollection ParseQueryString(string query, Encoding encoding)
        {
            var result = new List<HttpValue>();
            if (query.Length == 0)
                return new HttpValueCollection(result);
            var str1 = HttpHelper.HtmlDecode(query);
            var length = str1.Length;
            var startIndex1 = 0;
            var flag = true;
            while (startIndex1 <= length)
            {
                var startIndex2 = -1;
                var num = -1;
                for (var index = startIndex1; index < length; ++index)
                {
                    if (startIndex2 == -1 && str1[index] == 61)
                        startIndex2 = index + 1;
                    else if (str1[index] == 38)
                    {
                        num = index;
                        break;
                    }
                }
                if (flag)
                {
                    flag = false;
                    if (str1[startIndex1] == 63)
                        ++startIndex1;
                }
                string name;
                if (startIndex2 == -1)
                {
                    name = null;
                    startIndex2 = startIndex1;
                }
                else
                    name = UrlDecode(str1.Substring(startIndex1, startIndex2 - startIndex1 - 1), encoding);
                if (num < 0)
                {
                    startIndex1 = -1;
                    num = str1.Length;
                }
                else
                    startIndex1 = num + 1;
                var str2 = UrlDecode(str1.Substring(startIndex2, num - startIndex2), encoding);
                result.Add(new HttpValue(name, str2));
                if (startIndex1 == -1)
                    break;
            }
            return new HttpValueCollection(result);
        }
    }
}