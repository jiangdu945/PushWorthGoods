﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PushWorthGoods
{
    /// <summary>
    /// 工具类
    /// </summary>
    public class Utils
    {
        /// <summary>
        /// 字符串转Unicode
        /// </summary>
        /// <param name="source">源字符串</param>
        /// <returns>Unicode编码后的字符串</returns>
        public static string String2Unicode(string str)
        {
            var bytes = Encoding.Unicode.GetBytes(str);
            var stringBuilder = new StringBuilder();
            for (var i = 0; i < bytes.Length; i += 2)
            {
                stringBuilder.AppendFormat("\\u{0}{1}", bytes[i + 1].ToString("x").PadLeft(2, '0'), bytes[i].ToString("x").PadLeft(2, '0'));
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Unicode转字符串
        /// </summary>
        /// <param name="unicode">经过Unicode编码的字符串</param>
        /// <returns>正常字符串</returns>
        internal static string Unicode2String(string unicode)
        {
            return new Regex(@"\\u([0-9A-F]{4})", RegexOptions.IgnoreCase | RegexOptions.Compiled)
                .Replace(unicode, x => Convert.ToChar(Convert.ToUInt16(x.Result("$1"), 16)).ToString());
        }


    }
}
