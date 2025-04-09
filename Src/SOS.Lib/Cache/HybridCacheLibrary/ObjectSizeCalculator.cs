using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace SOS.Lib.Cache.HybridCacheLibrary;
public static class ObjectSizeCalculator
{
    // 16 byte reference + object header every reference type object
    public static long CalculateObjectSize(object obj)
    {
        if (obj == null) return 0;

        Type type = obj.GetType();
        long size = 0;

        // Handle value types and simple types directly
        if (type.IsValueType)
        {
            size = Marshal.SizeOf(obj);
        }
        else
        {
            // Reference type base size (reference + object header)
            size = IntPtr.Size + Marshal.SizeOf(typeof(IntPtr)); // Object reference size and object header (sync block + type object pointer)

            if (obj is string str)
            {
                size += CalculateStringObjectSize(str);
            }
            else
            {
                // Iterate over fields
                foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                {
                    object fieldValue = field.GetValue(obj);
                    size += CalculateFieldSize(fieldValue);
                }
            }
        }

        return size;
    }

    private static long CalculateFieldSize(object fieldValue)
    {
        if (fieldValue == null) return 0;

        Type fieldType = fieldValue.GetType();

        // Handle primitive types and value types
        if (fieldType.IsValueType)
        {
            return Marshal.SizeOf(fieldValue);
        }

        // Handle strings
        if (fieldValue is string str)
        {
            return IntPtr.Size + CalculateStringObjectSize(str); // String reference size + content size
        }

        // Handle arrays
        if (fieldType.IsArray)
        {
            long arraySize = IntPtr.Size; // Reference size for the array
            foreach (var element in (fieldValue as Array))
            {
                arraySize += CalculateFieldSize(element);
            }
            return arraySize;
        }

        // Handle lists and other collections
        if (fieldValue is ICollection collection)
        {
            long collectionSize = IntPtr.Size; // Reference size for the collection
            foreach (var item in collection)
            {
                collectionSize += CalculateFieldSize(item);
            }
            return collectionSize;
        }

        // Handle custom reference types recursively
        return CalculateObjectSize(fieldValue);
    }

    private static long CalculateStringObjectSize(string input)
    {
        long totalSize = 0;

        foreach (var c in input.AsSpan())
        {
            totalSize += c switch
            {
                <= '\x7F' => 1, // ASCII character (1 byte) + '\x7F' = 127 is the last ASCII character
                _ when char.IsSurrogate(c) => 4, // Surrogate character (UTF-32, 4 bytes)
                _ => Encoding.UTF8.GetByteCount(c.ToString()) // Non-ASCII, non-surrogate character (UTF-8)
            };
        }

        return totalSize;
    }
}


