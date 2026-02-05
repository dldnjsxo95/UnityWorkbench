using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LWT.UnityWorkbench.Utilities
{
    /// <summary>
    /// Extension methods for collections (List, Array, IEnumerable).
    /// </summary>
    public static class CollectionExtensions
    {
        #region Random

        /// <summary>
        /// Gets a random element from the list.
        /// </summary>
        public static T GetRandom<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("Cannot get random element from empty list.");
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Gets a random element from the list or default if empty.
        /// </summary>
        public static T GetRandomOrDefault<T>(this IList<T> list)
        {
            if (list == null || list.Count == 0) return default;
            return list[UnityEngine.Random.Range(0, list.Count)];
        }

        /// <summary>
        /// Gets multiple random elements from the list.
        /// </summary>
        public static List<T> GetRandom<T>(this IList<T> list, int count)
        {
            if (list == null || list.Count == 0) return new List<T>();

            var result = new List<T>(count);
            var indices = Enumerable.Range(0, list.Count).ToList();
            indices.Shuffle();

            for (int i = 0; i < Mathf.Min(count, indices.Count); i++)
            {
                result.Add(list[indices[i]]);
            }
            return result;
        }

        /// <summary>
        /// Gets a random element using weighted selection.
        /// </summary>
        public static T GetWeightedRandom<T>(this IList<T> list, Func<T, float> weightSelector)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("Cannot get random element from empty list.");

            float totalWeight = 0f;
            foreach (var item in list)
            {
                totalWeight += weightSelector(item);
            }

            float randomValue = UnityEngine.Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var item in list)
            {
                currentWeight += weightSelector(item);
                if (randomValue <= currentWeight)
                    return item;
            }

            return list[list.Count - 1];
        }

        #endregion

        #region Shuffle

        /// <summary>
        /// Shuffles the list in place using Fisher-Yates algorithm.
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = UnityEngine.Random.Range(0, n + 1);
                (list[k], list[n]) = (list[n], list[k]);
            }
        }

        /// <summary>
        /// Returns a new shuffled list.
        /// </summary>
        public static List<T> Shuffled<T>(this IEnumerable<T> enumerable)
        {
            var list = enumerable.ToList();
            list.Shuffle();
            return list;
        }

        #endregion

        #region Null & Empty Checks

        /// <summary>
        /// Checks if the collection is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection)
        {
            return collection == null || collection.Count == 0;
        }

        /// <summary>
        /// Checks if the enumerable is null or empty.
        /// </summary>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable == null || !enumerable.Any();
        }

        /// <summary>
        /// Returns empty enumerable if null.
        /// </summary>
        public static IEnumerable<T> OrEmpty<T>(this IEnumerable<T> enumerable)
        {
            return enumerable ?? Enumerable.Empty<T>();
        }

        #endregion

        #region ForEach

        /// <summary>
        /// Performs an action on each element.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var item in enumerable)
            {
                action(item);
            }
        }

        /// <summary>
        /// Performs an action on each element with index.
        /// </summary>
        public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
        {
            int index = 0;
            foreach (var item in enumerable)
            {
                action(item, index++);
            }
        }

        #endregion

        #region Add & Remove

        /// <summary>
        /// Adds an item only if it doesn't already exist.
        /// </summary>
        public static bool AddUnique<T>(this IList<T> list, T item)
        {
            if (!list.Contains(item))
            {
                list.Add(item);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Adds multiple items.
        /// </summary>
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        /// <summary>
        /// Removes all items matching a predicate.
        /// </summary>
        public static int RemoveAll<T>(this IList<T> list, Predicate<T> match)
        {
            int removed = 0;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i]))
                {
                    list.RemoveAt(i);
                    removed++;
                }
            }
            return removed;
        }

        /// <summary>
        /// Removes and returns the first element.
        /// </summary>
        public static T PopFirst<T>(this IList<T> list)
        {
            if (list.Count == 0)
                throw new InvalidOperationException("Cannot pop from empty list.");

            T item = list[0];
            list.RemoveAt(0);
            return item;
        }

        /// <summary>
        /// Removes and returns the last element.
        /// </summary>
        public static T PopLast<T>(this IList<T> list)
        {
            if (list.Count == 0)
                throw new InvalidOperationException("Cannot pop from empty list.");

            int lastIndex = list.Count - 1;
            T item = list[lastIndex];
            list.RemoveAt(lastIndex);
            return item;
        }

        #endregion

        #region Find & Index

        /// <summary>
        /// Gets element at index or default if out of range.
        /// </summary>
        public static T GetAtOrDefault<T>(this IList<T> list, int index)
        {
            if (index < 0 || index >= list.Count) return default;
            return list[index];
        }

        /// <summary>
        /// Gets the first element or default.
        /// </summary>
        public static T FirstOrDefault<T>(this IList<T> list)
        {
            return list.Count > 0 ? list[0] : default;
        }

        /// <summary>
        /// Gets the last element or default.
        /// </summary>
        public static T LastOrDefault<T>(this IList<T> list)
        {
            return list.Count > 0 ? list[list.Count - 1] : default;
        }

        /// <summary>
        /// Finds the index of an item matching a predicate.
        /// </summary>
        public static int FindIndex<T>(this IList<T> list, Predicate<T> match)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (match(list[i])) return i;
            }
            return -1;
        }

        /// <summary>
        /// Finds the index of the minimum element.
        /// </summary>
        public static int IndexOfMin<T>(this IList<T> list) where T : IComparable<T>
        {
            if (list.Count == 0) return -1;

            int minIndex = 0;
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].CompareTo(list[minIndex]) < 0)
                    minIndex = i;
            }
            return minIndex;
        }

        /// <summary>
        /// Finds the index of the maximum element.
        /// </summary>
        public static int IndexOfMax<T>(this IList<T> list) where T : IComparable<T>
        {
            if (list.Count == 0) return -1;

            int maxIndex = 0;
            for (int i = 1; i < list.Count; i++)
            {
                if (list[i].CompareTo(list[maxIndex]) > 0)
                    maxIndex = i;
            }
            return maxIndex;
        }

        #endregion

        #region Dictionary

        /// <summary>
        /// Gets value or default from dictionary.
        /// </summary>
        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue defaultValue = default)
        {
            return dict.TryGetValue(key, out var value) ? value : defaultValue;
        }

        /// <summary>
        /// Gets or adds a value to the dictionary.
        /// </summary>
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, Func<TValue> valueFactory)
        {
            if (!dict.TryGetValue(key, out var value))
            {
                value = valueFactory();
                dict[key] = value;
            }
            return value;
        }

        /// <summary>
        /// Adds or updates a value in the dictionary.
        /// </summary>
        public static void AddOrUpdate<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key, TValue value)
        {
            dict[key] = value;
        }

        #endregion

        #region Conversion

        /// <summary>
        /// Converts to a HashSet.
        /// </summary>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> enumerable)
        {
            return new HashSet<T>(enumerable);
        }

        /// <summary>
        /// Converts to array, creating a new array only if necessary.
        /// </summary>
        public static T[] ToArrayEfficient<T>(this IEnumerable<T> enumerable)
        {
            return enumerable is T[] array ? array : enumerable.ToArray();
        }

        #endregion
    }
}
