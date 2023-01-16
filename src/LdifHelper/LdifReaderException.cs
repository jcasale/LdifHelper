namespace LdifHelper;

using System;
using System.Globalization;
using System.Runtime.Serialization;

/// <summary>
/// Represents an LDIF reader exception.
/// </summary>
[Serializable]
public class LdifReaderException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LdifReaderException" /> class.
    /// </summary>
    public LdifReaderException()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LdifReaderException" /> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    public LdifReaderException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LdifReaderException" /> class.
    /// </summary>
    /// <param name="format">The exception message to format.</param>
    /// <param name="args">The format arguments.</param>
    public LdifReaderException(string format, params object[] args)
        : base(string.Format(CultureInfo.CurrentCulture, format, args))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LdifReaderException" /> class.
    /// </summary>
    /// <param name="message">The exception message.</param>
    /// <param name="innerException">The inner exception.</param>
    public LdifReaderException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LdifReaderException" /> class.
    /// </summary>
    /// <param name="format">The exception message to format.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="args">The format arguments.</param>
    public LdifReaderException(string format, Exception innerException, params object[] args)
        : base(string.Format(CultureInfo.CurrentCulture, format, args), innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LdifReaderException"></see> class with serialized data.
    /// </summary>
    /// <param name="serializationInfo">The <see cref="SerializationInfo"></see> that holds the serialized object data about the exception being thrown.</param>
    /// <param name="streamingContext">The <see cref="StreamingContext"></see> that contains contextual information about the source or destination.</param>
    /// <exception cref="NotImplementedException"></exception>
    protected LdifReaderException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
    }
}