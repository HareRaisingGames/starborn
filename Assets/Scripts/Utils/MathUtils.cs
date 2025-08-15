using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public static class MathUtils
{
    public static bool InRange(float value, float min, float max)
    {
        return value >= min && value <= max;
    }

    //Normalize a value between two points
    public static float Normalize(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    //Normalize a value but in reverse
    public static float ReverseNormalize(float value, float min, float max)
    {
        return 1 - Normalize(value, min, max);
    }

    //Find the average in a list of numeric values
    /**
     * This works well with floats, ints, and doubles.
     * Even if there's anything else, as long as there's at least one numeric value, it'll work
     */
    public static float ListAverage<T>(List<T> numbers)
    {
        float sum = 0;
        int length = 0;
        if(numbers.Count != 0)
        {
            foreach (T number in numbers)
            {
                if (!IsNumericType(number))
                    continue;

                if (number is float f)
                {
                    sum += f;
                    length += 1;
                }
                else if (number is double d)
                {
                    sum += (float)d;
                    length += 1;
                }
                else if (number is int i)
                {
                    sum += i;
                    length += 1;
                }
            }

            if(length >= 1) //To avoid a divide by 0
                return sum / length;

            throw new Exception("The following list does not contain at least one numeric value");
        }
        throw new Exception("The following list must contain at least one variable");
    }

    //Check if object is numeric
    public static bool IsNumericType(this object o)
    {
        switch (Type.GetTypeCode(o.GetType()))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }
}
