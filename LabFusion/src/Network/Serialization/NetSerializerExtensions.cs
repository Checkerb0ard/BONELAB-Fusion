using System.Text.Json;

namespace LabFusion.Network.Serialization;

public static class NetSerializerExtensions
{
    /// <summary>
    /// Serializes an object by its json representation.
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="value"></param>
    public static void SerializeValueByJson(this INetSerializer serializer, ref object value)
    {
        if (!SerializeHasValue(serializer, ref value))
        {
            return;
        }

        var type = SerializeType(serializer, ref value);

        SerializeJson(serializer, ref value, type);
    }

    /// <summary>
    /// Serializes an object. 
    /// <para>For built in data-types, this will use a custom serialization implementation. 
    /// For INetSerializables, it will use their serialization implementation.
    /// For other objects, it will attempt to serialize the object as json.</para>
    /// <para>Note that this is inefficient and should only be used when generically serializing an object that you do not know the type of.</para>
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="value"></param>
    /// <exception cref="NotSupportedException"></exception>
    public static void SerializeValue(this INetSerializer serializer, ref object value)
    {
        if (!SerializeHasValue(serializer, ref value))
        { 
            return; 
        }

        var type = SerializeType(serializer, ref value);

        if (type == typeof(int))
        {
            int casted = (int)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(uint))
        {
            uint casted = (uint)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(short))
        {
            short casted = (short)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(ushort))
        {
            ushort casted = (ushort)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(long))
        {
            long casted = (long)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(ulong))
        {
            ulong casted = (ulong)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(double))
        {
            double casted = (double)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(bool))
        {
            bool casted = (bool)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(byte))
        {
            byte casted = (byte)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(sbyte))
        {
            sbyte casted = (sbyte)value;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(byte?))
        {
            byte? casted = value as byte?;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(ushort?))
        {
            ushort? casted = value as ushort?;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(byte[]))
        {
            byte[] casted = value as byte[];
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type == typeof(string))
        {
            string casted = value as string;
            serializer.SerializeValue(ref casted);
            value = casted;
        }
        else if (type.IsAssignableTo(typeof(INetSerializable)))
        {
            var serializable = value as INetSerializable;

            serializer.SerializeValue(ref serializable, type);

            if (serializer.IsReader)
            {
                value = serializable;
            }
        }
        else if (type.IsValueType || type.IsSerializable)
        {
            SerializeJson(serializer, ref value, type);
        }
        else
        {
            throw new NotSupportedException($"Serialization of type {type.FullName} is not supported.");
        }
    }

    /// <summary>
    /// Serializes a <typeparamref name="TSerializable"/>.
    /// </summary>
    /// <typeparam name="TSerializable"></typeparam>
    /// <param name="serializer"></param>
    /// <param name="value"></param>
    public static void SerializeValue<TSerializable>(this INetSerializer serializer, ref TSerializable value) where TSerializable : INetSerializable, new()
    {
        if (serializer.IsReader)
        {
            value = new TSerializable();
        }

        value.Serialize(serializer);
    }

    /// <summary>
    /// Serializes a nullable <typeparamref name="TSerializable"/>.
    /// </summary>
    /// <typeparam name="TSerializable"></typeparam>
    /// <param name="serializer"></param>
    /// <param name="value"></param>
    public static void SerializeValue<TSerializable>(this INetSerializer serializer, ref TSerializable? value) where TSerializable : struct, INetSerializable
    {
        bool hasValue = value.HasValue;

        serializer.SerializeValue(ref hasValue);

        if (!hasValue)
        {
            return;
        }

        // Serializing value.Value directly will only serialize to a copy, since its a struct
        // So we must serialize to a local value first then copy it to the reference
        TSerializable result = serializer.IsReader ? new() : value.Value;

        result.Serialize(serializer);

        value = result;
    }

    /// <summary>
    /// Serializes an array segment of <typeparamref name="TSerializable"/>.
    /// </summary>
    /// <typeparam name="TSerializable"></typeparam>
    /// <param name="serializer"></param>
    /// <param name="value"></param>
    public static void SerializeValue<TSerializable>(this INetSerializer serializer, ref ArraySegment<TSerializable> value) where TSerializable : INetSerializable, new()
    {
        int count = 0;

        if (!serializer.IsReader)
        {
            count = value.Count;
        }

        serializer.SerializeValue(ref count);

        if (serializer.IsReader)
        {
            value = new ArraySegment<TSerializable>(new TSerializable[count]);
        }

        for (var i = 0; i < count; i++)
        {
            var serializable = value[i];

            serializer.SerializeValue(ref serializable);

            value[i] = serializable;
        }
    }

