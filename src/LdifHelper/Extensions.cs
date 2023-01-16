namespace LdifHelper;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

/// <summary>
/// Represents extension methods used by the assembly.
/// </summary>
public static class Extensions
{
    /// <summary>
    /// Adds a new dictionary key and value or appends a value to an existing key.
    /// </summary>
    /// <param name="input">The dictionary to add or append to.</param>
    /// <param name="key">The dictionary key.</param>
    /// <param name="value">The dictionary value.</param>
    public static void AddOrAppend(this Dictionary<string, List<object>> input, string key, object value)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        if (key is null)
        {
            throw new ArgumentNullException(nameof(key), "The dictionary key can not be null.");
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentOutOfRangeException(nameof(key), "The dictionary key can not be empty or whitespace.");
        }

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "The dictionary value can not be null.");
        }

        if (input.TryGetValue(key, out var values))
        {
            values.Add(value);
        }
        else
        {
            input[key] = new List<object> { value };
        }
    }

    /// <summary>
    /// Generates an LDIF value-spec according to RFC2849.
    /// </summary>
    /// <param name="type">The attribute type.</param>
    /// <param name="value">The attribute value.</param>
    /// <returns>An LDIF value-spec according to RFC2849.</returns>
    /// <remarks>
    /// This method does not make assumptions on what the string representation of an object should be.
    /// All types must be converted to either a string or byte[] before being boxed.
    /// </remarks>
    public static string GetValueSpec(string type, object value)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type), "The attribute type can not be null.");
        }

        if (string.IsNullOrWhiteSpace(type))
        {
            throw new ArgumentOutOfRangeException(nameof(type), "The attribute type can not be empty or whitespace.");
        }

        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "The attribute value can not be null.");
        }

        if (value is string s)
        {
            var encode = false;
            if (!s.IsSafeInitChar())
            {
                encode = true;
            }
            else if (!s.IsSafeString())
            {
                encode = true;
            }

            return encode ? $"{type}:: {s.ToBase64()}" : $"{type}: {s}";
        }

        if (value is byte[] b)
        {
            return $"{type}:: {b.ToBase64()}";
        }

        throw new ArgumentOutOfRangeException(nameof(value), $"Unknown value type \"{value.GetType()}\".");
    }

    /// <summary>
    /// Determines RFC2849 safe initial character compliance.
    /// </summary>
    /// <param name="value">The dn or attribute value.</param>
    /// <returns>Whether or not the strings leading character is ASCII safe.</returns>
    public static bool IsSafeInitChar(this string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "The attribute value can not be null.");
        }

        // RFC2849 note 5.
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        // RFC2849 note 4.
        return Constants.SafeAsciiInitChars.Contains(value[0]);
    }

    /// <summary>
    /// Determines RFC2849 safe string compliance.
    /// </summary>
    /// <param name="value">The dn or attribute value.</param>
    /// <returns>Whether or not the string is ASCII safe.</returns>
    public static bool IsSafeString(this string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "The attribute value can not be null.");
        }

        // RFC2849 note 5.
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }

        // RFC2849 note 8.
        if (value[value.Length - 1] == 32)
        {
            return false;
        }

        // RFC2849 note 4.
        var chars = new HashSet<int>(value.Select(x => (int)x));

        return chars.IsSubsetOf(Constants.SafeAsciiChars);
    }

    /// <summary>
    /// BASE64 encodes a string.
    /// </summary>
    /// <param name="value">The value to encode.</param>
    /// <returns>The UTF8 based BASE64 encoded value.</returns>
    public static string ToBase64(this string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "The attribute value can not be null.");
        }

        var utf16Bytes = Encoding.Unicode.GetBytes(value);
        var utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);

        return Convert.ToBase64String(utf8Bytes);
    }

    /// <summary>
    /// BASE64 encodes a byte array.
    /// </summary>
    /// <param name="value">The value to encode.</param>
    /// <returns>The BASE64 encoded value.</returns>
    /// <remarks>The original binary encoding is not changed.</remarks>
    public static string ToBase64(this byte[] value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value), "The attribute value can not be null.");
        }

        return Convert.ToBase64String(value);
    }

    /// <summary>
    /// Converts a dictionary to an collection of LDAP attribute objects.
    /// </summary>
    /// <param name="input">The dictionary to convert.</param>
    /// <returns>A collection of LDAP attribute objects.</returns>
    public static IEnumerable<LdifAttribute> ToLdapAttributes(this Dictionary<string, List<object>> input)
    {
        if (input is null)
        {
            throw new ArgumentNullException(nameof(input));
        }

        foreach (var kvp in input)
        {
            yield return new LdifAttribute(kvp.Key, kvp.Value);
        }
    }

    /// <summary>
    /// Wraps lines according to RFC2849 note 10 compliance.
    /// </summary>
    /// <param name="value">The combined attribute type and value line.</param>
    /// <returns>A wrapped string if required.</returns>
    public static string Wrap(this string value)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }

        var stringBuilder = new StringBuilder();

        var startIndex = 0;
        var maxLen = value.Length;
        var wrapLen = Constants.MaxLineLength - 1;
        while (startIndex < maxLen)
        {
            int len;
            if (startIndex > 0)
            {
                // Subtract 1 from all but the first line to accommodate leading space.
                stringBuilder.AppendFormat(CultureInfo.InvariantCulture, "{0} ", Environment.NewLine);
                len = Math.Min(wrapLen, maxLen - startIndex);
            }
            else
            {
                len = Math.Min(Constants.MaxLineLength, maxLen - startIndex);
            }

            stringBuilder.Append(value.Substring(startIndex, len));
            startIndex += len;
        }

        return stringBuilder.ToString();
    }
}