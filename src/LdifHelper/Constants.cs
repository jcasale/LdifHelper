namespace LdifHelper;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Represents constants used by the assembly.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Gets the maximum line length before a wrap is required.
    /// </summary>
    /// <value>The maximum line length before a wrap is required.</value>
    public static int MaxLineLength => 76;

    /// <summary>
    /// Gets a collection of integers representing the ASCII characters deemed unsafe as the non leading character in a string.
    /// </summary>
    /// <value>The collection of integers representing the ASCII characters deemed unsafe as the non leading character in a string.</value>
    public static HashSet<int> UnsafeAsciiChars { get; } = new(new[] { 0, 10, 13 });

    /// <summary>
    /// Gets a collection of integers representing the ASCII characters deemed unsafe as the leading character in a string.
    /// </summary>
    /// <value>The collection of integers representing the ASCII characters deemed unsafe as the leading character in a string.</value>
    public static HashSet<int> UnsafeAsciiInitChars { get; } = new(new[] { 0, 10, 13, 32, 58, 60 });

    /// <summary>
    /// Gets a collection of integers representing the ASCII characters deemed safe as the non leading character in a string.
    /// </summary>
    /// <value>The collection of integers representing the ASCII characters deemed safe as the non leading character in a string.</value>
    public static HashSet<int> SafeAsciiChars { get; } = new(Enumerable.Range(1, 127).Except(UnsafeAsciiChars));

    /// <summary>
    /// Gets a collection of integers representing the ASCII characters deemed safe as the leading character in a string.
    /// </summary>
    /// <value>The collection of integers representing the ASCII characters deemed safe as the leading character in a string.</value>
    public static HashSet<int> SafeAsciiInitChars { get; } = new(Enumerable.Range(1, 127).Except(UnsafeAsciiInitChars));
}