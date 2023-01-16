namespace LdifHelper
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Represents constants used by the assembly.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Gets a REGEX that splits a distinguished name on unescaped commas.
        /// </summary>
        /// <value>The REGEX that splits a distinguished name on unescaped commas.</value>
        public static Regex DistinguishedNameRegex { get; } = new Regex(@"(?<=[^\\]),(?=\w+=)", RegexOptions.Compiled);

        /// <summary>
        /// Gets the maximum line length before a wrap is required.
        /// </summary>
        /// <value>The maximum line length before a wrap is required.</value>
        public static int MaxLineLength { get; } = 76;

        /// <summary>
        /// Gets a collection of integers representing the ASCII characters deemed unsafe as the non leading character in a string.
        /// </summary>
        /// <value>The collection of integers representing the ASCII characters deemed unsafe as the non leading character in a string.</value>
        public static HashSet<int> UnsafeAsciiChars { get; } = new HashSet<int>(new[] { 0, 10, 13 });

        /// <summary>
        /// Gets a collection of integers representing the ASCII characters deemed unsafe as the leading character in a string.
        /// </summary>
        /// <value>The collection of integers representing the ASCII characters deemed unsafe as the leading character in a string.</value>
        public static HashSet<int> UnsafeAsciiInitChars { get; } = new HashSet<int>(new[] { 0, 10, 13, 32, 58, 60 });

        /// <summary>
        /// Gets a collection of integers representing the ASCII characters deemed safe as the non leading character in a string.
        /// </summary>
        /// <value>The collection of integers representing the ASCII characters deemed safe as the non leading character in a string.</value>
        public static HashSet<int> SafeAsciiChars { get; } = new HashSet<int>(Enumerable.Range(1, 127).Except(UnsafeAsciiChars));

        /// <summary>
        /// Gets a collection of integers representing the ASCII characters deemed safe as the leading character in a string.
        /// </summary>
        /// <value>The collection of integers representing the ASCII characters deemed safe as the leading character in a string.</value>
        public static HashSet<int> SafeAsciiInitChars { get; } = new HashSet<int>(Enumerable.Range(1, 127).Except(UnsafeAsciiInitChars));
    }
}