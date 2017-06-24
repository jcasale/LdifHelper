// -----------------------------------------------------------------------
//  <copyright file="LdifReaderException.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper
{
    using System;
    using System.Globalization;

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
    }
}