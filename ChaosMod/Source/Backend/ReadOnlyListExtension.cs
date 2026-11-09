using System;
using System.Collections.Generic;

namespace Lego.ChaosMod.Backend;
public static class ReadOnlyListExtension {
    public static int IndexOf<T>(this IReadOnlyList<T> list, T item)
    {
        for (int i = 0; i < list.Count; i++) {
            if (list[i].Equals(item))
                return i;
        }
        return -1;
    }

    public static int FindIndex<T>(this IReadOnlyList<T> list, Predicate<T> match)
    {
        for (int i = 0; i < list.Count; i++) {
            if (match(list[i]))
                return i;
        }
        return -1;
    }

    public static T Find<T>(this IReadOnlyList<T> list, Predicate<T> match)
    {
        for (int i = 0; i < list.Count; i++) {
            if (match(list[i]))
                return list[i];
        }
        return default;
    }

    public static List<T> FindAll<T>(this IReadOnlyList<T> list, Predicate<T> match)
    {
        List<T> items = [];
        for (int i = 0; i < list.Count; i++) {
            if (match(list[i]))
                items.Add(list[i]);
        }
        return items;
    }
}