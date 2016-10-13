using System;
using System.Collections;
using System.Globalization;
using System.Text;

namespace HybridBridge.HttpTools
{
    internal static class HttpHelper
    {
        public static int PollChar(string str, int offset, int length)
        {
            var num1 = 0;
            var num2 = length + offset;
            for (var index = offset; index < num2; ++index)
            {
                var ch = str[index];
                if (ch > sbyte.MaxValue)
                    return -1;
                var @int = ConvertCharBase((byte) ch);
                if (@int == -1)
                    return -1;
                num1 = (num1 << 4) + @int;
            }
            return num1;
        }

        public static void PushChar(IList buf, char ch, Encoding e)
        {
            if (ch > byte.MaxValue)
            {
                var encoding = e;
                var chars = new[]
                {
                    ch
                };
                foreach (var num in encoding.GetBytes(chars))
                    buf.Add(num);
            }
            else
                buf.Add((byte) ch);
        }

        private static int ConvertCharBase(byte b)
        {
            var ch = (char) b;
            if (ch >= 48 && ch <= 57)
                return ch - 48;
            if (ch >= 97 && ch <= 102)
                return ch - 97 + 10;
            if (ch >= 65 && ch <= 70)
                return ch - 65 + 10;
            return -1;
        }


        public static string HtmlDecode(string s)
        {
            if (s == null)
                return null;
            if (s.Length == 0)
                return string.Empty;
            if (s.IndexOf('&') == -1)
                return s;
            var stringBuilder1 = new StringBuilder();
            var stringBuilder2 = new StringBuilder();
            var stringBuilder3 = new StringBuilder();
            var length = s.Length;
            var num1 = 0;
            var num2 = 0;
            var flag1 = false;
            var flag2 = false;
            for (var index = 0; index < length; ++index)
            {
                var ch = s[index];
                if (num1 == 0)
                {
                    if (ch == 38)
                    {
                        stringBuilder2.Append(ch);
                        stringBuilder1.Append(ch);
                        num1 = 1;
                    }
                    else
                        stringBuilder3.Append(ch);
                }
                else if (ch == 38)
                {
                    num1 = 1;
                    if (flag2)
                    {
                        stringBuilder2.Append(num2.ToString(CultureInfo.InvariantCulture));
                        flag2 = false;
                    }
                    stringBuilder3.Append(stringBuilder2);
                    stringBuilder2.Length = 0;
                    stringBuilder2.Append('&');
                }
                else if (num1 == 1)
                {
                    if (ch == 59)
                    {
                        num1 = 0;
                        stringBuilder3.Append(stringBuilder2);
                        stringBuilder3.Append(ch);
                        stringBuilder2.Length = 0;
                    }
                    else
                    {
                        num2 = 0;
                        flag1 = false;
                        num1 = (int) ch == 35 ? 3 : 2;
                        stringBuilder2.Append(ch);
                        stringBuilder1.Append(ch);
                    }
                }
                else if (num1 == 2)
                {
                    stringBuilder2.Append(ch);
                    if (ch == 59)
                    {
                        var str = stringBuilder2.ToString();
                        if (str.Length > 1 && HttpEntities.Entities.ContainsKey(str.Substring(1, str.Length - 2)))
                            str = HttpEntities.Entities[str.Substring(1, str.Length - 2)].ToString();
                        stringBuilder3.Append(str);
                        num1 = 0;
                        stringBuilder2.Length = 0;
                        stringBuilder1.Length = 0;
                    }
                }
                else if (num1 == 3)
                {
                    if (ch == 59)
                    {
                        if (num2 == 0)
                            stringBuilder3.Append(stringBuilder1 + ";");
                        else if (num2 > ushort.MaxValue)
                        {
                            stringBuilder3.Append("&#");
                            stringBuilder3.Append(num2.ToString(CultureInfo.InvariantCulture));
                            stringBuilder3.Append(";");
                        }
                        else
                            stringBuilder3.Append((char) num2);
                        num1 = 0;
                        stringBuilder2.Length = 0;
                        stringBuilder1.Length = 0;
                        flag2 = false;
                    }
                    else if (flag1 && IsHexadecimalDigit(ch))
                    {
                        num2 = num2*16 + HexadecimalToInt(ch);
                        flag2 = true;
                        stringBuilder1.Append(ch);
                    }
                    else if (char.IsDigit(ch))
                    {
                        num2 = num2*10 + (ch - 48);
                        flag2 = true;
                        stringBuilder1.Append(ch);
                    }
                    else if (num2 == 0 && (ch == 120 || ch == 88))
                    {
                        flag1 = true;
                        stringBuilder1.Append(ch);
                    }
                    else
                    {
                        num1 = 2;
                        if (flag2)
                        {
                            stringBuilder2.Append(num2.ToString(CultureInfo.InvariantCulture));
                            flag2 = false;
                        }
                        stringBuilder2.Append(ch);
                    }
                }
            }
            if (stringBuilder2.Length > 0)
                stringBuilder3.Append(stringBuilder2);
            else if (flag2)
                stringBuilder3.Append(num2.ToString(CultureInfo.InvariantCulture));
            return stringBuilder3.ToString();
        }

        private static int HexadecimalToInt(char digit)
        {
            if (48 <= digit && digit <= 57)
                return digit - 48;
            if (97 <= digit && digit <= 102)
                return digit - 97 + 10;
            if (65 <= digit && digit <= 70)
                return digit - 65 + 10;
            throw new ArgumentException("digit");
        }


        private static bool IsHexadecimalDigit(char character)
        {
            if (48 <= character && character <= 57 || 97 <= character && character <= 102)
                return true;
            if (65 <= character)
                return character <= 70;
            return false;
        }
    }
}