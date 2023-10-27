using System;
using System.Collections.Generic;
using System.Linq;

namespace ErXZEService.Helper
{
    public static class IEnumerableExtensions
    {
        public static TResult MaxOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult defaultValue = default)
        {
            if (source == null || !source.Any())
            {
                return defaultValue;
            }

            return source.Max(selector);
        }

        public static TResult MinOrDefault<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> selector, TResult defaultValue = default)
        {
            if (source == null || !source.Any())
            {
                return defaultValue;
            }

            return source.Min(selector);
        }

        public static decimal AverageOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector, decimal defaultValue = default)
        {
            if (source == null || !source.Any())
            {
                return defaultValue;
            }

            return source.Average(selector);
        }
    }
}
