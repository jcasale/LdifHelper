namespace LdifHelper;

using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Represents an LDIF attribute type and values.
/// </summary>
#pragma warning disable CA1711
public class LdifAttribute : IReadOnlyList<object>
#pragma warning restore CA1711
{
    /// <summary>
    /// Represents the attribute values.
    /// </summary>
    private readonly List<object> attributeValues;

    /// <summary>
    /// Initializes a new instance of the <see cref="LdifAttribute"/> class.
    /// </summary>
    /// <param name="attributeType">The attribute type.</param>
    /// <param name="attributeValues">The attribute values.</param>
    public LdifAttribute(string attributeType, IEnumerable<object> attributeValues)
    {
        if (string.IsNullOrWhiteSpace(attributeType))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(attributeType));
        }

        this.AttributeType = attributeType;
        this.attributeValues = attributeValues is null ? new List<object>() : new List<object>(attributeValues);
    }

    /// <summary>
    /// Gets the attribute type.
    /// </summary>
    /// <value>The attribute type.</value>
    public string AttributeType { get; }

    /// <summary>
    /// Gets the attribute values.
    /// </summary>
    /// <value>The attribute values.</value>
    public IEnumerable<object> AttributeValues => this.attributeValues;

    /// <summary>
    /// Gets the number of values contained in the <see cref="LdifAttribute"/>.
    /// </summary>
    /// <value>The number of values contained in the <see cref="LdifAttribute"/>.</value>
    public int Count => this.attributeValues.Count;

    /// <summary>
    /// Gets the value at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <returns>The value at the specified index.</returns>
    public object this[int index] => this.attributeValues[index];

    /// <summary>
    /// Gets an enumerable that can be used to iterate through the collection of attribute values.
    /// </summary>
    /// <returns>An enumerable that can be used to iterate through the collection of attribute values.</returns>
    public IEnumerator<object> GetEnumerator() => this.attributeValues.GetEnumerator();

    /// <summary>
    /// Gets an enumerable that can be used to iterate through the collection of attribute values.
    /// </summary>
    /// <returns>An enumerable that can be used to iterate through the collection of attribute values.</returns>
    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    /// <summary>
    /// Returns a string that represents the current object.
    /// </summary>
    /// <returns>A string that represents the current object.</returns>
    public override string ToString() => $"{nameof(LdifAttribute)}<{this.AttributeType}>";
}