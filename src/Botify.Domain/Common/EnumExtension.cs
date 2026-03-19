namespace Botify.Domain.Common;
public static class EnumExtensions
{
    /// <summary>
    /// Converts a string to the specified enum type.
    /// </summary>
    /// <typeparam name="TEnum">Enum type</typeparam>
    /// <param name="value">String value to convert</param>
    /// <param name="ignoreCase">Ignore case when parsing</param>
    /// <returns>Parsed enum value</returns>
    /// <exception cref="ArgumentException">If the string cannot be parsed</exception>
    public static TEnum ToEnum<TEnum>(this string value, bool ignoreCase = true) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));

        if (Enum.TryParse<TEnum>(value, ignoreCase, out var result))
        {
            return result;
        }

        throw new ArgumentException($"'{value}' is not a valid {typeof(TEnum).Name}");
    }

    /// <summary>
    /// Safe conversion: returns default enum if parsing fails.
    /// </summary>
    public static TEnum ToEnumOrDefault<TEnum>(this string value, TEnum defaultValue = default, bool ignoreCase = true) where TEnum : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return defaultValue;

        return Enum.TryParse<TEnum>(value, ignoreCase, out var result) ? result : defaultValue;
    }
}
