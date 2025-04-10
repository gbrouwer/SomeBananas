using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Halverson
{
    public static class HalversonExtensions
    {
        /// <summary>
        /// Returns a random subset of the given list with 'n' elements.
        /// </summary>
        public static List<Vector3> TakeRandomSubset(this List<Vector3> source, int n)
        {
            if (source == null || source.Count == 0)
                throw new ArgumentException("Source list cannot be null or empty.");

            if (n <= 0)
                throw new ArgumentException("Subset size must be greater than zero.");

            if (n > source.Count)
                throw new ArgumentException("Subset size cannot be greater than the list size.");

            return source.OrderBy(_ => UnityEngine.Random.value).Take(n).ToList();
        }

        /// <summary>
        /// Returns a contiguous subset of the given list from start index with 'n' elements.
        /// </summary>
        public static List<Vector3> TakeSubset(this List<Vector3> source, int startIndex, int n)
        {
            if (source == null || source.Count == 0)
                throw new ArgumentException("Source list cannot be null or empty.");

            if (startIndex < 0 || startIndex >= source.Count)
                throw new ArgumentOutOfRangeException(nameof(startIndex), "Start index out of range.");

            if (n <= 0)
                throw new ArgumentException("Subset size must be greater than zero.");

            return source.Skip(startIndex).Take(n).ToList();
        }
    }
}
