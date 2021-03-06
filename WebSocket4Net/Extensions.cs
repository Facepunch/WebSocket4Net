﻿using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

namespace WebSocket4Net
{
    public static class Extensions
    {
        private readonly static char[] m_CrLf = { '\r', '\n' };

        public static void AppendFormatWithCrLf(this StringBuilder builder, string format, object arg)
        {
            builder.AppendFormat(format, arg);
            builder.Append(m_CrLf);
        }

        public static void AppendFormatWithCrLf(this StringBuilder builder, string format, params object[] args)
        {
            builder.AppendFormat(format, args);
            builder.Append(m_CrLf);
        }

        public static void AppendWithCrLf(this StringBuilder builder, string content)
        {
            builder.Append(content);
            builder.Append(m_CrLf);
        }

        public static void AppendWithCrLf(this StringBuilder builder)
        {
            builder.Append(m_CrLf);
        }

        private const string m_Tab = "\t";
        private const char m_Colon = ':';
        private const string m_Space = " ";
        private const string m_ValueSeparator = ", ";

        public static bool ParseMimeHeader(this string source, IDictionary<string, object> valueContainer, out string verbLine)
        {
            verbLine = string.Empty;

            var items = valueContainer;

            string line;
            string prevKey = string.Empty;

            var reader = new StringReader(source);

            while (!string.IsNullOrEmpty(line = reader.ReadLine()))
            {
                if (string.IsNullOrEmpty(verbLine))
                {
                    verbLine = line;
                    continue;
                }

                object currentValue;

                if (line.StartsWith(m_Tab) && !string.IsNullOrEmpty(prevKey))
                {
                    if (!items.TryGetValue(prevKey, out currentValue))
                        return false;

                    items[prevKey] = currentValue + line.Trim();
                    continue;
                }

                int pos = line.IndexOf(m_Colon);

                if (pos < 0)
                    continue;

                string key = line.Substring(0, pos);

                if (!string.IsNullOrEmpty(key))
                    key = key.Trim();

                string value = line.Substring(pos + 1);
                if (!string.IsNullOrEmpty(value) && value.StartsWith(m_Space) && value.Length > 1)
                    value = value.Substring(1);

                if (string.IsNullOrEmpty(key))
                    continue;

                if (!items.TryGetValue(key, out currentValue))
                {
                    items.Add(key, value);
                }
                else
                {
                    items[key] = currentValue + m_ValueSeparator + value;
                }

                prevKey = key;
            }

            return true;
        }

        public static TValue GetValue<TValue>(this IDictionary<string, object> valueContainer, string name, TValue defaultValue)
        {
            object value;

            if (!valueContainer.TryGetValue(name, out value))
                return defaultValue;

            return (TValue)value;
        }

        public static string GetOrigin(this Uri uri)
        {
#if NETFX_CORE || NETCORE
            return uri.GetComponents(UriComponents.SchemeAndServer, UriFormat.Unescaped);
#else
            return uri.GetLeftPart(UriPartial.Authority);
#endif
        }

        public static string CalculateChallenge(this string source)
        {
#if NETFX_CORE
            var algProv = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Sha1);
            var hash = algProv.CreateHash();
            hash.Append(CryptographicBuffer.ConvertStringToBinary(source, BinaryStringEncoding.Utf8));
            return CryptographicBuffer.EncodeToBase64String(hash.GetValueAndReset());

#elif !SILVERLIGHT
            return Convert.ToBase64String(SHA1.Create().ComputeHash(Encoding.ASCII.GetBytes(source)));
#else
            return Convert.ToBase64String(SHA1.Create().ComputeHash(ASCIIEncoding.Instance.GetBytes(source)));
#endif

        }
    }
}
