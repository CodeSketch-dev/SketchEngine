using System;
using System.Collections.Generic;
using UnityEngine;

namespace SketchEngine.Core.Extensions
{
    public static class ExtensionsLinq
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(
            this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var seen = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seen.Add(keySelector(element)))
                    yield return element;
            }
        }
    }
}
