// -----------------------------------------------------------------------
//  <copyright file="ExtensionTests.cs" company="Active Netwerx">
//  Copyright (c) Active Netwerx. All rights reserved.
//  </copyright>
//  <author>Joseph L. Casale</author>
// -----------------------------------------------------------------------

namespace LdifHelper.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using Xunit;

    /// <summary>
    /// Represents LDIF extension tests.
    /// </summary>
    public class ExtensionTests
    {
        /// <summary>
        /// Ensures the extension method rejects an empty string key.
        /// </summary>
        [Fact]
        public void AddOrAppendEmptyKeyThrows()
        {
            Dictionary<string, List<object>> input = new Dictionary<string, List<object>>();

            Assert.Throws<ArgumentOutOfRangeException>(() => input.AddOrAppend(string.Empty, new object[] { "value" }));
        }

        /// <summary>
        /// Ensures the extension method correctly adds a new key and appends a new value.
        /// </summary>
        [Fact]
        public void AddOrAppendIsValid()
        {
            Dictionary<string, List<object>> input = new Dictionary<string, List<object>>();

            Assert.Empty(input.Keys);

            input.AddOrAppend("keyA", new object[] { "value" });

            Assert.Equal(1, input.Keys.Count);
            Assert.Equal(1, input["keyA"].Count);

            input.AddOrAppend("keyA", new object[] { "value" });

            Assert.Equal(1, input.Keys.Count);
            Assert.Equal(2, input["keyA"].Count);

            input.AddOrAppend("keyB", new object[] { "value" });

            Assert.Equal(2, input.Keys.Count);
            Assert.Equal(1, input["keyB"].Count);
        }

        /// <summary>
        /// Ensures the extension method rejects null input.
        /// </summary>
        [Fact]
        public void AddOrAppendNullInputThrows()
        {
            Dictionary<string, List<object>> input = null;

            Assert.Throws<ArgumentNullException>(() => input.AddOrAppend("key", new object[] { "value" }));
        }

        /// <summary>
        /// Ensures the extension method rejects a null key.
        /// </summary>
        [Fact]
        public void AddOrAppendNullKeyThrows()
        {
            Dictionary<string, List<object>> input = new Dictionary<string, List<object>>();

            Assert.Throws<ArgumentNullException>(() => input.AddOrAppend(null, new object[] { "value" }));
        }

        /// <summary>
        /// Ensures the extension method rejects a null value.
        /// </summary>
        [Fact]
        public void AddOrAppendNullValueThrows()
        {
            Dictionary<string, List<object>> input = new Dictionary<string, List<object>>();

            Assert.Throws<ArgumentNullException>(() => input.AddOrAppend("key", null));
        }

        /// <summary>
        /// Ensures the extension method rejects a white space string key.
        /// </summary>
        [Fact]
        public void AddOrAppendWhiteSpaceKeyThrows()
        {
            Dictionary<string, List<object>> input = new Dictionary<string, List<object>>();

            Assert.Throws<ArgumentOutOfRangeException>(() => input.AddOrAppend(" ", new object[] { "value" }));
        }

        /// <summary>
        /// Ensures the method rejects an empty attribute type.
        /// </summary>
        [Fact]
        public void GetValueSpecEmptyTypeThrows() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => Extensions.GetValueSpec(string.Empty, "value"));

        /// <summary>
        /// Ensures the method rejects a null attribute type.
        /// </summary>
        [Fact]
        public void GetValueSpecNullTypeThrows() =>
            Assert.Throws<ArgumentNullException>(() => Extensions.GetValueSpec(null, "value"));

        /// <summary>
        /// Ensures the method rejects a null attribute value.
        /// </summary>
        [Fact]
        public void GetValueSpecNullValueThrows() =>
            Assert.Throws<ArgumentNullException>(() => Extensions.GetValueSpec("type", null));

        /// <summary>
        /// Ensures the method rejects a null attribute value.
        /// </summary>
        [Fact]
        public void GetValueSpecUnknownValueTypeThrows() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => Extensions.GetValueSpec("type", 42));

        /// <summary>
        /// Ensures the method base64 encodes a binary attribute value.
        /// </summary>
        [Fact]
        public void GetValueSpecBinaryDataIsEncoded()
        {
            // Arrange.
            const string attributeType = "sn";
            const string attributeValue = "value";
            byte[] bytes = Encoding.UTF8.GetBytes(attributeValue);

            // Act.
            string sut = Extensions.GetValueSpec(attributeType, bytes);

            // Assert.
            Assert.Equal(attributeType, sut.Substring(0, attributeType.Length));

            byte[] valueBytes = Convert.FromBase64String(sut.Substring(attributeType.Length + 3));
            string valueString = Encoding.UTF8.GetString(valueBytes);
            Assert.Equal(attributeValue, valueString);
        }

        /// <summary>
        /// Ensures the method base64 encodes an attribute value with an unsafe initial character.
        /// </summary>
        [Fact]
        public void GetValueSpecUnSafeInitCharIsEncoded()
        {
            // Arrange.
            const string attributeType = "givenName";
            const string attributeValue = "Émilie";

            // Act.
            string sut = Extensions.GetValueSpec(attributeType, attributeValue);

            // Assert.
            Assert.Equal(attributeType, sut.Substring(0, attributeType.Length));

            byte[] valueBytes = Convert.FromBase64String(sut.Substring(attributeType.Length + 3));
            string valueString = Encoding.UTF8.GetString(valueBytes);
            Assert.Equal(attributeValue, valueString);
        }

        /// <summary>
        /// Ensures the method base64 encodes an attribute value with an unsafe initial character.
        /// </summary>
        [Fact]
        public void GetValueSpecUnSafeStringIsEncoded()
        {
            // Arrange.
            const string attributeType = "sn";
            const string attributeValue = "Châtelet";

            // Act.
            string sut = Extensions.GetValueSpec(attributeType, attributeValue);

            Assert.Equal(attributeType, sut.Substring(0, attributeType.Length));

            // Assert.
            byte[] valueBytes = Convert.FromBase64String(sut.Substring(attributeType.Length + 3));
            string valueString = Encoding.UTF8.GetString(valueBytes);
            Assert.Equal(attributeValue, valueString);
        }

        /// <summary>
        /// Ensures the method rejects a white space attribute type.
        /// </summary>
        [Fact]
        public void GetValueSpecWhiteSpaceTypeThrows() =>
            Assert.Throws<ArgumentOutOfRangeException>(() => Extensions.GetValueSpec(" ", "value"));

        /// <summary>
        /// Ensures the extension method identifies some possible cases.
        /// </summary>
        [Fact]
        public void IsSafeInitCharIsValid()
        {
            Assert.True(string.Empty.IsSafeInitChar());
            Assert.True("a".IsSafeInitChar());
            Assert.False(" ".IsSafeInitChar());
            Assert.False("Émilie".IsSafeInitChar());
        }

        /// <summary>
        /// Ensures the extension method rejects a null attribute value.
        /// </summary>
        [Fact]
        public void IsSafeInitCharNullValueThrows()
        {
            const string value = null;

            Assert.Throws<ArgumentNullException>(() => value.IsSafeInitChar());
        }

        /// <summary>
        /// Ensures the extension method identifies some possible cases.
        /// </summary>
        [Fact]
        public void IsSafeStringIsValid()
        {
            Assert.True(string.Empty.IsSafeString());
            Assert.True("ascii chars".IsSafeString());
            Assert.False("EndsWithSpace ".IsSafeString());
            Assert.False("Châtelet".IsSafeString());
        }

        /// <summary>
        /// Ensures the extension method rejects a null attribute value.
        /// </summary>
        [Fact]
        public void IsSafeStringNullValueThrows()
        {
            const string value = null;

            Assert.Throws<ArgumentNullException>(() => value.IsSafeString());
        }

        /// <summary>
        /// Ensures the extension method correctly base64 encodes a value.
        /// </summary>
        [Fact]
        public void ToBase64BytesIsValid()
        {
            // Arrange.
            const string value = "value";
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(value);

            // Act.
            string encoded = utf16Bytes.ToBase64();

            // Assert.
            byte[] bytes = Convert.FromBase64String(encoded);
            string decoded = Encoding.Unicode.GetString(bytes);
            Assert.Equal(value, decoded);
        }

        /// <summary>
        /// Ensures the extension method rejects a null value.
        /// </summary>
        [Fact]
        public void ToBase64BytesNullValueThrows()
        {
            const byte[] value = null;

            Assert.Throws<ArgumentNullException>(() => value.ToBase64());
        }

        /// <summary>
        /// Ensures the extension method correctly base64 encodes a value.
        /// </summary>
        [Fact]
        public void ToBase64StringIsValid()
        {
            // Arrange.
            const string value = "value";

            // Act.
            string sut = value.ToBase64();

            // Assert.
            byte[] bytes = Convert.FromBase64String(sut);
            string decoded = Encoding.UTF8.GetString(bytes);
            Assert.Equal(value, decoded);
        }

        /// <summary>
        /// Ensures the extension method rejects a null value.
        /// </summary>
        [Fact]
        public void ToBase64StringNullValueThrows()
        {
            const string value = null;

            Assert.Throws<ArgumentNullException>(() => value.ToBase64());
        }

        /// <summary>
        /// Ensures the extension method rejects a null value.
        /// </summary>
        [Fact]
        public void WrapNullValueThrows()
        {
            const string value = null;

            Assert.Throws<ArgumentNullException>(() => value.Wrap());
        }

        /// <summary>
        /// Ensures the extension method correctly wraps a long line.
        /// </summary>
        [Fact]
        public void WrapColumnLengthIsValid()
        {
            // Arrange.
            const string value =
                "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Phasellus convallis" +
                " et erat at mollis. Nullam in risus laoreet, pharetra leo a, volutpat massa. Cras" +
                " quis sodales velit. In sit amet augue gravida, sagittis dui a, placerat nunc." +
                " Fusce non nisi vel orci sagittis elementum. Praesent elit nulla, elementum sed" +
                " sem a, semper dictum arcu. Duis luctus arcu id arcu scelerisque pharetra. Nunc" +
                " a elementum felis, quis auctor diam.";

            // Act.
            string sut = value.Wrap();

            // Assert.
            string[] lines = sut.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

            Assert.NotEmpty(lines);

            foreach (string line in lines)
            {
                Assert.True(line.Length <= Constants.MaxLineLength);
            }
        }

        /// <summary>
        /// Ensures the <see cref="Extensions.Wrap"/> extension method correctly processes strings of various lengths.
        /// </summary>
        [Fact]
        public void WrapIsValid()
        {
            var generator = new Random();

            for (int i = 0; i < 4000; i++)
            {
                // Arrange.
                var bytes = new byte[i];
                generator.NextBytes(bytes);
                var data = $"wrap:: {bytes.ToBase64()}";

                // Act.
                var sut = data.Wrap().Split(new[] { Environment.NewLine }, StringSplitOptions.None);
                var stringBuilder = new StringBuilder();
                foreach (string line in sut)
                {
                    stringBuilder.Append(line.TrimStart());
                }

                // Assert.
                Assert.Equal(data, stringBuilder.ToString());
            }
        }
    }
}