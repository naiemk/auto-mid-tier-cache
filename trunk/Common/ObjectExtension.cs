using System;
using System.Collections.Generic;
using System.Text;


namespace Common
{
    public static class ObjectExtension
    {
        public static T Set<T>(this T me, Action<T> action)
        {
            action(me);
            return me;
        }

        public static TOut IfNotNull<TIn, TOut>(this TIn theIn, Func<TIn, TOut> f) where TIn : class
        {
            return theIn != null ? f(theIn) : default(TOut);
        }

        public static string FormatWith(this string str, params object[] pars)
        {
            return String.Format(str, pars);
        }

        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> data)
        {
            var hs = new HashSet<T>();
            foreach (var dt in data)
            {
                hs.Add(dt);
            }
            return hs;
        }

        public static string JoinStrings(this IEnumerable<string> me, string delim)
        {
            var sb = new StringBuilder();
            var first = true; 
            foreach (var s in me)
            {
                sb.Append((first ? "" : delim) + s);
                first = false;
            }
            return sb.ToString();
        }
    }
}
