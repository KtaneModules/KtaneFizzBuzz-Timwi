using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions {
    // Fisher-Yates Shuffle
    public static IList<T> Shuffle<T>(this IList<T> list, MonoRandom rnd) {
        int i = list.Count;
        while (i > 1) {
            int index = rnd.Next(i);
            i--;
            T value = list[index];
            list[index] = list[i];
            list[i] = value;
        }
        return list;
    }
}
