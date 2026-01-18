using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MiscUtils
{
#if NET_4_6
    public static dynamic Random(params dynamic[] items)
    {
        System.Random random = new System.Random();
        float r = (float)random.NextDouble();

        if (items.Length <= 0) return null;
        float prob = 1f / items.Length;
        int i = 0;
        foreach(dynamic item in items)
        {
            float min = i * prob;
            i++;
            float max = i * prob;

            if (r > min && r <= max)
                return item;

        }
        return null;
    }
#endif
}
