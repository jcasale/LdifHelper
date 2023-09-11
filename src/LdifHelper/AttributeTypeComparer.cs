namespace LdifHelper;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a comparer which orders by attribute type name naturally with the exception of placing the objectClass first.
/// </summary>
public class AttributeTypeComparer : Comparer<string>
{
    /// <summary>
    /// Represents the attribute type to order first.
    /// </summary>
    private const string ObjectClass = "objectClass";

    /// <summary>
    /// Prevents a default instance of the <see cref="AttributeTypeComparer"/> from being created.
    /// </summary>
    private AttributeTypeComparer() { }

    /// <summary>
    /// Gets the default instance of the <see cref="AttributeTypeComparer"/> class.
    /// </summary>
    /// <value>The default instance.</value>
    public static AttributeTypeComparer GetComparer { get; } = new();

    /// <summary>
    /// Performs a comparison of two attribute types and returns a value indicating whether one type is less than, equal to, or greater than the other.
    /// </summary>
    /// <param name="x">The first attribute type to compare.</param>
    /// <param name="y">The second attribute type to compare.</param>
    /// <returns>A signed integer that indicates the relative values of x and y.</returns>
    public override int Compare(string x, string y)
    {
        if (string.IsNullOrWhiteSpace(x))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(x));
        }

        if (string.IsNullOrWhiteSpace(y))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(y));
        }

        if (x.Equals(ObjectClass, StringComparison.OrdinalIgnoreCase)
            && y.Equals(ObjectClass, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        if (x.Equals(ObjectClass, StringComparison.OrdinalIgnoreCase)
            && !y.Equals(ObjectClass, StringComparison.OrdinalIgnoreCase))
        {
            return -1;
        }

        if (!x.Equals(ObjectClass, StringComparison.OrdinalIgnoreCase)
            && y.Equals(ObjectClass, StringComparison.OrdinalIgnoreCase))
        {
            return 1;
        }

        return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
    }
}