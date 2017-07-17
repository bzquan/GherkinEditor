//----------------------------------------------------------------------------------------
//	Copyright © 2006 - 2017 Tangible Software Solutions Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class is used to convert some of the C++ std::vector methods to C#.
//----------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

internal static class VectorHelper
{
    internal static void Resize<T>(this List<T> list, int newSize, T value = default(T))
    {
        int cur = list.Count;
        if (newSize < cur)
            list.RemoveRange(newSize, cur - newSize);
        else if (newSize > cur)
        {
            if (newSize > list.Capacity)//this bit is purely an optimisation, to avoid multiple automatic capacity changes.
                list.Capacity = newSize;
            list.AddRange(Enumerable.Repeat(value, newSize - cur));
        }
    }
}