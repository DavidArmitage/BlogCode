using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.TagHelpers.Cache;
using Microsoft.Extensions.Caching.Memory;

public static class MemoryCacheExtensions
{
    private static readonly Func<MemoryCache, object> GetEntriesCollection = Delegate.CreateDelegate(
        typeof(Func<MemoryCache, object>),
        typeof(MemoryCache).GetProperty("EntriesCollection", BindingFlags.NonPublic | BindingFlags.Instance).GetGetMethod(true),
        throwOnBindFailure: true) as Func<MemoryCache, object>;

    public static IEnumerable GetKeys(this IMemoryCache memoryCache) {
        return ((IDictionary)GetEntriesCollection((MemoryCache)memoryCache)).Keys;
    }

    public static IEnumerable<T> GetKeys<T>(this IMemoryCache memoryCache) {
        return GetKeys(memoryCache).OfType<T>();
    }

    public static string GetKeyByVaryByValue(this IMemoryCache memoryCache, string varyByValue)
    {
        var keys = GetKeys(memoryCache);
        foreach (var key in keys)
        {
            FieldInfo varyByField = key.GetType().GetField("_varyBy", BindingFlags.NonPublic | BindingFlags.Instance);
            if(varyByField != null && varyByField.GetValue(key) != null && (string)varyByField.GetValue(key) == varyByValue)
            {
                PropertyInfo keyProperty = key.GetType().GetProperty("Key", BindingFlags.NonPublic | BindingFlags.Instance);
                if (keyProperty != null)
                    return (string)keyProperty.GetValue(key);
            }
        }

        return string.Empty;
    }

    public static CacheTagKey GetCacheTagKeyBy_varyBy(this IMemoryCache memoryCache, string varyByValue)
    {
        var keys = GetKeys(memoryCache);
        foreach (var key in keys)
        {
            FieldInfo varyByField = key.GetType().GetField("_varyBy", BindingFlags.NonPublic | BindingFlags.Instance);
            if (varyByField != null && varyByField.GetValue(key) != null && (string)varyByField.GetValue(key) == varyByValue)
                return (CacheTagKey)key;
        }

        return null;
    }

    public static List<CacheTagKey> GetCacheTagKeyBy_varyBy(this IMemoryCache memoryCache, IEnumerable<string> varyByValues)
    {
        if(varyByValues != null && varyByValues.Any())
        {
            List<CacheTagKey> cacheTagKeys = new List<CacheTagKey>();
            var keys = GetKeys(memoryCache);
            foreach (var key in keys)
            {
                FieldInfo varyByField = key.GetType().GetField("_varyBy", BindingFlags.NonPublic | BindingFlags.Instance);
                if (varyByField != null && varyByField.GetValue(key) != null && varyByValues.Contains((string)varyByField.GetValue(key)))
                    cacheTagKeys.Add((CacheTagKey)key);
            }

            return cacheTagKeys;
        }

        return null;
    }

    public static void RemoveCacheTagKeyBy_varyBy(this IMemoryCache memoryCache, string varyByValue)
    {
        var cacheTagKey = GetCacheTagKeyBy_varyBy(memoryCache, varyByValue);
        if (cacheTagKey != null)
            memoryCache.Remove(cacheTagKey);
    }

    public static void RemoveCacheTagKeyBy_varyBy(this IMemoryCache memoryCache, IEnumerable<string> varyByValues)
    {
        var cacheTagKeys = GetCacheTagKeyBy_varyBy(memoryCache, varyByValues);
        if(cacheTagKeys != null && cacheTagKeys.Any())
        {
            foreach (var cacheTagKey in cacheTagKeys)
                memoryCache.Remove(cacheTagKey);
        }
    }
}