    /// <summary>
    /// Serializes an INetSerializable given its type.
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="value"></param>
    /// <param name="type"></param>
    public static void SerializeValue(this INetSerializer serializer, ref INetSerializable value, Type type)
    {
        if (serializer.IsReader)
        {
            value = Activator.CreateInstance(type) as INetSerializable;
        }

        value.Serialize(serializer);
    }

    /// <summary>
    /// Serializes the major, minor, and build values of a version.
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="value"></param>
    public static void SerializeValue(this INetSerializer serializer, ref Version value)
    {
        int major = 0, minor = 0, build = 0;

        if (!serializer.IsReader)
        {
            major = value.Major;
            minor = value.Minor;
            build = value.Build;
        }

        serializer.SerializeValue(ref major);
        serializer.SerializeValue(ref minor);
        serializer.SerializeValue(ref build);

        if (serializer.IsReader)
        {
            value = new Version(major, minor, build);
        }
    }

    /// <summary>
    /// Serializes a dictionary with string keys and string values.
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="value"></param>
    public static void SerializeValue(this INetSerializer serializer, ref Dictionary<string, string> value)
    {
        int length = 0;

        if (!serializer.IsReader)
        {
            length = value.Count;
        }

        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
        {
            value = new(length);

            for (var i = 0; i < length; i++)
            {
                string keyString = null;
                string valueString = null;

                serializer.SerializeValue(ref keyString);
                serializer.SerializeValue(ref valueString);

                value.Add(keyString, valueString);
            }
        }
        else
        {
            foreach (var pair in value)
            {
                string keyString = pair.Key;
                string valueString = pair.Value;

                serializer.SerializeValue(ref keyString);
                serializer.SerializeValue(ref valueString);
            }
        }
    }

    /// <summary>
    /// Serializes a list of strings.
    /// </summary>
    /// <param name="serializer"></param>
    /// <param name="value"></param>
    public static void SerializeValue(this INetSerializer serializer, ref List<string> value)
    {
        int length = 0;

        if (!serializer.IsReader)
        {
            length = value.Count;
        }

        serializer.SerializeValue(ref length);

        if (serializer.IsReader)
        {
            value = new(length);

            for (var i = 0; i < length; i++)
            {
                string readString = null;

                serializer.SerializeValue(ref readString);

                value.Add(readString);
            }
        }
        else
        {
            for (var i = 0; i < length; i++)
            {
                string writtenString = value[i];

                serializer.SerializeValue(ref writtenString);
            }
        }
    }

    /// <summary>
    /// Returns the size, in bytes, of the INetSerializable as a nullable.
    /// This accounts for extra data written for whether or not it is null.
    /// </summary>
    /// <typeparam name="TSerializable"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int? GetNullableSize<TSerializable>(this TSerializable? value) where TSerializable : struct, INetSerializable
    {
        int? size = sizeof(bool);

        if (!value.HasValue)
        {
            return size;
        }

        size += value.Value.GetSize();

        return size;
    }

    private static Type SerializeType(INetSerializer serializer, ref object value)
    {
        string typeName = null;

        if (!serializer.IsReader)
        {
            typeName = value.GetType().AssemblyQualifiedName;
        }

        serializer.SerializeValue(ref typeName);

        var type = Type.GetType(typeName);

        return type;
    }

    private static bool SerializeHasValue(INetSerializer serializer, ref object value)
    {
        bool hasValue = value != null;

        serializer.SerializeValue(ref hasValue);

        if (!hasValue)
        {
            return false;
        }

        return true;
    }

    private static void SerializeJson(INetSerializer serializer, ref object value, Type type)
    {
        var options = new JsonSerializerOptions()
        {
            IncludeFields = true,
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
        };

        var json = JsonSerializer.SerializeToUtf8Bytes(value, options);

        serializer.SerializeValue(ref json);

        if (serializer.IsReader)
        {
            value = JsonSerializer.Deserialize(json, type, options);
        }
    }
}
